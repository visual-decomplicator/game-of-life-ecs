using Unity.Entities;
using UnityEngine;

namespace Components {
    public class VisualEntityComponentAuthoring : MonoBehaviour {
        private class VisualEntityComponentAuthoringBaker : Baker<VisualEntityComponentAuthoring> {
            public override void Bake(VisualEntityComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VisualEntityComponent {
                    Entity = Entity.Null
                });
            }
        }
    }
    
    public struct VisualEntityComponent : IComponentData {
        public Entity Entity;
    }
}