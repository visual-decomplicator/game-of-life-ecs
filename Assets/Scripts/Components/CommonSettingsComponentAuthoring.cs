using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    public class CommonSettingsComponentAuthoring : MonoBehaviour {
        public float GridGap;
        public GameObject CellPrefab;
        public GameObject DeadVisualPrefab;
        public GameObject AliveVisualPrefab;
        public float3 VisualCellRotation;
        public float StepDelay;
        
        private class CommonSettingsComponentBaker : Baker<CommonSettingsComponentAuthoring> {
            public override void Bake(CommonSettingsComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CommonSettingsComponent {
                    GridGap = authoring.GridGap,
                    CellPrefab = GetEntity(authoring.CellPrefab, TransformUsageFlags.None),
                    DeadVisualPrefab = GetEntity(authoring.DeadVisualPrefab, TransformUsageFlags.Dynamic),
                    AliveVisualPrefab = GetEntity(authoring.AliveVisualPrefab, TransformUsageFlags.Dynamic),
                    VisualCellRotation = quaternion.EulerXYZ(authoring.VisualCellRotation)
                });
                AddComponent(entity, new CommonStepComponent {
                    StepDelay = authoring.StepDelay
                });
                
                AddComponent<NeedFitCameraComponent>(entity);
                AddComponent<ManualCameraPositioningComponent>(entity);
                SetComponentEnabled<ManualCameraPositioningComponent>(entity, false);
            }
        }
    }
    
    public struct CommonSettingsComponent : IComponentData {
        public float GridGap;
        public Entity CellPrefab;
        public Entity DeadVisualPrefab;
        public Entity AliveVisualPrefab;
        public quaternion VisualCellRotation;
    }
    
    public struct CommonStepComponent : IComponentData {
        public float StepDelay;
        public float Timer;
    }
    
    public struct NeedFitCameraComponent : IComponentData, IEnableableComponent {}
    public struct ManualCameraPositioningComponent : IComponentData, IEnableableComponent {}
}