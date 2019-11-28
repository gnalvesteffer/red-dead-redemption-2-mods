using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RDR2;
using RDR2.Math;
using RDR2.Native;

namespace MapEditing
{
    internal static class Utilities
    {
        public static void DebugPrint(object text)
        {
            var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
            Function.Call(Hash._DRAW_TEXT, createdString, 5, 5);
        }

        public static void UserFriendlyPrint(object text)
        {
            if (text == null)
            {
                text = string.Empty;
            }
            var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
            Function.Call(Hash._LOG_SET_CACHED_OBJECTIVE, createdString);
            Function.Call(Hash._LOG_PRINT_CACHED_OBJECTIVE);
            Function.Call(Hash._LOG_CLEAR_CACHED_OBJECTIVE);
        }

        public static int GetHashKey(string hashValue)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, hashValue);
        }

        public static Ped CreatePed(string hashValue, Vector3 position)
        {
            return Function.Call<Ped>(Hash.CREATE_PED, GetHashKey(hashValue), position.X, position.Y, position.Z, 0, false, false, 0);
        }

        public static Entity CreateObject(string hashValue, Vector3 position)
        {
            return Function.Call<Entity>(Hash.CREATE_OBJECT, GetHashKey(hashValue), position.X, position.Y, position.Z, false, false, false);
        }

        public static Vector3 GetEntityRotation(Entity entity)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION, entity, 0);
        }

        public static void SetEntityRotation(Entity entity, Vector3 rotation)
        {
            Function.Call(Hash.SET_ENTITY_ROTATION, entity, rotation.X, rotation.Y, rotation.Z, 0, false);
        }

        public static Vector3 GetEntityPosition(Entity entity)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, entity);
        }

        public static void SetEntityPosition(Entity entity, Vector3 position)
        {
            Function.Call(Hash.SET_ENTITY_COORDS, entity, position.X, position.Y, position.Z);
        }
    }

    internal class MapEditor
    {
        private class SpawnedObject
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public string HashValue;
            public Entity Entity;

            public SpawnedObject(string hashValue, Vector3 position, Vector3 rotation, Entity entity)
            {
                HashValue = hashValue;
                Position = position;
                Rotation = rotation;
                Entity = entity;
            }
        }

        private class Input
        {
            public string Name;
            public Keys Key;
            public bool CanBeHeld;
            public int CooldownDurationInMilliseconds;
            public bool CanBeUsedOutsideOfMapEditor;
            public Action Handler;
            public int LastUsageTimestampInMilliseconds;


            public Input(
                string name,
                Keys key,
                bool canBeHeld,
                int cooldownDurationInMilliseconds,
                bool canBeUsedOutsideOfMapEditor,
                Action handler
            )
            {
                Name = name;
                Key = key;
                CanBeHeld = canBeHeld;
                CooldownDurationInMilliseconds = cooldownDurationInMilliseconds;
                CanBeUsedOutsideOfMapEditor = canBeUsedOutsideOfMapEditor;
                Handler = handler;
            }
        }

        private enum TransformationAxis
        {
            X,
            Y,
            Z,
        }

        private enum TransformationMode
        {
            Translation,
            Rotation,
        }

        private const float TranslationSpeed = 0.1f;
        private const float RotationSpeed = 1.0f;
        private static readonly Random Random = new Random();
        private readonly List<SpawnedObject> _spawnedObjects = new List<SpawnedObject>();
        private readonly Input[] _inputs;
        private bool _isInMapEditorMode;
        private TransformationMode _transformationMode;
        private TransformationAxis _translationAxis;
        private TransformationAxis _rotationAxis;

        public MapEditor()
        {
            _inputs = new[]
            {
                new Input("Toggle Map Editor", Keys.F1, false, 1000, true, ToggleMapEditor),
                new Input("Spawn Test Object", Keys.F2, false, 1000, false, SpawnTestObject),
                new Input("Change Transformation Mode", Keys.Oemcomma, false, 200, false, ChangeTransformationMode),
                new Input("Change Transformation Axis", Keys.OemPeriod, false, 200, false, ChangeTransformationAxis),
                new Input("Negative Transformation", Keys.Left, true, 0, false, () => ApplyTransformation(-1.0f)),
                new Input("Positive Transformation", Keys.Right, true, 0, false, () => ApplyTransformation(1.0f)),
            };
        }

        public void OnTick()
        {
            HandleInputs();
        }

        private void HandleInputs()
        {
            foreach (var input in _inputs)
            {
                var currentTimeInMilliseconds = Game.GameTime;
                if (
                    Game.IsKeyPressed(input.Key) &&
                    (input.CanBeUsedOutsideOfMapEditor || _isInMapEditorMode && !input.CanBeUsedOutsideOfMapEditor) &&
                    currentTimeInMilliseconds - input.LastUsageTimestampInMilliseconds > input.CooldownDurationInMilliseconds
                )
                {
                    input.LastUsageTimestampInMilliseconds = currentTimeInMilliseconds;
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
            Utilities.UserFriendlyPrint(_isInMapEditorMode ? "Entered Map Editor" : "Exited Map Editor");
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
            _spawnedObjects.Add(spawnedObject);
            Utilities.UserFriendlyPrint(string.Format("Created {0}", hashValue));
            return spawnedObject;
        }

        private void SpawnTestObject()
        {
            var playerPed = Game.Player.Character;
            var spawnRotation = new Vector3(0.0f, 0.0f, (float)Random.NextDouble() * 360.0f);
            var spawnPosition = Utilities.GetEntityPosition(playerPed).Around(3.0f);
            SpawnObject("p_tree_joshua_01a", spawnPosition, spawnRotation);
        }

        private void LoadMap()
        {
        }

        private void SaveMap()
        {
        }
    }

    public class MapEditorScript : Script
    {
        private readonly MapEditor _mapEditor = new MapEditor();

        public MapEditorScript()
        {
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            _mapEditor.OnTick();
        }
    }
}
