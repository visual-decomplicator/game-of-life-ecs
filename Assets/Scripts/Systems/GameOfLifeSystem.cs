using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems {
    public partial struct GameOfLifeSystem : ISystem {
        
        private const int CellCounterMapInitCapacity = 1000;
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
            state.RequireForUpdate<CommonStepComponent>();
        }

        private static void AddCountAtPosition(ref NativeHashMap<int2, int> cellCounterMap, int2 position, int count) {
            if (cellCounterMap.TryGetValue(position, out var value)) {
                value += count;
                cellCounterMap[position] = value;
                return;
            }
            
            cellCounterMap.Add(position, count);
        }
        
        public static void AddCountAroundCell(ref NativeHashMap<int2, int> cellCounterMap, int2 position, int count) {
            AddCountAtPosition(ref cellCounterMap, new int2(position.x-1, position.y-1), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x, position.y-1), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x+1, position.y-1), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x-1, position.y), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x+1, position.y), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x-1, position.y+1), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x, position.y+1), count);
            AddCountAtPosition(ref cellCounterMap, new int2(position.x+1, position.y+1), count);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var commonStep in SystemAPI.Query<RefRW<CommonStepComponent>>()) {
                commonStep.ValueRW.Timer += SystemAPI.Time.DeltaTime;
                if (commonStep.ValueRO.Timer < commonStep.ValueRO.StepDelay) {
                    return;
                }
                commonStep.ValueRW.Timer = 0;
            }
            
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            Entity commonSettingsEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            var cellCounterMap = new NativeHashMap<int2, int>(CellCounterMapInitCapacity, Allocator.Temp);
            var cellsToDeleteMap = new NativeHashSet<int2>(100, Allocator.Temp);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            SystemAPI.SetComponentEnabled<NeedFitCameraComponent>(commonSettingsEntity, true);
            
            foreach (var (counter, visualEntity, gridPosition, entity) in SystemAPI
                         .Query<CounterComponent, VisualEntityComponent, GridPositionComponent>()
                         .WithNone<IsAliveComponent>()
                         .WithEntityAccess()
                    ) {
                // New cell born when it have 3 neighbours.
                if (counter.Value == 3) {
                    AddCountAroundCell(ref cellCounterMap, gridPosition.Position, 1);
                    ecb.AddComponent<IsAliveComponent>(entity);
                    if (visualEntity.Entity != Entity.Null) {
                        ecb.DestroyEntity(visualEntity.Entity);
                        ecb.SetComponent(entity, new VisualEntityComponent(){Entity = Entity.Null});
                    }
                    continue;
                }

                // Cell must be removed if it have no neighbours.
                if (counter.Value <= 0) {
                    cellsToDeleteMap.Add(gridPosition.Position);
                }
            }

            foreach (var (counter, gridPosition, visualEntity, entity) in SystemAPI
                         .Query<CounterComponent, GridPositionComponent, VisualEntityComponent>()
                         .WithAll<IsAliveComponent>()
                         .WithEntityAccess()
                    ) {
                // Cells with 2 or 3 neighbours will survive.
                if (counter.Value == 2 || counter.Value == 3) {
                    continue;
                }
                
                // The rest of entities will be destroyed because of solitude or overpopulation.
                ecb.RemoveComponent<IsAliveComponent>(entity);
                AddCountAroundCell(ref cellCounterMap, gridPosition.Position, -1);
                if (visualEntity.Entity != Entity.Null) {
                    ecb.DestroyEntity(visualEntity.Entity);
                    ecb.SetComponent(entity, new VisualEntityComponent(){Entity = Entity.Null});
                }
            }
            
            // Apply new neighbour counters to existed cells.
            foreach (var (gridPosition, counter) in SystemAPI
                         .Query<RefRO<GridPositionComponent>, RefRW<CounterComponent>>()
                     ) {
                if (cellCounterMap.TryGetValue(gridPosition.ValueRO.Position, out var value)) {
                    counter.ValueRW.Value += value;
                    cellCounterMap.Remove(gridPosition.ValueRO.Position);
                    if (counter.ValueRO.Value > 0) {
                        cellsToDeleteMap.Remove(gridPosition.ValueRO.Position);
                    }
                }
            }

            // Instantiate missing cells and apply counters to them.
            foreach (var kvPair in cellCounterMap) {
                Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                ecb.SetComponent(spawned, new GridPositionComponent { Position = kvPair.Key });
                ecb.SetComponent(spawned, new CounterComponent { Value = kvPair.Value });
            }
            
            foreach (var (gridPosition, visualEntity, entity) in SystemAPI
                         .Query<GridPositionComponent, VisualEntityComponent>()
                         .WithEntityAccess()
                    ) {
                if (!cellsToDeleteMap.Contains(gridPosition.Position)) {
                    continue;
                }

                if (visualEntity.Entity != Entity.Null) {
                    ecb.DestroyEntity(visualEntity.Entity);
                }
                ecb.DestroyEntity(entity);
            }

            cellCounterMap.Dispose();
            cellsToDeleteMap.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}