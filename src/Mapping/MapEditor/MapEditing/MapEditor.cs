using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapEditing.MapPersistence;
using MapEditing.Utilities;
using RDR2;
using RDR2.Math;
using RDR2.UI;
using Control = System.Windows.Forms.Control;
using Screen = System.Windows.Forms.Screen;

namespace MapEditing.MapEditing
{
    internal class MapEditor
    {
        private const float TranslationSpeed = 0.025f;
        private const float RotationSpeed = 1.0f;
        private readonly ThreadedClipboardUtility _threadedClipboardUtility = new ThreadedClipboardUtility();
        private readonly MapPersistenceManager _mapPersistenceManager = new MapPersistenceManager();
        private readonly List<MapObject> _spawnedObjects = new List<MapObject>();
        private readonly Dictionary<Keys, Input> _keyboardInputs;
        private readonly HashSet<Keys> _keysToProcess = new HashSet<Keys>();
        private readonly HashSet<Keys> _handledNonRepeatableKeys = new HashSet<Keys>();
        private Vector2 _lastCursorPosition = new Vector2(0.0f, 0.0f);
        private bool _isInMapEditorMode;
        private long _tick;
        private TransformationMode _transformationMode;
        private TransformationAxis _translationAxis;
        private TransformationAxis _rotationAxis;
        private MapEditorCamera _mapEditorCamera = new MapEditorCamera();
        private bool _isRotatingMapEditorCamera;
        private string _selectedObjectHash = "p_amb_tent01x";

        public MapEditor()
        {
            _keyboardInputs = new[]
            {
                new Input("Toggle Map Editor", Keys.F1, false, 0, true, ToggleMapEditor),
                new Input("Spawn Object", Keys.F2, false, 0, false, SpawnSelectedObjectHash),
                new Input("Delete Object", Keys.Delete, false, 0, false, RemoveLastSpawnedObject),
                new Input("Load Map", Keys.F3, false, 0, false, LoadMap),
                new Input("Save Map", Keys.F4, false, 0, false, SaveMap),
                new Input("Change Transformation Mode", Keys.Oemcomma, false, 0, false, ChangeTransformationMode),
                new Input("Change Transformation Axis", Keys.OemPeriod, false, 0, false, ChangeTransformationAxis),
                new Input("Negative Transformation", Keys.Left, true, 0, false, () => ApplyTransformation(-1.0f)),
                new Input("Positive Transformation", Keys.Right, true, 0, false, () => ApplyTransformation(1.0f)),
                new Input("Move Camera Forward", Keys.W, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeFront)),
                new Input("Move Camera Backward", Keys.S, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeBack)),
                new Input("Move Camera Left", Keys.A, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeLeft)),
                new Input("Move Camera Right", Keys.D, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeRight)),
                new Input("Move Camera Up", Keys.E, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeTop)),
                new Input("Move Camera Down", Keys.Q, true, 0, false, () => _mapEditorCamera.Translate(Vector3.RelativeBottom)),
                new Input("Fast Camera Speed", Keys.ShiftKey, true, 0, false, () => _mapEditorCamera.SetSpeedModifierThisTick(10.0f)),
                new Input("Slow Camera Speed", Keys.ControlKey, true, 0, false, () => _mapEditorCamera.SetSpeedModifierThisTick(0.1f)),
            }.ToDictionary(input => input.Key);
        }

        public void OnTick()
        {
            HandleInputs();
            _mapEditorCamera.OnTick();
            ++_tick;
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            _keysToProcess.Add(e.KeyCode);
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            _keysToProcess.Remove(e.KeyCode);
            _handledNonRepeatableKeys.Remove(e.KeyCode);
        }

        private void HandleInputs()
        {
            HandleMapEditorCameraRotation();
            HandleKeys();
        }

        private void HandleMapEditorCameraRotation()
        {
            if (!_isInMapEditorMode)
            {
                return;
            }
            var screenResolution = Screen.PrimaryScreen.Bounds; // TODO: make reliable
            if (Control.MouseButtons == MouseButtons.Right)
            {
                if (!_isRotatingMapEditorCamera)
                {
                    Cursor.Position = new Point(screenResolution.Width / 2, screenResolution.Height / 2);
                    _lastCursorPosition = new Vector2(0.0f, 0.0f);
                }
                var currentCursorPosition = new Vector2(
                    (float)Cursor.Position.X / screenResolution.Width - 0.5f,
                    (float)Cursor.Position.Y / screenResolution.Height - 0.5f
                );
                var deltaCursorPosition = currentCursorPosition - _lastCursorPosition;
                _mapEditorCamera.Rotate(new Vector3(-deltaCursorPosition.Y, 0, -deltaCursorPosition.X));
                _lastCursorPosition = currentCursorPosition;
                _isRotatingMapEditorCamera = true;
            }
            else
            {
                if (_isRotatingMapEditorCamera)
                {
                    Cursor.Position = new Point(screenResolution.Width / 2, screenResolution.Height / 2);
                    _lastCursorPosition = new Vector2(0.0f, 0.0f);
                }
                Hud.ShowCursorThisFrame();
                _isRotatingMapEditorCamera = false;
            }
        }

        private void HandleKeys()
        {
            foreach (var keyToProcess in _keysToProcess)
            {
                if (!_keyboardInputs.ContainsKey(keyToProcess))
                {
                    continue;
                }
                var input = _keyboardInputs[keyToProcess];
                var shouldHandleInput =
                    (input.CanBeUsedOutsideOfMapEditor || _isInMapEditorMode && !input.CanBeUsedOutsideOfMapEditor) &&
                    _tick - input.LastUsageTick > input.CooldownDurationInTicks &&
                    (input.IsRepeatable || !_handledNonRepeatableKeys.Contains(keyToProcess));
                if (shouldHandleInput)
                {
                    if (!input.IsRepeatable)
                    {
                        _handledNonRepeatableKeys.Add(keyToProcess);
                    }
                    input.LastUsageTick = _tick;
                    input.Handler();
                }
            }
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
            NativeUtility.UserFriendlyPrint($"Transformation Mode: {_transformationMode}");
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
            NativeUtility.UserFriendlyPrint($"Translation Axis: {_translationAxis}");
        }

        private void ChangeRotationAxis()
        {
            var totalRotationAxisEntries = Enum.GetValues(typeof(TransformationAxis)).Length;
            _rotationAxis = (TransformationAxis)(((int)_rotationAxis + 1) % totalRotationAxisEntries);
            NativeUtility.UserFriendlyPrint($"Rotation Axis: {_rotationAxis}");
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
            _mapEditorCamera.Enter();
            Game.Player.CanControlCharacter = false;
            NativeUtility.UserFriendlyPrint("Entered Map Editor");
        }

        private void OnMapEditorModeExit()
        {
            _mapEditorCamera.Exit();
            Game.Player.CanControlCharacter = true;
            NativeUtility.UserFriendlyPrint("Exited Map Editor");
        }

        private void TranslateLastSpawnedObject(Vector3 deltaTranslation)
        {
            var lastSpawnedObject = _spawnedObjects.LastOrDefault();
            if (lastSpawnedObject == null)
            {
                return;
            }
            var newPosition = lastSpawnedObject.Position + deltaTranslation;
            NativeUtility.SetEntityPosition(lastSpawnedObject.Entity, newPosition);
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
            NativeUtility.SetEntityRotation(lastSpawnedObject.Entity, newRotation);
            lastSpawnedObject.Rotation = newRotation;
        }

        private MapObject SpawnObject(string modelName, Vector3 position, Vector3 rotation)
        {
            var entity = NativeUtility.CreateObject(modelName, position);
            NativeUtility.SetEntityRotation(entity, rotation);
            var spawnedObject = new MapObject(modelName, position, rotation, entity);
            if (entity == null)
            {
                NativeUtility.UserFriendlyPrint($"Failed to spawn \"{modelName}\"");
                return null;
            }
            _spawnedObjects.Add(spawnedObject);
            NativeUtility.UserFriendlyPrint($"Created \"{modelName}\"");
            return spawnedObject;
        }

        private void SpawnSelectedObjectHash()
        {
            _selectedObjectHash = _threadedClipboardUtility.GetText();
            var spawnPosition = _mapEditorCamera.Position;
            var spawnRotation = new Vector3(0, 0, _mapEditorCamera.Rotation.Z);
            SpawnObject(_selectedObjectHash, spawnPosition, spawnRotation);
        }

        private void RemoveLastSpawnedObject()
        {
            var mapObject = _spawnedObjects.LastOrDefault();
            if (mapObject == null)
            {
                return;
            }
            mapObject.Entity.Delete();
            _spawnedObjects.Remove(mapObject);
            NativeUtility.UserFriendlyPrint($"Removed \"{mapObject.ModelName}\"");
        }

        private void LoadMap()
        {
            const string mapFilePath = "scripts/MapEditor/maps/test.map";
            var serializableMap = _mapPersistenceManager.LoadMap(mapFilePath);
            foreach (var serializableMapObject in serializableMap.MapObjects)
            {
                SpawnObject(
                    serializableMapObject.ModelName,
                    serializableMapObject.Position,
                    serializableMapObject.Rotation
                );
            }
            NativeUtility.UserFriendlyPrint($"Map loaded: \"{mapFilePath}\"");
        }

        private void SaveMap()
        {
            const string mapFilePath = "scripts/MapEditor/maps/test.map";
            var serializableMap = new SerializableMap
            {
                MapName = $"{Game.Player.Name}'s Map",
                AuthorName = Game.Player.Name,
                MapObjects = _spawnedObjects.Select(mapObject => new SerializableMapObject(mapObject)),
            };
            _mapPersistenceManager.SaveMap(mapFilePath, serializableMap);
            NativeUtility.UserFriendlyPrint($"Map saved to {mapFilePath}");
        }
    }
}
