using Unity.Entities;
using UnityEngine;

public class FPSCounterComponentAuthoring : MonoBehaviour
{
    public int MaxBufferElements;
    class Baker : Baker<FPSCounterComponentAuthoring> {
        public override void Bake(FPSCounterComponentAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new FPSCounterComponent {
                MaxBufferElements = authoring.MaxBufferElements
            });
            AddBuffer<FPSCounterElement>(entity);
        }
    }
}

[InternalBufferCapacity(50)]
public struct FPSCounterElement : IBufferElementData {
    public float Value;
}

public struct FPSCounterComponent : IComponentData {
    public int CurrentFPS;
    public int CurrentBufferElement;
    public int MaxBufferElements;
}