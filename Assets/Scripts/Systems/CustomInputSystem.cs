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
            public float2 Position;
        }
        
        protected override void OnCreate() {
            _camera = Camera.main;
            RequireForUpdate<CommonSettingsComponent>();
            _customInputAction = new CustomInputAction();
            _customInputAction.Game.Enable();
            _customInputAction.Game.NextStep.performed += NextStepOnperformed;
            _customInputAction.Game.PauseSteps.performed += PauseStepsOnperformed;
            _customInputAction.Game.AddCell.performed += AddCellOnperformed;
            _customInputAction.Game.CameraAutoFit.performed += CameraAutoFitOnperformed;
            _customInputAction.Game.CameraManualZoom.performed += CameraManualZoomOnperformed;
            _customInputAction.Game.CameraManualMovement.performed += CameraManualMovementOnperformed;
        }

        private void CameraManualMovementOnperformed(InputAction.CallbackContext obj) {
            Vector2 movement = obj.ReadValue<Vector2>();
            CameraController.Instance.Move(movement);
        }

        private void CameraManualZoomOnperformed(InputAction.CallbackContext obj) {
            Vector2 zoom = obj.ReadValue<Vector2>();
            if (zoom.y > 0) {
                CameraController.Instance.ZoomIn();
            } else if (zoom.y < 0) {
                CameraController.Instance.ZoomOut();
            }
        }

        private void CameraAutoFitOnperformed(InputAction.CallbackContext obj) {
            Entity commonEntity = SystemAPI.GetSingletonEntity<CommonSettingsComponent>();
            SystemAPI.SetComponentEnabled<NeedFitCameraComponent>(commonEntity, true);
        }

        private void PauseStepsOnperformed(InputAction.CallbackContext obj) {
            foreach (var step in SystemAPI.Query<RefRW<CommonStepComponent>>()) {
                step.ValueRW.Timer = -1000f;
            }
        }

        private void AddCellOnperformed(InputAction.CallbackContext obj) {
            var clickPosition = obj.ReadValue<Vector2>();
            Ray clickRay = _camera.ScreenPointToRay(clickPosition);
            OnClick?.Invoke(this, new ClickData() {
                Position = new float2(clickRay.origin.x, clickRay.origin.z)
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