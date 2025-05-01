using Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems {
    public partial class FitCameraSystem : SystemBase {
        protected override void OnUpdate() {
            Entity commonEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            if (!SystemAPI.IsComponentEnabled<NeedFitCameraComponent>(commonEntity) || 
                SystemAPI.IsComponentEnabled<ManualCameraPositioningComponent>(commonEntity)) {
                return;
            }

            bool firstPoint = true;
            Bounds bounds = default;
        
            foreach (var transform in SystemAPI.Query<LocalTransform>().WithAll<CellVisualComponent>()) 
            {
                if (firstPoint)
                {
                    // Initialize bounds with first point
                    bounds = new Bounds(transform.Position, Vector3.zero);
                    firstPoint = false;
                }
                else
                {
                    bounds.Encapsulate(transform.Position);
                }
            }

            if (!firstPoint) // Only if we found at least one cell
            {
                CameraController.Instance.FitToBounds(bounds);
            }
        
            SystemAPI.SetComponentEnabled<NeedFitCameraComponent>(commonEntity, false);
        }
    }
}