using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems {
    public partial struct GameOfLifeSystem : ISystem {
        
        private const int CellCounterMapInitCapacity = 1000;
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
            state.RequireForUpdate<CommonStepComponent>();
        }

        private void AddCountAtPosition(ref NativeHashMap<int2, int> cellCounterMap, int2 position, int count) {
            if (cellCounterMap.TryGetValue(position, out var value)) {
                value += count;
                cellCounterMap[position] = value;
                return;
            }
            
            cellCounterMap.Add(position, count);
        }
        
        private void AddCountAroundCell(ref NativeHashMap<int2, int> cellCounterMap, int2 position, int count) {
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
            var cellCounterMap = new NativeHashMap<int2, int>(CellCounterMapInitCapacity, Allocator.Temp);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (gridPosition, visualEntity, entity) in SystemAPI
                         .Query<GridPositionComponent, VisualEntityComponent>()
                         .WithAll<NeedSetIsAliveComponent>()
                         .WithNone<IsAliveComponent>()
                         .WithEntityAccess()
                     ) {
                AddCountAroundCell(ref cellCounterMap, gridPosition.Position, 1);
                ecb.AddComponent<IsAliveComponent>(entity);
                if (visualEntity.Entity != Entity.Null) {
                    ecb.DestroyEntity(visualEntity.Entity);
                }
                Entity spawned = ecb.Instantiate(commonSettings.AliveVisualPrefab);
                float3 cellPosition = new float3(
                    gridPosition.Position.x + commonSettings.GridGap * gridPosition.Position.x, 0,
                    gridPosition.Position.y + commonSettings.GridGap * gridPosition.Position.y
                    );
                ecb.SetComponent(spawned, new LocalTransform() {
                    Position = cellPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                ecb.SetComponent(entity, new VisualEntityComponent() { Entity = spawned });
                ecb.RemoveComponent<NeedSetIsAliveComponent>(entity);
            }
            
            foreach (var (gridPosition, visualEntity, entity) in SystemAPI
                         .Query<GridPositionComponent, VisualEntityComponent>()
                         .WithAll<NeedRemoveIsAliveComponent, IsAliveComponent>()
                         .WithEntityAccess()
                    ) {
                AddCountAroundCell(ref cellCounterMap, gridPosition.Position, -1);
                ecb.RemoveComponent<IsAliveComponent>(entity);
                if (visualEntity.Entity != Entity.Null) {
                    ecb.DestroyEntity(visualEntity.Entity);
                }
                
                Entity spawned = ecb.Instantiate(commonSettings.DeadVisualPrefab);
                float3 cellPosition = new float3(
                    gridPosition.Position.x + commonSettings.GridGap * gridPosition.Position.x, 0,
                    gridPosition.Position.y + commonSettings.GridGap * gridPosition.Position.y
                );
                ecb.SetComponent(spawned, new LocalTransform() {
                    Position = cellPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                ecb.SetComponent(entity, new VisualEntityComponent() { Entity = spawned });
                
                ecb.RemoveComponent<NeedRemoveIsAliveComponent>(entity);
            }

            foreach (var (gridPosition, counter) in SystemAPI
                         .Query<RefRO<GridPositionComponent>, RefRW<CounterComponent>>()
                     ) {
                if (cellCounterMap.TryGetValue(gridPosition.ValueRO.Position, out var value)) {
                    counter.ValueRW.Value += value;
                    cellCounterMap.Remove(gridPosition.ValueRO.Position);
                }
            }

            foreach (var kvPair in cellCounterMap) {
                Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                ecb.SetComponent(spawned, new GridPositionComponent { Position = kvPair.Key });
                ecb.SetComponent(spawned, new CounterComponent { Value = kvPair.Value });
                
                Entity spawnedVisual = ecb.Instantiate(commonSettings.DeadVisualPrefab);
                float3 cellPosition = new float3(
                    kvPair.Key.x + commonSettings.GridGap * kvPair.Key.x, 0,
                    kvPair.Key.y + commonSettings.GridGap * kvPair.Key.y
                );
                ecb.SetComponent(spawnedVisual, new LocalTransform() {
                    Position = cellPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                ecb.SetComponent(spawned, new VisualEntityComponent() { Entity = spawnedVisual });
            }

            cellCounterMap.Dispose();

            foreach (var (counter, visualEntity, entity) in SystemAPI
                         .Query<CounterComponent, VisualEntityComponent>()
                         .WithNone<IsAliveComponent, NeedSetIsAliveComponent>()
                         .WithEntityAccess()
                     ) {
                if (counter.Value == 3) {
                    ecb.AddComponent<NeedSetIsAliveComponent>(entity);
                    continue;
                }

                if (counter.Value <= 0) {
                    if (visualEntity.Entity != Entity.Null) {
                        ecb.DestroyEntity(visualEntity.Entity);
                    }
                    ecb.DestroyEntity(entity);
                }
            }

            foreach (var (counter, entity) in SystemAPI
                         .Query<CounterComponent>()
                         .WithAll<IsAliveComponent>()
                         .WithNone<NeedRemoveIsAliveComponent>()
                         .WithEntityAccess()
                     ) {
                if (counter.Value <= 1 || counter.Value > 3) {
                    ecb.AddComponent<NeedRemoveIsAliveComponent>(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}