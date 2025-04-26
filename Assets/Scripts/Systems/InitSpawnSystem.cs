using Components;
using Unity.Burst;
using Unity.Entities;

namespace Systems {
    public partial struct InitSpawnSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CommonSettingsComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            foreach (var (initSpawn, entity) in SystemAPI.Query<InitialSpawnerComponent>().WithEntityAccess()) {
                ref CellCoordinates coordinates = ref initSpawn.Coordinates.Value;
                var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);
                for (int i = 0; i < coordinates.Coordinates.Length; i++) {
                    Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
                    ecb.SetComponent(spawned, new GridPositionComponent { Position = coordinates.Coordinates[i] });
                    ecb.AddComponent<NeedSetIsAliveComponent>(spawned);
                }
                ecb.RemoveComponent<InitialSpawnerComponent>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}