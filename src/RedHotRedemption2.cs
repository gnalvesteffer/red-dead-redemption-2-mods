using System;
using RDR2;
using RDR2.Native;

public class RedHotRedemption2 : Script
{
    private const float PlayerMovementSpeedToTriggerNormalTimeScale = 1.0f;
    private const float NormalTimeScaleDurationInSecondsWhenShooting = 0.75f;
    private const float MinimumTimeScale = 0.25f;

    private DateTime _shootTimestamp = DateTime.UtcNow;

    public RedHotRedemption2()
    {
        Tick += OnTick;
        Interval = 1;
    }

    private void OnTick(object sender, EventArgs e)
    {
        var playerPed = Game.Player.Character;
        if (Game.Player.IsAlive)
        {
            var isPlayerShooting = IsPedShooting(playerPed);
            if (isPlayerShooting)
            {
                _shootTimestamp = DateTime.UtcNow;
            }
            Game.TimeScale = (DateTime.UtcNow - _shootTimestamp).TotalSeconds <= NormalTimeScaleDurationInSecondsWhenShooting
                ? 1.0f
                : Clamp(GetPedSpeed(playerPed) / PlayerMovementSpeedToTriggerNormalTimeScale, MinimumTimeScale, 1.0f);
        }
        else
        {
            Tick -= OnTick;
            Game.TimeScale = 1.0f;
        }
    }

    private static float GetPedSpeed(Ped ped)
    {
        return Function.Call<float>(Hash.GET_ENTITY_SPEED, ped);
    }

    private static bool IsPedAlive(Ped ped)
    {
        return Function.Call<int>(Hash.GET_ENTITY_HEALTH, ped) > 0;
    }

    private static bool IsPedShooting(Ped ped)
    {
        return Function.Call<bool>(Hash.IS_PED_SHOOTING, ped);
    }

    private static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private static void Print(object text)
    {
        var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
        Function.Call(Hash._DRAW_TEXT, createdString, 5, 5);
    }
}
