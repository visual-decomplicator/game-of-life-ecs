using Components;
using Unity.Entities;

namespace Systems {
    public partial class InputSpawnSystem : SystemBase {
        protected override void OnCreate() {
            this.World.GetExistingSystemManaged<CustomInputSystem>().OnClick += OnClick;
            this.RequireForUpdate<CommonSettingsComponent>();
        }

        private void OnClick(object sender, CustomInputSystem.ClickData e) {
            var ecb = this.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            bool entityFound = false;
            foreach (var (gridPosition, entity) in SystemAPI
                         .Query<GridPositionComponent>()
                         .WithEntityAccess()
                     ) {
                if (!gridPosition.Position.Equals(e.Position)) {
                    continue;
                }
                
                entityFound = true;
                if (!SystemAPI.HasComponent<IsAliveComponent>(entity) 
                    && !SystemAPI.HasComponent<NeedSetIsAliveComponent>(entity)
                    && !SystemAPI.HasComponent<NeedRemoveIsAliveComponent>(entity)) {
                    ecb.AddComponent<NeedSetIsAliveComponent>(entity);
                }
            }

            if (entityFound) return;
            
            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            Entity spawned = ecb.Instantiate(commonSettings.CellPrefab);
            ecb.SetComponent(spawned, new GridPositionComponent { Position = e.Position });
            ecb.AddComponent<NeedSetIsAliveComponent>(spawned);
        }

        protected override void OnUpdate() {
            
        }
    }
}