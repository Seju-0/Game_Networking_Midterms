using UnityEngine;
using UnityEngine.AI;

public class Basic_AI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform pointB;

    // Reference to Player or object to follow
    public string playerTag = "Player";
    private Transform player;

    [Header("AI Color Change")]
    public Renderer playerRendererOverride;   // optional: drag the player's Renderer here

    private Renderer[] aiRenderers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(pointB.position);

        // Get all child renderers (important if there are multiple meshes)
        aiRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("Destination reached");
        }
    }

    // Trigger-based bump (detecting collision with the player)
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        player = other.transform;

        // Copy color from player
        var src = playerRendererOverride ? playerRendererOverride : other.GetComponentInChildren<Renderer>();
        if (src != null) ApplyColorToAI(GetColor(src));

        // Start following the player
        agent.SetDestination(player.position);
    }

    // Get the color from the player's material
    static Color GetColor(Renderer r)
    {
        // Access sharedMaterial for the prefab, but create a copy of the material for runtime modification
        var m = r.material; // this automatically creates a copy of the material at runtime
        if (m == null) return Color.white;

        // Handle both standard and URP materials
        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor"); // URP/Lit
        if (m.HasProperty("_Color")) return m.GetColor("_Color");     // Standard/Legacy
        return Color.white;
    }

    // Apply the color to all materials on the AI's mesh
    void ApplyColorToAI(Color c)
    {
        foreach (var r in aiRenderers)
        {
            foreach (var m in r.materials) // Use materials (this automatically creates material instances for each renderer)
            {
                if (m == null) continue;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            }
        }
    }
}
