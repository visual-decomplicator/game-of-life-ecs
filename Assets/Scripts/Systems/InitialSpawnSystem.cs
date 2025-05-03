using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems {

    public partial class InitIputSystem : SystemBase {
        protected override void OnCreate() {
            RequireForUpdate<InitialSpawnerComponent>();
            RequireForUpdate<GameStateComponent>();
            
        }

        protected override void OnStartRunning() {
            InitInputUI.Instance.OnStartButtonClick += OnStartButtonClick;
        }

        private void OnStartButtonClick(object sender, InitInputUI.InitSettings e) {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (gameState.State != GameState.InputConfig) {
                return;
            }
            
            var spawnSettings = SystemAPI.GetSingleton<InitialSpawnerComponent>();
            var stepSettings = SystemAPI.GetSingleton<CommonStepComponent>();
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            Entity commonEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            commonSettings.MaxGridSize = e.GridSize;
            spawnSettings.EntitiesCount = e.EntitiesCount;
            stepSettings.StepDelay = e.StepDelay;
            SystemAPI.SetSingleton(spawnSettings);
            SystemAPI.SetSingleton(new GameStateComponent(){ State = GameState.Prepare });
            SystemAPI.SetSingleton(stepSettings);
            SystemAPI.SetSingleton(commonSettings);
            SystemAPI.SetComponentEnabled<NeedFitCameraComponent>(commonEntity, true);
        }

        protected override void OnUpdate() {
            
        }
    }
    
    public partial class PrepareCompleteSystem : SystemBase {
        protected override void OnCreate() {
            RequireForUpdate<GameStateComponent>();
            
        }
        
        protected override void OnUpdate() {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (gameState.State != GameState.PreparationCompleted) {
                return;
            }
            
            LoaderUI.Instance.Hide();
            SystemAPI.SetSingleton(new GameStateComponent(){ State = GameState.Play });
        }
    }
    
    public partial struct InitialSpawnSystem : ISystem {
        private EntityArchetype _cellEntityArchetype;
        
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
            state.RequireForUpdate<InitialSpawnerComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<GameStateComponent>();
            
            _cellEntityArchetype = state.EntityManager.CreateArchetype(
                typeof(GridPositionComponent),
                typeof(CounterComponent),
                typeof(VisualEntityComponent),
                typeof(IsAliveComponent),
                typeof(NeedChangeVisualComponent)
            );
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (gameState.State != GameState.Prepare) {
                return;
            }
            
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            var commonSettingsEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (initSpawner, random, entity) in SystemAPI
                         .Query<RefRW<InitialSpawnerComponent>, RefRW<RandomComponent>>()
                         .WithEntityAccess()
                     ) {
                if (initSpawner.ValueRO.EntitiesCount <= 0) {
                    ecb.RemoveComponent<InitialSpawnerComponent>(entity);
                    SystemAPI.SetSingleton(new GameStateComponent(){State = GameState.PreparationCompleted});
                    continue;
                }
                
                var spawnedEntitiesMap = new NativeHashMap<int2, Entity>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                var cellPositionsToSpawn = new NativeHashSet<int2>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                var cellCounterMap = new NativeHashMap<int2, int>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                for (int i = 0; i < initSpawner.ValueRO.SpawnBatchSize; i++) {
                    int2 position = new int2(
                        random.ValueRW.Value.NextInt(-commonSettings.MaxGridSize.x,
                            commonSettings.MaxGridSize.x),
                        random.ValueRW.Value.NextInt(-commonSettings.MaxGridSize.y,
                            commonSettings.MaxGridSize.y)
                    );
                    cellPositionsToSpawn.Add(position);
                }
                initSpawner.ValueRW.EntitiesCount -= initSpawner.ValueRO.SpawnBatchSize;

                foreach (var gridPosition in SystemAPI.Query<GridPositionComponent>().WithAll<IsAliveComponent>()) {
                    cellPositionsToSpawn.Remove(gridPosition.Position);
                }

                foreach (var (gridPosition, visualEntity, deadEntity) in SystemAPI
                             .Query<GridPositionComponent, VisualEntityComponent>()
                             .WithNone<IsAliveComponent>()
                             .WithEntityAccess()
                         ) {
                    if (!cellPositionsToSpawn.Contains(gridPosition.Position)) {
                        continue;
                    }
                    ecb.SetComponentEnabled<IsAliveComponent>(deadEntity, true);
                    ecb.SetComponentEnabled<NeedChangeVisualComponent>(deadEntity, true);
                    GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, gridPosition.Position, 1);
                    cellPositionsToSpawn.Remove(gridPosition.Position);
                }

                foreach (var position in cellPositionsToSpawn) {
                    GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, position, 1);
                    Entity spawned = ecb.CreateEntity(_cellEntityArchetype);
                    ecb.SetComponent(spawned, new GridPositionComponent { Position = position });
                    ecb.SetComponent(spawned, new VisualEntityComponent(){Entity = Entity.Null});
                    spawnedEntitiesMap.Add(position, spawned);
                }
                
                foreach (var (counter, gridPosition) in SystemAPI
                             .Query<RefRW<CounterComponent>, RefRO<GridPositionComponent>>()
                        ) {
                    if (cellCounterMap.TryGetValue(gridPosition.ValueRO.Position, out int value)) {
                        counter.ValueRW.Value += value;
                        cellCounterMap.Remove(gridPosition.ValueRO.Position);
                    }
                }
                
                foreach (var counterDeltaMap in cellCounterMap) {
                    if (spawnedEntitiesMap.TryGetValue(counterDeltaMap.Key, out Entity item)) {
                        ecb.SetComponent(item, new CounterComponent() { Value = counterDeltaMap.Value });
                        continue;
                    }

                    Entity spawned = ecb.CreateEntity(_cellEntityArchetype);
                    ecb.SetComponent(spawned, new GridPositionComponent { Position = counterDeltaMap.Key });
                    ecb.SetComponent(spawned, new CounterComponent() { Value = counterDeltaMap.Value });
                    ecb.SetComponent(spawned, new VisualEntityComponent(){Entity = Entity.Null});
                    ecb.SetComponentEnabled<IsAliveComponent>(spawned, false);
                    ecb.SetComponentEnabled<NeedChangeVisualComponent>(spawned, false);
                }
                
                spawnedEntitiesMap.Dispose();
                cellCounterMap.Dispose();
                cellPositionsToSpawn.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}