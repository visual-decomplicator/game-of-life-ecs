using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components {
    public class InitialSpawnerComponentAuthoring : MonoBehaviour {
        public int2 MaxGridSize;
        public int EntitiesCount;
        public int SpawnBatchSize;

        class Baker : Baker<InitialSpawnerComponentAuthoring> {
            public override void Bake(InitialSpawnerComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new InitialSpawnerComponent {
                    MaxGridSize = authoring.MaxGridSize,
                    EntitiesCount = authoring.EntitiesCount,
                    SpawnBatchSize = authoring.SpawnBatchSize
                });
            }
        }
    }

    public struct InitialSpawnerComponent : IComponentData {
        public int2 MaxGridSize;
        public int EntitiesCount;
        public int SpawnBatchSize;
    }
}