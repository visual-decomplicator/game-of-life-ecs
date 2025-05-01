using Components;
using Unity.Burst;
using Unity.Entities;

namespace Systems {
    public partial struct CleanupSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (visualEntity, entity) in SystemAPI
                         .Query<VisualEntityComponent>()
                         .WithNone<GridPositionComponent>()
                         .WithEntityAccess()
                     ) {
                if (visualEntity.Entity != Entity.Null) {
                    ecb.DestroyEntity(visualEntity.Entity);
                }
                ecb.RemoveComponent<VisualEntityComponent>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}