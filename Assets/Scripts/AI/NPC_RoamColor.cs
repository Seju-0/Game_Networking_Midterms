using UnityEngine;
using UnityEngine.AI;
using Fusion;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider))]
public class NPC_RoamColor : NetworkBehaviour
{
    [Header("Roaming Settings")]
    public float roamRadius = 15f;
    public float minWaitTime = 1.5f;
    public float maxWaitTime = 3.5f;

    private NavMeshAgent agent;
    private Renderer[] renderers;

    // networked flag so all players see color change
    [Networked] private bool HasChangedColor { get; set; }

    private Vector3 homePos;
    private Coroutine roamRoutine;

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        renderers = GetComponentsInChildren<Renderer>(true);

        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        homePos = transform.position;
        HasChangedColor = false;

        if (Object.HasStateAuthority)
            roamRoutine = Runner.StartCoroutine(RoamLoop());
    }

    IEnumerator RoamLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (!agent.isOnNavMesh) continue;

            Vector3 randomDir = Random.insideUnitSphere * roamRadius + homePos;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasChangedColor) return; // already claimed
        if (!Object.HasStateAuthority) return; // only host changes color
        if (!other.CompareTag("Player")) return;

        // Try to copy color from player's renderer
        Renderer playerRenderer = other.GetComponentInChildren<Renderer>();
        if (playerRenderer == null) return;

        Color playerColor = GetColorFromMaterial(playerRenderer);
        RPC_ApplyColor(playerColor);

        HasChangedColor = true; // lock it
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ApplyColor(Color c)
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m == null) continue;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            }
        }
    }

    private Color GetColorFromMaterial(Renderer r)
    {
        var mat = r.sharedMaterial; // safe access, no prefab modification
        if (mat == null) return Color.white;
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
        return Color.white;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, roamRadius);
    }

    public void SetHasChangedColor(bool value)
    {
        HasChangedColor = value;
    }
}
