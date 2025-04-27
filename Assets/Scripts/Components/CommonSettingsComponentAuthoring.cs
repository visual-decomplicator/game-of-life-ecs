using Unity.Entities;
using UnityEngine;

namespace Components {
    public class CommonSettingsComponentAuthoring : MonoBehaviour {
        public float GridGap;
        public GameObject CellPrefab;
        public GameObject DeadVisualPrefab;
        public GameObject AliveVisualPrefab;
        public float StepDelay;
        
        private class CommonSettingsComponentBaker : Baker<CommonSettingsComponentAuthoring> {
            public override void Bake(CommonSettingsComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CommonSettingsComponent {
                    GridGap = authoring.GridGap,
                    CellPrefab = GetEntity(authoring.CellPrefab, TransformUsageFlags.None),
                    DeadVisualPrefab = GetEntity(authoring.DeadVisualPrefab, TransformUsageFlags.Dynamic),
                    AliveVisualPrefab = GetEntity(authoring.AliveVisualPrefab, TransformUsageFlags.Dynamic)
                });
                AddComponent(entity, new CommonStepComponent {
                    StepDelay = authoring.StepDelay,
                    Timer = -1000 // For the first iteration make it longer, to have time to mark the necessary cells as alive using input.
                });;
            }
        }
    }
    
    public struct CommonSettingsComponent : IComponentData {
        public float GridGap;
        public Entity CellPrefab;
        public Entity DeadVisualPrefab;
        public Entity AliveVisualPrefab;
    }
    
    public struct CommonStepComponent : IComponentData {
        public float StepDelay;
        public float Timer;
    }
}