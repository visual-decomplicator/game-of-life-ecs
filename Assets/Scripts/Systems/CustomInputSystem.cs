using Components;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace Systems {
    public partial class CustomInputSystem : SystemBase {
        private CustomInputAction _customInputAction;
        protected override void OnCreate() {
            _customInputAction = new CustomInputAction();
            _customInputAction.Game.Enable();
            _customInputAction.Game.NextStep.performed += NextStepOnperformed;
        }

        private void NextStepOnperformed(InputAction.CallbackContext obj) {
            foreach (var step in SystemAPI.Query<RefRW<CommonStepComponent>>()) {
                step.ValueRW.Timer = step.ValueRO.StepDelay;
            }
        }

        protected override void OnUpdate() {
            
        }
        
        protected override void OnDestroy() {
            _customInputAction.Dispose();
        }
    }
}