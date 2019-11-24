using System;
using RDR2;
using RDR2.Native;

public class RagdollRedemption2 : Script
{
    private const int RagdollDamageThreshold = 20;

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
        if (_previousPlayerHealth - currentPlayerHealth >= RagdollDamageThreshold)
        {
            RagdollPed(playerPed);
        }
        _previousPlayerHealth = currentPlayerHealth;
    }

    private void RagdollPed(Ped ped)
    {
        Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, 5000, 5000, 0, false, false, false);
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
