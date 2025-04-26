using System;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems {
    public partial class CustomInputSystem : SystemBase {
        private Camera _camera;
        private CustomInputAction _customInputAction;

        public EventHandler<ClickData> OnClick;
        public class ClickData : EventArgs {
            public int2 Position;
        }
        
        protected override void OnCreate() {
            _camera = Camera.main;
            _customInputAction = new CustomInputAction();
            _customInputAction.Game.Enable();
            _customInputAction.Game.NextStep.performed += NextStepOnperformed;
            _customInputAction.Game.AddCell.performed += AddCellOnperformed;
        }

        private void AddCellOnperformed(InputAction.CallbackContext obj) {
            var clickPosition = obj.ReadValue<Vector2>();
            Ray clickRay = _camera.ScreenPointToRay(clickPosition);
            OnClick?.Invoke(this, new ClickData() {
                Position = new int2((int)clickRay.origin.x, (int)clickRay.origin.z)
            });
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