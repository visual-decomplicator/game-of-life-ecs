using Unity.Entities;
using UnityEngine;

namespace Components {
    public class CounterComponentAuthoring : MonoBehaviour {
        public int Counter;
        private class CounterComponentAuthoringBaker : Baker<CounterComponentAuthoring> {
            public override void Bake(CounterComponentAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CounterComponent { Value = authoring.Counter });
            }
        }
    }
    
    public struct CounterComponent : IComponentData {
        public int Value;
    }
}