using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    public class GridPositionComponentAuthoring : MonoBehaviour {
        public int2 Position;

        private class GridPositionComponentBaker : Baker<GridPositionComponentAuthoring> {
            public override void Bake(GridPositionComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GridPositionComponent { Position = authoring.Position });
                ;
            }
        }
    }

    public struct GridPositionComponent : IComponentData {
        public int2 Position;
    }
}