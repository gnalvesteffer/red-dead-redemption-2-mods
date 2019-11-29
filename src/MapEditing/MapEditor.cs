using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RDR2;
using RDR2.Math;

namespace MapEditing
{
    public class MapEditor
    {
        private const float TranslationSpeed = 0.1f;
        private const float RotationSpeed = 1.0f;
        private const float CameraMovementSpeed = 0.1f;
        private static readonly Random Random = new Random();
        private readonly List<SpawnedObject> _spawnedObjects = new List<SpawnedObject>();
        private readonly Input[] _inputs;
        private bool _isInMapEditorMode;
        private long _tick;
        private TransformationMode _transformationMode;
        private TransformationAxis _translationAxis;
        private TransformationAxis _rotationAxis;
        private Camera _camera;

        public MapEditor()
        {
            _inputs = new[]
            {
                new Input("Toggle Map Editor", Keys.F1, 30, true, ToggleMapEditor),
                new Input("Spawn Test Object", Keys.F2, 30, false, SpawnTestObject),
                new Input("Change Transformation Mode", Keys.Oemcomma, 30, false, ChangeTransformationMode),
                new Input("Change Transformation Axis", Keys.OemPeriod, 30, false, ChangeTransformationAxis),
                new Input("Negative Transformation", Keys.Left, 0, false, () => ApplyTransformation(-1.0f)),
                new Input("Positive Transformation", Keys.Right, 0, false, () => ApplyTransformation(1.0f)),
                new Input("Move Camera Forward", Keys.W, 0, false, () => MoveCamera(Vector3.RelativeFront, new Vector3())),
                new Input("Move Camera Backward", Keys.S, 0, false, () => MoveCamera(Vector3.RelativeBack, new Vector3())),
                new Input("Move Camera Left", Keys.A, 0, false, () => MoveCamera(Vector3.RelativeLeft, new Vector3())),
                new Input("Move Camera Right", Keys.D, 0, false, () => MoveCamera(Vector3.RelativeRight, new Vector3())),
                new Input("Move Camera Up", Keys.E, 0, false, () => MoveCamera(Vector3.RelativeTop, new Vector3())),
                new Input("Move Camera Down", Keys.Q, 0, false, () => MoveCamera(Vector3.RelativeBottom, new Vector3())),
            };
        }

        public void OnTick()
        {
            HandleInputs();
            ++_tick;
        }

        private void HandleInputs()
        {
            foreach (var input in _inputs)
            {
                if (
                    Game.IsKeyPressed(input.Key) &&
                    (input.CanBeUsedOutsideOfMapEditor || _isInMapEditorMode && !input.CanBeUsedOutsideOfMapEditor) &&
                    _tick - input.LastUsageTick > input.CooldownDurationInTicks
                )
                {
                    input.LastUsageTick = _tick;
                    input.Handler();
                }
            }
        }

        private void MoveCamera(Vector3 relativeDeltaTranslation, Vector3 relativeDeltaRotation)
        {
            var cameraRotation = _camera.Rotation;
            var cameraForwardRotationRadians = cameraRotation * Utilities.DegreesToRadians;
            var cameraRightRotationRadians = new Vector3(cameraRotation.X, cameraRotation.Y, cameraRotation.Z + 90) * Utilities.DegreesToRadians;
            var localSpaceRelativeDeltaTranslation = new Vector3(
                (float)(relativeDeltaTranslation.X * Math.Cos(cameraForwardRotationRadians.Z)) + (float)(relativeDeltaTranslation.Y * Math.Cos(cameraRightRotationRadians.Z)),
                (float)(relativeDeltaTranslation.X * Math.Sin(cameraForwardRotationRadians.Z)) + (float)(relativeDeltaTranslation.Y * Math.Sin(cameraRightRotationRadians.Z)),
                relativeDeltaTranslation.Z
            );
            _camera.Position += localSpaceRelativeDeltaTranslation * CameraMovementSpeed;
        }

        private void ApplyTransformation(float amount)
        {
            switch (_transformationMode)
            {
                case TransformationMode.Translation:
                    switch (_translationAxis)
                    {
                        case TransformationAxis.X:
                            TranslateLastSpawnedObject(new Vector3(amount, 0, 0) * TranslationSpeed);
                            break;
                        case TransformationAxis.Y:
                            TranslateLastSpawnedObject(new Vector3(0, amount, 0) * TranslationSpeed);
                            break;
                        case TransformationAxis.Z:
                            TranslateLastSpawnedObject(new Vector3(0, 0, amount) * TranslationSpeed);
                            break;
                    }
                    break;
                case TransformationMode.Rotation:
                    switch (_rotationAxis)
                    {
                        case TransformationAxis.X:
                            RotateLastSpawnedObject(new Vector3(amount, 0, 0) * RotationSpeed);
                            break;
                        case TransformationAxis.Y:
                            RotateLastSpawnedObject(new Vector3(0, amount, 0) * RotationSpeed);
                            break;
                        case TransformationAxis.Z:
                            RotateLastSpawnedObject(new Vector3(0, 0, amount) * RotationSpeed);
                            break;
                    }
                    break;
            }
        }

        private void ChangeTransformationMode()
        {
            var totalTransformationModeEntries = Enum.GetValues(typeof(TransformationMode)).Length;
            _transformationMode = (TransformationMode)(((int)_transformationMode + 1) % totalTransformationModeEntries);
            Utilities.UserFriendlyPrint(string.Format("Transformation Mode: {0}", _transformationMode));
        }

        private void ChangeTransformationAxis()
        {
            switch (_transformationMode)
            {
                case TransformationMode.Translation:
                    ChangeTranslationAxis();
                    break;
                case TransformationMode.Rotation:
                    ChangeRotationAxis();
                    break;
            }
        }

        private void ChangeTranslationAxis()
        {
            var totalAxisEntries = Enum.GetValues(typeof(TransformationAxis)).Length;
            _translationAxis = (TransformationAxis)(((int)_translationAxis + 1) % totalAxisEntries);
            Utilities.UserFriendlyPrint(string.Format("Translation Axis: {0}", _translationAxis));
        }

        private void ChangeRotationAxis()
        {
            var totalRotationAxisEntries = Enum.GetValues(typeof(TransformationAxis)).Length;
            _rotationAxis = (TransformationAxis)(((int)_rotationAxis + 1) % totalRotationAxisEntries);
            Utilities.UserFriendlyPrint(string.Format("Rotation Axis: {0}", _rotationAxis));
        }

        private void ToggleMapEditor()
        {
            _isInMapEditorMode = !_isInMapEditorMode;
            if (_isInMapEditorMode)
            {
                OnMapEditorModeEnter();
            }
            else
            {
                OnMapEditorModeExit();
            }
        }

        private void OnMapEditorModeEnter()
        {
            var playerPed = Game.Player.Character;
            var cameraPosition = Utilities.GetEntityPosition(playerPed);
            var cameraRotation = Utilities.GetEntityRotation(playerPed);
            _camera = World.CreateCamera(cameraPosition, cameraRotation, 75.0f);
            _camera.IsActive = true;
            Utilities.EnterScriptedCamera();
            Game.Player.CanControlCharacter = false;
            Utilities.UserFriendlyPrint("Entered Map Editor");
        }

        private void OnMapEditorModeExit()
        {
            _camera.IsActive = false;
            _camera.Delete();
            Utilities.ExitScriptedCamera();
            Game.Player.CanControlCharacter = true;
            Utilities.UserFriendlyPrint("Exited Map Editor");
        }

        private void TranslateLastSpawnedObject(Vector3 deltaTranslation)
        {
            var lastSpawnedObject = _spawnedObjects.LastOrDefault();
            if (lastSpawnedObject == null)
            {
                return;
            }
            var newPosition = lastSpawnedObject.Position + deltaTranslation;
            Utilities.SetEntityPosition(lastSpawnedObject.Entity, newPosition);
            lastSpawnedObject.Position = newPosition;
        }

        private void RotateLastSpawnedObject(Vector3 deltaRotation)
        {
            var lastSpawnedObject = _spawnedObjects.LastOrDefault();
            if (lastSpawnedObject == null)
            {
                return;
            }
            var newRotation = lastSpawnedObject.Rotation + deltaRotation;
            Utilities.SetEntityRotation(lastSpawnedObject.Entity, newRotation);
            lastSpawnedObject.Rotation = newRotation;
        }

        private SpawnedObject SpawnObject(string hashValue, Vector3 position, Vector3 rotation)
        {
            var entity = Utilities.CreateObject(hashValue, position);
            Utilities.SetEntityRotation(entity, rotation);
            var spawnedObject = new SpawnedObject(hashValue, position, rotation, entity);
            if (entity == null)
            {
                Utilities.UserFriendlyPrint(string.Format("Failed to create {0}", hashValue));
                return null;
            }
            _spawnedObjects.Add(spawnedObject);
            Utilities.UserFriendlyPrint(string.Format("Created {0}", hashValue));
            return spawnedObject;
        }

        private void SpawnTestObject()
        {
            var playerPed = Game.Player.Character;
            var spawnRotation = new Vector3(0.0f, 0.0f, (float)Random.NextDouble() * 360.0f);
            var spawnPosition = Utilities.GetEntityPosition(playerPed).Around(3.0f);
            SpawnObject("P_TRUNK04X", spawnPosition, spawnRotation);
        }

        private void LoadMap()
        {
        }

        private void SaveMap()
        {
        }
    }
}
