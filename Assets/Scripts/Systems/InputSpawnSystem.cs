using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems {
    public partial class InputSpawnSystem : SystemBase {
        protected override void OnCreate() {
            this.World.GetExistingSystemManaged<CustomInputSystem>().OnClick += OnClick;
            this.RequireForUpdate<CommonSettingsComponent>();
        }

        private void OnClick(object sender, CustomInputSystem.ClickData e) {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (gameState.State != GameState.Play) {
                return;
            }
            
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            int2 cellCoordinates = new int2(
                (int)math.round(e.Position.x / (1f + commonSettings.GridGap)),
                (int)math.round(e.Position.y / (1f + commonSettings.GridGap))
            );
            
            var ecb = this.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            bool entityFound = false;
            foreach (var gridPosition in SystemAPI
                         .Query<GridPositionComponent>()
                         .WithAll<IsAliveComponent>()
                     ) {
                if (gridPosition.Position.Equals(cellCoordinates)) {
                    entityFound = true;
                    break;
                }
            }

            if (entityFound) return;
            
            var cellCounterMap = new NativeHashMap<int2, int>(8, Allocator.Temp);
            foreach (var (gridPosition, visualEntity, entity) in SystemAPI
                         .Query<GridPositionComponent, VisualEntityComponent>()
                         .WithNone<IsAliveComponent>()
                         .WithEntityAccess()
                    ) {
                if (!gridPosition.Position.Equals(cellCoordinates)) {
                    continue;
                }
                
                entityFound = true;
                ecb.SetComponentEnabled<IsAliveComponent>(entity, true);
                GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, gridPosition.Position, 1);
                ecb.SetComponentEnabled<NeedChangeVisualComponent>(entity, true);
                break;
            }

            if (!entityFound) {
                GameOfLifeSystem.AddCountAroundCell(ref cellCounterMap, cellCoordinates, 1);
                Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                ecb.SetComponent(spawned, new GridPositionComponent { Position = cellCoordinates });
                ecb.SetComponentEnabled<IsAliveComponent>(spawned, true);
                ecb.AddComponent(spawned, new VisualEntityComponent(){Entity = Entity.Null});
                ecb.AddComponent<NeedChangeVisualComponent>(spawned);
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
                Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                ecb.SetComponent(spawned, new GridPositionComponent { Position = counterDeltaMap.Key });
                ecb.SetComponent(spawned, new CounterComponent() { Value = counterDeltaMap.Value });
                ecb.AddComponent(spawned, new VisualEntityComponent(){Entity = Entity.Null});
                ecb.AddComponent<NeedChangeVisualComponent>(spawned);
            }
        }

        protected override void OnUpdate() {
            
        }
    }
}