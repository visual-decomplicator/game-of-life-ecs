using Unity.Entities;
using UnityEngine;

namespace Components {
    public class CellVisualComponentAuthoring : MonoBehaviour {
        private class EntityVisualComponentBaker : Baker<CellVisualComponentAuthoring> {
            public override void Bake(CellVisualComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CellVisualComponent>(entity);
            }
        }
    }
    
    public struct CellVisualComponent : IComponentData {}
}