using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using RDR2;
using RDR2.Math;
using RDR2.Native;

public class PointCloudCapture : Script
{
    public struct PointCloudKey
    {
        public float X;
        public float Y;

        public PointCloudKey(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", X, Y);
        }
    }

    public struct PointCloudValue
    {
        public float X;
        public float Y;
        public float Z;

        public PointCloudValue(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return Z.ToString();
        }
    }

    private const string PointCloudFilePath = "scripts/point-cloud.xyz";
    private const float PointCloudCaptureRadius = 100.0f;
    private const float PointCloudCapturePrecision = 0.25f;
    private static readonly TimeSpan CaptureCooldownDuration = TimeSpan.FromSeconds(1.0f);

    private readonly Dictionary<PointCloudKey, PointCloudValue> _pointCloud = new Dictionary<PointCloudKey, PointCloudValue>();
    private DateTime _previousCaptureTimestamp = DateTime.UtcNow;

    private bool CanCapturePointCloud
    {
        get { return DateTime.UtcNow - _previousCaptureTimestamp > CaptureCooldownDuration; }
    }

    public PointCloudCapture()
    {
        Tick += OnTick;
        Interval = 1;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (Game.IsKeyPressed(Keys.F1) && CanCapturePointCloud)
        {
            _previousCaptureTimestamp = DateTime.UtcNow;
            CapturePointCloud();
            SavePointCloud();
            Print("Point Cloud Captured");
        }
    }

    private void CapturePointCloud()
    {
        var playerPosition = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, Game.Player.Character);
        for (var x = playerPosition.X - PointCloudCaptureRadius / 2; x < playerPosition.X + PointCloudCaptureRadius / 2; x += PointCloudCapturePrecision)
        for (var y = playerPosition.Y - PointCloudCaptureRadius / 2; y < playerPosition.Y + PointCloudCaptureRadius / 2; y += PointCloudCapturePrecision)
        {
            var pointCloudKey = new PointCloudKey(x, y);
            if (!_pointCloud.ContainsKey(pointCloudKey))
            {
                var groundHeightAtPlayerPosition = World.GetGroundHeight(new Vector2(x, y));
                var pointCloudValue = new PointCloudValue(pointCloudKey.X, pointCloudKey.Y, groundHeightAtPlayerPosition);
                _pointCloud.Add(pointCloudKey, pointCloudValue);
            }
        }
    }

    private void LoadPointCloud()
    {
        var serializedPointCloudLines = File.ReadAllLines(PointCloudFilePath);
        foreach (var serializedPointCloudLine in serializedPointCloudLines)
        {
            var parts = serializedPointCloudLine.Split(' ');
            var x = float.Parse(parts[0]);
            var y = float.Parse(parts[1]);
            var z = float.Parse(parts[2]);
            var pointCloudKey = new PointCloudKey(x, y);
            var pointCloudValue = new PointCloudValue(x, y, z);
            if (!_pointCloud.ContainsKey(pointCloudKey))
            {
                _pointCloud.Add(pointCloudKey, pointCloudValue);
            }
        }
    }

    private void SavePointCloud()
    {
        var serializedPointCloudValues = new List<string>();
        foreach (var pointCloudValue in _pointCloud.Values)
        {
            serializedPointCloudValues.Add(
                string.Format(
                    "{0} {1} {2}",
                    pointCloudValue.X,
                    pointCloudValue.Y,
                    pointCloudValue.Z
                )
            );
        }
        File.AppendAllLines(PointCloudFilePath, serializedPointCloudValues);
        _pointCloud.Clear();
    }

    private static void Print(object text)
    {
        var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
        Function.Call(Hash._LOG_SET_CACHED_OBJECTIVE, createdString);
        Function.Call(Hash._LOG_PRINT_CACHED_OBJECTIVE);
        Function.Call(Hash._LOG_CLEAR_CACHED_OBJECTIVE);
    }
}
