using Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems {
    public partial class FitCameraSystem : SystemBase {
        protected override void OnUpdate() {
            Entity commonEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            if (!SystemAPI.IsComponentEnabled<NeedFitCameraComponent>(commonEntity)) {
                return;
            }

            var commonSettings = SystemAPI.GetSingleton<CommonSettingsComponent>();
            var bounds = new Bounds(Vector3.zero, new Vector3(
                commonSettings.MaxGridSize.x * 2 + commonSettings.MaxGridSize.x * 2 * commonSettings.GridGap,
                0,
                commonSettings.MaxGridSize.y * 2 + + commonSettings.MaxGridSize.y * 2 * commonSettings.GridGap
            ));
            foreach (var transform in SystemAPI.Query<LocalTransform>().WithAll<CellVisualComponent>()) 
            {
                bounds.Encapsulate(transform.Position);
            }
            
            CameraController.Instance.FitToBounds(bounds);
            SystemAPI.SetComponentEnabled<NeedFitCameraComponent>(commonEntity, false);
        }
    }
}