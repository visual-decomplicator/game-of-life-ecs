using Unity.Entities;

namespace Components {
    public struct VisualEntityComponent : ICleanupComponentData {
        public Entity Entity;
    }
    
    public struct NeedChangeVisualComponent : IComponentData, IEnableableComponent {}
}