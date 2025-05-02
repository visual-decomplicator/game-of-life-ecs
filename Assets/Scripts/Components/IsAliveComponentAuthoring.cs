using Unity.Entities;
using UnityEngine;

namespace Components {
    public class IsAliveComponentAuthoring : MonoBehaviour {
        private class IsAliveComponentBaker : Baker<IsAliveComponentAuthoring> {
            public override void Bake(IsAliveComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent<IsAliveComponent>(entity);
                SetComponentEnabled<IsAliveComponent>(entity, false);
            }
        }
    }
    
    public struct IsAliveComponent : IComponentData, IEnableableComponent {}
}