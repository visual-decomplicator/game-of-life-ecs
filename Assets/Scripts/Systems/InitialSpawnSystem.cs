using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems {
    public partial struct InitialSpawnSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
            state.RequireForUpdate<InitialSpawnerComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (initSpawner, random, entity) in SystemAPI
                         .Query<RefRW<InitialSpawnerComponent>, RefRW<RandomComponent>>()
                         .WithEntityAccess()
                     ) {
                if (initSpawner.ValueRO.EntitiesCount <= 0) {
                    ecb.RemoveComponent<InitialSpawnerComponent>(entity);
                    continue;
                }
                
                var spawnedEntitiesMap = new NativeHashMap<int2, Entity>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                var cellPositionsToSpawn = new NativeHashSet<int2>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                var cellCounterMap = new NativeHashMap<int2, int>(initSpawner.ValueRO.SpawnBatchSize, Allocator.Temp);
                for (int i = 0; i < initSpawner.ValueRO.SpawnBatchSize; i++) {
                    int2 position = new int2(
                        random.ValueRW.Value.NextInt(-initSpawner.ValueRO.MaxGridSize.x,
                            initSpawner.ValueRO.MaxGridSize.x),
                        random.ValueRW.Value.NextInt(-initSpawner.ValueRO.MaxGridSize.y,
                            initSpawner.ValueRO.MaxGridSize.y)
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
                    ecb.AddComponent<IsAliveComponent>(deadEntity);
                    if (visualEntity.Entity != Entity.Null) {
                        ecb.DestroyEntity(visualEntity.Entity);
                        ecb.SetComponent(deadEntity, new VisualEntityComponent() {Entity = Entity.Null});
                    }
                    GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, gridPosition.Position, 1);
                    cellPositionsToSpawn.Remove(gridPosition.Position);
                }

                foreach (var position in cellPositionsToSpawn) {
                    GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, position, 1);
                    Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                    ecb.SetComponent(spawned, new GridPositionComponent { Position = position });
                    ecb.AddComponent<IsAliveComponent>(spawned);
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
                    
                    Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                    ecb.SetComponent(spawned, new GridPositionComponent { Position = counterDeltaMap.Key });
                    ecb.SetComponent(spawned, new CounterComponent() { Value = counterDeltaMap.Value });
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