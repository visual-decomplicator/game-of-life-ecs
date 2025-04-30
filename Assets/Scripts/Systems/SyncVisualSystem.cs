using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems {
    public partial struct SyncVisualSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (visualEntity, gridPosition, entity) in SystemAPI
                         .Query<VisualEntityComponent, GridPositionComponent>()
                         .WithAll<IsAliveComponent>()
                         .WithEntityAccess()
                     ) {
                if (visualEntity.Entity != Entity.Null) {
                    continue;
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
                ecb.SetComponent(entity, new VisualEntityComponent() {
                    Entity = spawned
                });
            }
            
            foreach (var (visualEntity, gridPosition, entity) in SystemAPI
                         .Query<VisualEntityComponent, GridPositionComponent>()
                         .WithNone<IsAliveComponent>()
                         .WithEntityAccess()
                    ) {
                if (visualEntity.Entity != Entity.Null) {
                    continue;
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
                ecb.SetComponent(entity, new VisualEntityComponent() {
                    Entity = spawned
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}