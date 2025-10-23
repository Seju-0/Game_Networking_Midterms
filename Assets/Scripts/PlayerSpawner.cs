using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Tooltip("Assign Player1–4 prefabs here")]
    public GameObject[] PlayerPrefabs;

    [Tooltip("Spawn radius from the center of map")]
    public float spawnRadius = 8f;

    public void PlayerJoined(PlayerRef player)
    {
        // Spawn only for this local player
        if (Runner.LocalPlayer != player) return;

        int idx = Mathf.Abs(player.PlayerId) % Mathf.Max(1, PlayerPrefabs.Length);
        GameObject prefab = PlayerPrefabs[idx];

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(circle.x, 1f, circle.y); // y=1 keeps above floor

        Runner.Spawn(prefab, spawnPos, Quaternion.identity, player);
    }
}
