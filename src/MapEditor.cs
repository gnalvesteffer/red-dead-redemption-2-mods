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

        private static readonly Random Random = new Random();
        private readonly List<SpawnedObject> _spawnedObjects = new List<SpawnedObject>();
        private readonly Input[] _inputs;

        private bool _isInMapEditorMode;

        public MapEditor()
        {
            _inputs = new[]
            {
                new Input("Toggle Map Editor", Keys.F1, false, 1000, true, ToggleMapEditor),
                new Input("Spawn Test Object", Keys.F2, false, 1000, false, SpawnTestObject),
                new Input("Rotate Left", Keys.Left, true, 0, false, () => RotateLastSpawnedObject(new Vector3(0, 0, -1.0f))),
                new Input("Rotate Right", Keys.Right, true, 0, false, () => RotateLastSpawnedObject(new Vector3(0, 0, 1.0f))),
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

        private void ToggleMapEditor()
        {
            _isInMapEditorMode = !_isInMapEditorMode;
            Utilities.UserFriendlyPrint(_isInMapEditorMode ? "Entered Map Editor" : "Exited Map Editor");
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
            var spawnedObject = SpawnObject("p_tree_joshua_01a", spawnPosition, spawnRotation);
            Utilities.UserFriendlyPrint(Utilities.GetEntityRotation(spawnedObject.Entity));
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
