using Unity.Entities;
using UnityEngine;

namespace Components {
    public class InitialSpawnerComponentAuthoring : MonoBehaviour {
        public int EntitiesCount;
        public int SpawnBatchSize;

        class Baker : Baker<InitialSpawnerComponentAuthoring> {
            public override void Bake(InitialSpawnerComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new InitialSpawnerComponent {
                    EntitiesCount = authoring.EntitiesCount,
                    SpawnBatchSize = authoring.SpawnBatchSize
                });
            }
        }
    }

    public struct InitialSpawnerComponent : IComponentData {
        public int EntitiesCount;
        public int SpawnBatchSize;
    }
}