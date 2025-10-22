using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if(Runner.LocalPlayer == player)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 5, 0), Quaternion.identity);
        }
    }
}
