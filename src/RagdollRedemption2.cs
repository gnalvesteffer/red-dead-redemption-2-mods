using System;
using RDR2;
using RDR2.Native;

public class RagdollRedemption2 : Script
{
    private enum RagdollType
    {
        Normal = 0,
        FallWithStiffLegs = 1,
        NarrowLegStumble = 2,
        WideLegStumble = 3,
        LENGTH = 4,
    }

    private const int RagdollDamageThreshold = 15;
    private const int MinimumRagdollDamageTimeScalar = 5;
    private const int MaximumRagdollDamageTimeScalar = 25;

    private static readonly Random _random = new Random();

    private int _previousPlayerHealth;

    public RagdollRedemption2()
    {
        Tick += OnTick;
        Interval = 1;
    }

    private void OnTick(object sender, EventArgs e)
    {
        var playerPed = Game.Player.Character;
        var currentPlayerHealth = GetPedHealth(playerPed);
        var deltaPlayerHealth = _previousPlayerHealth - currentPlayerHealth;
        if (deltaPlayerHealth >= RagdollDamageThreshold)
        {
            RagdollPed(
                playerPed,
                deltaPlayerHealth * MinimumRagdollDamageTimeScalar,
                deltaPlayerHealth * MaximumRagdollDamageTimeScalar,
                (RagdollType)_random.Next((int)RagdollType.LENGTH)
            );
        }
        _previousPlayerHealth = currentPlayerHealth;
    }

    private void RagdollPed(Ped ped, int minDuration, int maxDuration, RagdollType ragdollType)
    {
        Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, minDuration, maxDuration, (int)ragdollType, false, false, false);
    }

    private void Print(object text)
    {
        var createdString = Function.Call<string>(Hash.CREATE_STRING, 10, "LITERAL_STRING", text.ToString());
        Function.Call(Hash._DRAW_TEXT, createdString, 5, 5);
    }

    private int GetPedHealth(Ped ped)
    {
        return Function.Call<int>(Hash.GET_ENTITY_HEALTH, ped);
    }
}
