using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    public class CommonSettingsComponentAuthoring : MonoBehaviour {
        public float GridGap;
        public GameObject AliveVisualPrefab;
        public float3 VisualCellRotation;
        public float StepDelay;
        public int2 MaxGridSize;
        
        private class CommonSettingsComponentBaker : Baker<CommonSettingsComponentAuthoring> {
            public override void Bake(CommonSettingsComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CommonSettingsComponent {
                    GridGap = authoring.GridGap,
                    AliveVisualPrefab = GetEntity(authoring.AliveVisualPrefab, TransformUsageFlags.Dynamic),
                    VisualCellRotation = quaternion.EulerXYZ(authoring.VisualCellRotation),
                    MaxGridSize = authoring.MaxGridSize
                });
                AddComponent(entity, new CommonStepComponent {
                    StepDelay = authoring.StepDelay
                });
                
                AddComponent<NeedFitCameraComponent>(entity);
                SetComponentEnabled<NeedFitCameraComponent>(entity, false);
            }
        }
    }
    
    public struct CommonSettingsComponent : IComponentData {
        public float GridGap;
        public int2 MaxGridSize;
        public Entity AliveVisualPrefab;
        public quaternion VisualCellRotation;
    }
    
    public struct CommonStepComponent : IComponentData {
        public float StepDelay;
        public float Timer;
    }
    
    public struct NeedFitCameraComponent : IComponentData, IEnableableComponent {}
}