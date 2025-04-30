using Unity.Entities;
using UnityEngine;

namespace Components {
    public class RandomComponentAuthoring : MonoBehaviour {
        public uint Seed = 1234567890;

        class Baker : Baker<RandomComponentAuthoring> {
            public override void Bake(RandomComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                uint seed = authoring.Seed;
                if (seed == 0) {
                    seed = (uint)Random.Range(0, int.MaxValue);
                }

                AddComponent(entity, new RandomComponent {
                    Value = new Unity.Mathematics.Random(seed)
                });
            }
        }
    }

    public struct RandomComponent : IComponentData {
        public Unity.Mathematics.Random Value;
    }
}