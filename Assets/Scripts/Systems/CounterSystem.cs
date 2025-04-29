using Components;
using Unity.Entities;

namespace Systems {
    public partial class CounterSystem : SystemBase {
        protected override void OnUpdate() {
            var hiddenEntitiesQuery = SystemAPI.QueryBuilder().WithAll<CounterComponent>().Build();
            var visibleEntitiesQuery = SystemAPI.QueryBuilder().WithAll<CellVisualComponent>().Build();
            int hiddenEntitiesCount = hiddenEntitiesQuery.CalculateEntityCount();
            int visibleEntitiesCount = visibleEntitiesQuery.CalculateEntityCount();
            CountersUI.Instance.SetHiddenEntitiesCount(hiddenEntitiesCount);
            CountersUI.Instance.SetVisibleEntitiesCount(visibleEntitiesCount);
            foreach (var fpsCounter in SystemAPI.Query<FPSCounterComponent>()) {
                CountersUI.Instance.SetFps(fpsCounter.CurrentFPS);
            }
        }
    }
}