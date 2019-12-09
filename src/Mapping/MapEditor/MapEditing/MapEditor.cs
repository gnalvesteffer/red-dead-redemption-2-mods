using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MapEditing.MapPersistence;
using MapEditing.UserInterface.MenuFramework;
using MapEditing.Utilities;
using RDR2;
using RDR2.Math;
using RDR2.UI;
using Control = System.Windows.Forms.Control;
using Menu = MapEditing.UserInterface.MenuFramework.Menu;
using TextElement = MapEditing.UserInterface.Basic.TextElement;

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
        private readonly Menu _mapEditorMenu;
        private readonly MapEditorCamera _mapEditorCamera = new MapEditorCamera();
        private bool _isRotatingMapEditorCamera;
        private bool _isInMapEditorMode;
        private long _tick;
        private TransformationMode _transformationMode;
        private TransformationAxis _translationAxis;
        private TransformationAxis _rotationAxis;
        private string _selectedObjectModelName;
        private MapObject _selectedMapObject;

        public MapEditor()
        {
            _mapEditorMenu = new Menu(
                new MenuConfiguration
                {
                    Title = "Map Editor",
                    TitleBackgroundColor = Color.Black,
                    TitleTextColor = Color.White,
                    MenuItems = new[]
                    {
                        new MenuItemConfiguration
                        {
                            GetDisplayText = () => "Spawn Object",
                            FontSizeInPixels = 14,
                            UnfocusedBackgroundColor = Color.Black,
                            FocusedBackgroundColor = Color.DarkRed,
                            UnfocusedTextColor = Color.DarkGray,
                            FocusedTextColor = Color.White,
                            OnSelect = SpawnSelectedObjectModelName,
                        },
                        new MenuItemConfiguration
                        {
                            GetDisplayText = () => $"Delete Object",
                            FontSizeInPixels = 14,
                            UnfocusedBackgroundColor = Color.Black,
                            FocusedBackgroundColor = Color.DarkRed,
                            UnfocusedTextColor = Color.DarkGray,
                            FocusedTextColor = Color.White,
                            OnSelect = DeleteSelectedObject,
                        },
                    },
                }
            );
            _keyboardInputs = new[]
            {
                new Input("Toggle Map Editor", Keys.F1, false, 0, true, ToggleMapEditor),
                new Input("Spawn Object", Keys.F2, false, 0, false, SpawnSelectedObjectModelName),
                new Input("Delete Object", Keys.Delete, false, 0, false, DeleteSelectedObject),
                new Input("Load Map", Keys.F3, false, 0, false, LoadMap),
                new Input("Save Map", Keys.F4, false, 0, false, SaveMap),
                new Input("Select Previous Object", Keys.OemOpenBrackets, false, 0, false, () => ChangeSelectedObject(-1)),
                new Input("Select Next Object", Keys.OemCloseBrackets, false, 0, false, () => ChangeSelectedObject(1)),
                new Input("Change Transformation Mode", Keys.Oemcomma, false, 0, false, ChangeTransformationMode),
                new Input("Change Transformation Axis", Keys.OemPeriod, false, 0, false, ChangeTransformationAxis),
                new Input("Menu Up", Keys.Up, false, 0, false, () => _mapEditorMenu.NavigateMenu(MenuNavigationDirection.Previous)),
                new Input("Menu Down", Keys.Down, false, 0, false, () => _mapEditorMenu.NavigateMenu(MenuNavigationDirection.Next)),
                new Input("Menu Select", Keys.Enter, false, 0, false, () => _mapEditorMenu.SelectFocusedMenuItem()),
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
            DrawSelectedObjectIndicator();
            _mapEditorMenu.Draw();
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
            var screenResolution = ScreenUtility.GetScreenResolution();
            if (Control.MouseButtons == MouseButtons.Right)
            {
                if (!_isRotatingMapEditorCamera)
                {
                    Cursor.Position = new Point((int)screenResolution.X / 2, (int)screenResolution.Y / 2);
                }
                var deltaCursorPosition = new Vector2(
                    Cursor.Position.X / screenResolution.X - 0.5f,
                    Cursor.Position.Y / screenResolution.Y - 0.5f
                );
                _mapEditorCamera.Rotate(new Vector3(-deltaCursorPosition.Y, 0, -deltaCursorPosition.X));
                Cursor.Position = new Point((int)screenResolution.X / 2, (int)screenResolution.Y / 2);
                _isRotatingMapEditorCamera = true;
            }
            else
            {
                if (_isRotatingMapEditorCamera)
                {
                    Cursor.Position = new Point((int)screenResolution.X / 2, (int)screenResolution.Y / 2);
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

        private void DrawSelectedObjectIndicator()
        {
            if (!_isInMapEditorMode || _selectedMapObject == null)
            {
                return;
            }

            new TextElement
            {
                Message = _selectedMapObject.ModelName,
                NormalizedScreenPosition = NativeUtility.WorldToScreen(_selectedMapObject.Position),
                FontSizeInPixels = 16,
                Color = Color.White,
                IsCentered = true,
            }.Draw();
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
            _mapEditorMenu.IsVisible = true;
            Game.Player.CanControlCharacter = false;
            NativeUtility.UserFriendlyPrint("Entered Map Editor");
        }

        private void OnMapEditorModeExit()
        {
            _mapEditorCamera.Exit();
            _mapEditorMenu.IsVisible = false;
            Game.Player.CanControlCharacter = true;
            NativeUtility.UserFriendlyPrint("Exited Map Editor");
        }

        private void TranslateLastSpawnedObject(Vector3 deltaTranslation)
        {
            if (_selectedMapObject == null)
            {
                return;
            }
            var newPosition = _selectedMapObject.Position + deltaTranslation;
            NativeUtility.SetEntityPosition(_selectedMapObject.Entity, newPosition);
            _selectedMapObject.Position = newPosition;
        }

        private void RotateLastSpawnedObject(Vector3 deltaRotation)
        {
            if (_selectedMapObject == null)
            {
                return;
            }
            var newRotation = _selectedMapObject.Rotation + deltaRotation;
            NativeUtility.SetEntityRotation(_selectedMapObject.Entity, newRotation);
            _selectedMapObject.Rotation = newRotation;
        }

        private MapObject SpawnObject(string modelName, Vector3 position, Vector3 rotation)
        {
            Entity entity;
            if (modelName.StartsWith("0x"))
            {
                if (int.TryParse(modelName.Substring(2), NumberStyles.HexNumber, new NumberFormatInfo(), out var modelHash))
                {
                    entity = NativeUtility.CreateObject(modelHash, position);
                }
                else
                {
                    NativeUtility.UserFriendlyPrint($"Failed to spawn \"{modelName}\"");
                    return null;
                }
            }
            else
            {
                entity = NativeUtility.CreateObject(modelName, position);
            }
            NativeUtility.SetEntityRotation(entity, rotation);
            var spawnedObject = new MapObject(modelName, position, rotation, entity);
            if (entity == null)
            {
                NativeUtility.UserFriendlyPrint($"Failed to spawn \"{modelName}\"");
                return null;
            }
            _spawnedObjects.Add(spawnedObject);
            _selectedMapObject = spawnedObject;
            NativeUtility.UserFriendlyPrint($"Created \"{modelName}\"");
            return spawnedObject;
        }

        private void SpawnSelectedObjectModelName()
        {
            _selectedObjectModelName = _threadedClipboardUtility.GetText();
            var cameraPosition = _mapEditorCamera.Position;
            var adjustedCameraRotation = (float)Math.PI / 180f * _mapEditorCamera.Rotation;
            var raycastResult = World.Raycast(
                cameraPosition,
                cameraPosition +
                Vector3.Normalize(
                    new Vector3(
                        (float)-Math.Sin(adjustedCameraRotation.Z) * (float)Math.Abs(Math.Cos(adjustedCameraRotation.X)),
                        (float)Math.Cos(adjustedCameraRotation.Z) * (float)Math.Abs(Math.Cos(adjustedCameraRotation.X)),
                        (float)Math.Sin(adjustedCameraRotation.X)
                    )
                ) * 100.0f,
                IntersectOptions.Everything
            );
            var spawnPosition = raycastResult.DitHit
                ? raycastResult.HitPosition
                : _mapEditorCamera.Position;
            var spawnRotation = new Vector3(0, 0, _mapEditorCamera.Rotation.Z);
            SpawnObject(_selectedObjectModelName, spawnPosition, spawnRotation);
        }

        private void DeleteSelectedObject()
        {
            if (_selectedMapObject == null)
            {
                return;
            }
            var selectedMapObjectIndex = _spawnedObjects.IndexOf(_selectedMapObject);
            _selectedMapObject.Entity.Delete();
            _spawnedObjects.Remove(_selectedMapObject);
            NativeUtility.UserFriendlyPrint($"Removed \"{_selectedMapObject.ModelName}\"");
            var totalSpawnedObjects = _spawnedObjects.Count;
            _selectedMapObject = totalSpawnedObjects > 0
                ? _spawnedObjects[(selectedMapObjectIndex - 1).Clamp(0, totalSpawnedObjects - 1)]
                : null;
        }

        private void ChangeSelectedObject(int deltaIndex)
        {
            var selectedMapObjectIndex = _spawnedObjects.IndexOf(_selectedMapObject);
            var totalSpawnedObjects = _spawnedObjects.Count;
            _selectedMapObject = totalSpawnedObjects > 0
                ? _spawnedObjects[(selectedMapObjectIndex + deltaIndex).Clamp(0, totalSpawnedObjects - 1)]
                : null;
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
