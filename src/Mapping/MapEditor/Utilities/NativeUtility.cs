using System;
using System.Drawing;
using RDR2;
using RDR2.Math;
using RDR2.Native;
using Font = RDR2.UI.Font;

namespace MapEditing.Utilities
{
    internal static class NativeUtility
    {
        public const int GameFullFontSizeInPixels = 45;
        public const float DegreesToRadians = (float)(Math.PI * 2 / 360.0f);
        public const float RadiansToDegrees = (float)(360 / (Math.PI * 2));

        public static void DebugPrint(object text)
        {
            var createdString = CreateString(text.ToString());
            Function.Call(Hash._DRAW_TEXT, createdString, 5, 5);
            Console.WriteLine(text);
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

        public static string CreateString(string text)
        {
            return Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text);
        }

        public static int GetHashKeyFromModelName(string modelName)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, modelName);
        }

        public static Ped CreatePed(string modelName, Vector3 position, bool isVisible = true)
        {
            var ped = Function.Call<Ped>(Hash.CREATE_PED, GetHashKeyFromModelName(modelName), position.X, position.Y, position.Z, 0, false, false, 0);
            Function.Call((Hash)0x283978A15512B2FE, ped, isVisible);
            return ped;
        }

        public static Entity CreateObject(int hash, Vector3 position)
        {
            return Function.Call<Entity>(Hash.CREATE_OBJECT_NO_OFFSET, hash, position.X, position.Y, position.Z, false, false, false);
        }

        public static Entity CreateObject(string modelName, Vector3 position)
        {
            return Function.Call<Entity>(Hash.CREATE_OBJECT_NO_OFFSET, GetHashKeyFromModelName(modelName), position.X, position.Y, position.Z, false, false, false);
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

        public static void EnterScriptedCamera()
        {
            Function.Call(Hash.RENDER_SCRIPT_CAMS, true, false, 0);
        }

        public static void ExitScriptedCamera()
        {
            Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, 0);
        }

        public enum ShapeTestIntersectionLevel : uint
        {
            None = 0,
            IntersectWorld = 1,
            IntersectVehicles = 2,
            IntersectPedsSimpleCollision = 4,
            IntersectPeds = 8,
            IntersectObjects = 16,
            IntersectWater = 32,
            Unknown = 128,
            IntersectFoliage = 256,
            IntersectEverything = 4294967295
        }

        /// <returns>Ray Handle</returns>
        public static int StartShapeTestRay(
            Vector3 startPosition,
            Vector3 endPosition,
            ShapeTestIntersectionLevel intersectionLevel = ShapeTestIntersectionLevel.IntersectObjects,
            Entity entityToIgnore = null
        )
        {
            return Function.Call<int>(
                Hash._START_SHAPE_TEST_RAY,
                startPosition.X,
                startPosition.Y,
                startPosition.Z,
                endPosition.X,
                endPosition.Y,
                endPosition.Z,
                (int)intersectionLevel,
                entityToIgnore,
                1
            );
        }

        /// Bad results
        public static (int resultStatus, bool didHit, Vector3 hitPosition, Vector3 surfaceNormal) GetShapeTestResult(int rayHandle)
        {
            var didHitOutputArgument = new OutputArgument();
            var endPositionOutputArgument = new OutputArgument();
            var surfaceNormalOutputArgument = new OutputArgument();
            var entityHitOutputArgument = new OutputArgument();
            var resultStatus = Function.Call<int>(
                Hash.GET_SHAPE_TEST_RESULT,
                rayHandle,
                didHitOutputArgument,
                endPositionOutputArgument,
                surfaceNormalOutputArgument,
                entityHitOutputArgument
            );

            var didHit = didHitOutputArgument.GetResult<bool>();
            if (didHit)
            {
                return (
                    resultStatus,
                    true,
                    endPositionOutputArgument.GetResult<Vector3>(),
                    surfaceNormalOutputArgument.GetResult<Vector3>()
                );
            }
            return (
                resultStatus,
                false,
                new Vector3(),
                new Vector3()
            );
        }

        public static void DrawRectangle(
            Vector2 normalizedOriginScreenPosition,
            Vector2 sizeRelativeToScreen,
            Color color
        )
        {
            Function.Call(
                Hash.DRAW_RECT,
                normalizedOriginScreenPosition.X,
                normalizedOriginScreenPosition.Y,
                sizeRelativeToScreen.X,
                sizeRelativeToScreen.Y,
                color.R,
                color.G,
                color.B,
                color.A
            );
        }

        /// <returns>Normalized screen position</returns>
        public static Vector2 WorldToScreen(Vector3 worldPosition)
        {
            var screenPositionXOutputArgument = new OutputArgument();
            var screenPositionYOutputArgument = new OutputArgument();
            Function.Call(
                Hash.GET_SCREEN_COORD_FROM_WORLD_COORD,
                worldPosition.X,
                worldPosition.Y,
                worldPosition.Z,
                screenPositionXOutputArgument,
                screenPositionYOutputArgument
            );
            return new Vector2(
                screenPositionXOutputArgument.GetResult<float>(),
                screenPositionYOutputArgument.GetResult<float>()
            );
        }

        public static void DrawText(
            string text,
            int fontSizeInPixels,
            Color color,
            Vector2 normalizedScreenPosition,
            bool isCentered = true,
            bool shouldDrawShadow = false
        )
        {
            if (shouldDrawShadow)
            {
                Function.Call(Hash.SET_TEXT_DROPSHADOW, 2, 0, 0, 0, 255);
            }

            var createdString = CreateString(text);
            Function.Call(Hash.SET_TEXT_CENTRE, isCentered);
            Function.Call(Hash.SET_TEXT_SCALE, 1.0f, GetTextScale(text, fontSizeInPixels).Y);
            Function.Call(Hash._SET_TEXT_COLOR, color.R, color.G, color.B, color.A);
            Function.Call(Hash._DRAW_TEXT, createdString, normalizedScreenPosition.X, normalizedScreenPosition.Y);
        }

        public static Vector2 GetTextScale(string text, int fontSizeInPixels)
        {
            var normalizedFontSize = (float)fontSizeInPixels / GameFullFontSizeInPixels;
            var normalizedCharacterWidth = normalizedFontSize * ScreenUtility.GetAspectRatio();
            var textWidth = text.Trim().Length * normalizedCharacterWidth;
            return new Vector2(textWidth, normalizedFontSize);
        }
    }
}
