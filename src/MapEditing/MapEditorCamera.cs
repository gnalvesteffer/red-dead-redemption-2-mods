using System;
using RDR2;
using RDR2.Math;

namespace MapEditing
{
    public class MapEditorCamera
    {
        private const float CameraMovementSpeed = 0.1f;
        private const float CameraRotationSpeed = 360.0f;
        private Camera _camera;
        private Vector3 _cameraDeltaRelativeTranslationThisTick;
        private Vector3 _cameraDeltaRelativeRotationThisTick;
        private float _speedModifierThisTick;

        public Vector3 Position => _camera?.Position ?? new Vector3();
        public Vector3 Rotation => _camera?.Rotation ?? new Vector3();

        public void OnTick()
        {
            ApplyTranslation();
            ApplyRotation();
            ResetInputs();
        }

        public void Enter()
        {
            var playerPed = Game.Player.Character;
            var cameraPosition = Utilities.GetEntityPosition(playerPed);
            var cameraRotation = Utilities.GetEntityRotation(playerPed);
            _camera = World.CreateCamera(cameraPosition, cameraRotation, 75.0f);
            _camera.IsActive = true;
            Utilities.EnterScriptedCamera();
        }

        public void Exit()
        {
            _camera.IsActive = false;
            _camera.Delete();
            Utilities.ExitScriptedCamera();
        }

        public void Translate(Vector3 amount)
        {
            _cameraDeltaRelativeTranslationThisTick += amount;
        }

        public void Rotate(Vector3 amount)
        {
            _cameraDeltaRelativeRotationThisTick += amount;
        }

        public void SetSpeedModifierThisTick(float speedModifier)
        {
            _speedModifierThisTick = speedModifier;
        }

        private void ApplyTranslation()
        {
            if (_camera == null)
            {
                return;
            }
            var cameraRotation = _camera.Rotation;
            var cameraForwardRotationRadians = cameraRotation * Utilities.DegreesToRadians;
            var cameraRightRotationRadians = new Vector3(cameraRotation.X, cameraRotation.Y, cameraRotation.Z + 90) * Utilities.DegreesToRadians;
            var localSpaceRelativeDeltaTranslation = new Vector3(
                (float)(_cameraDeltaRelativeTranslationThisTick.X * Math.Cos(cameraForwardRotationRadians.Z)) + (float)(_cameraDeltaRelativeTranslationThisTick.Y * Math.Cos(cameraRightRotationRadians.Z)),
                (float)(_cameraDeltaRelativeTranslationThisTick.X * Math.Sin(cameraForwardRotationRadians.Z)) + (float)(_cameraDeltaRelativeTranslationThisTick.Y * Math.Sin(cameraRightRotationRadians.Z)),
                _cameraDeltaRelativeTranslationThisTick.Z
            );
            _camera.Position += localSpaceRelativeDeltaTranslation * CameraMovementSpeed * _speedModifierThisTick;
        }

        private void ApplyRotation()
        {
            if (_camera == null)
            {
                return;
            }
            var localSpaceRelativeDeltaRotation = new Vector3(
                _cameraDeltaRelativeRotationThisTick.X,
                0.0f,
                _cameraDeltaRelativeRotationThisTick.Z
            );
            _camera.Rotation += localSpaceRelativeDeltaRotation * CameraRotationSpeed;
        }

        private void ResetInputs()
        {
            _cameraDeltaRelativeTranslationThisTick = new Vector3();
            _cameraDeltaRelativeRotationThisTick = new Vector3();
            _speedModifierThisTick = 1.0f;
        }
    }
}
