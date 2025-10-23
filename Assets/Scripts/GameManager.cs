using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fusion;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Match Settings")]
    [Tooltip("Round length in seconds")]
    public float matchDuration = 60f;

    [Tooltip("How often (seconds) to refresh the scoreboard UI locally")]
    public float scoreboardRefreshInterval = 0.25f;

    [Header("UI (TextMeshPro)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreboardText;
    public TextMeshProUGUI resultText;

    [Header("UI Buttons")]
    public GameObject restartButton;

    [Networked] private float RemainingTime { get; set; }
    [Networked] private NetworkBool MatchEnded { get; set; }

    private NPC_RoamColor[] npcs;
    private readonly List<PlayerMovement> players = new();

    private float _scoreRefreshTimer;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            RemainingTime = Mathf.Max(1f, matchDuration);
            MatchEnded = false;
        }

        npcs = FindObjectsByType<NPC_RoamColor>(FindObjectsSortMode.None);
        RefreshPlayersList();

        UpdateTimerUI();
        UpdateScoreboardUI(force: true);

        if (resultText)
        {
            resultText.gameObject.SetActive(false);
            resultText.alpha = 0f;
            resultText.transform.localScale = Vector3.one * 0.5f;
        }

        if (restartButton)
            restartButton.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || MatchEnded)
            return;

        RemainingTime -= Runner.DeltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            MatchEnded = true;

            int winnerIndex = DetermineWinnerIndex(out Color winnerColor, out int winnerScore);

            RPC_LockAllPlayers(true);
            RPC_AnnounceResults(winnerIndex, winnerColor, winnerScore);
        }
    }

    private void Update()
    {
        UpdateTimerUI();

        _scoreRefreshTimer += Time.unscaledDeltaTime;
        if (_scoreRefreshTimer >= scoreboardRefreshInterval)
        {
            _scoreRefreshTimer = 0f;
            UpdateScoreboardUI();
        }
    }

    // ---------------- UI ----------------

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        float t = Mathf.Max(0f, RemainingTime);
        timerText.text = $"Time: {t:0.0}s";
    }

    private void UpdateScoreboardUI(bool force = false)
    {
        if (scoreboardText == null) return;

        if (players.Count == 0 || force)
            RefreshPlayersList();

        var sb = new StringBuilder();
        sb.Append("<b> SCOREBOARD</b>\n\n");

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p == null) continue;

            var rend = p.GetComponentInChildren<Renderer>();
            var playerColor = ReadRendererColor(rend, Color.white);
            int score = CountNPCsMatchingColor(playerColor);

            string hex = ColorUtility.ToHtmlStringRGB(playerColor);
            sb.Append($"<color=#{hex}>Player {i + 1}</color>: {score}\n");
        }

        scoreboardText.richText = true;
        scoreboardText.text = sb.ToString();
    }

    // ---------------- Logic ----------------

    private void RefreshPlayersList()
    {
        players.Clear();
        players.AddRange(FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None));
    }

    private int CountNPCsMatchingColor(Color playerColor)
    {
        if (npcs == null || npcs.Length == 0) return 0;

        int count = 0;
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            var rend = npc.GetComponentInChildren<Renderer>();
            if (rend == null) continue;

            Color npcColor = ReadRendererColor(rend, Color.white);
            if (ApproximatelyEqualColor(playerColor, npcColor))
                count++;
        }
        return count;
    }

    private int DetermineWinnerIndex(out Color winnerColor, out int winnerScore)
    {
        winnerScore = -1;
        winnerColor = Color.white;
        int winnerIdx = -1;

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p == null) continue;

            var rend = p.GetComponentInChildren<Renderer>();
            var c = ReadRendererColor(rend, Color.white);
            int score = CountNPCsMatchingColor(c);

            if (score > winnerScore)
            {
                winnerScore = score;
                winnerIdx = i;
                winnerColor = c;
            }
        }

        return winnerIdx;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AnnounceResults(int winnerIndex, Color winnerColor, int winnerScore)
    {
        if (scoreboardText)
        {
            string hex = ColorUtility.ToHtmlStringRGB(winnerColor);
            scoreboardText.text += $"\n\n<color=#{hex}> Player {winnerIndex + 1} WINS</color> with {winnerScore} NPCs!";
        }

        int localIndex = -1;
        var localPlayer = FindLocalPlayer();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == localPlayer)
            {
                localIndex = i;
                break;
            }
        }

        if (resultText)
        {
            resultText.gameObject.SetActive(true);

            if (localIndex == winnerIndex)
            {
                resultText.text = " YOU WIN!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = " YOU LOSE!";
                resultText.color = Color.red;
            }

            resultText.StopAllCoroutines();
            resultText.StartCoroutine(PopOutResult(resultText));
        }

        // Show restart button only for host, now that results are out
        if (restartButton)
            restartButton.SetActive(Object.HasStateAuthority);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LockAllPlayers(bool locked)
    {
        RefreshPlayersList();
        foreach (var p in players)
        {
            if (p == null) continue;
            p.enabled = !locked;
        }
    }

    private PlayerMovement FindLocalPlayer()
    {
        foreach (var p in players)
        {
            if (p != null && p.HasStateAuthority)
                return p;
        }
        return null;
    }

    // ---------------- Helpers ----------------

    private static Color ReadRendererColor(Renderer r, Color fallback)
    {
        if (r == null) return fallback;
        Material m = null;
        try { m = r.material; } catch { }
        if (m == null) m = r.sharedMaterial;
        if (m == null) return fallback;

        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
        if (m.HasProperty("_Color")) return m.GetColor("_Color");
        return fallback;
    }

    private static bool ApproximatelyEqualColor(Color a, Color b)
    {
        const float eps = 0.05f;
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps;
    }

    private IEnumerator PopOutResult(TextMeshProUGUI text)
    {
        float duration = 0.7f;
        float t = 0f;

        text.alpha = 0f;
        text.transform.localScale = Vector3.one * 0.5f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            text.alpha = Mathf.SmoothStep(0f, 1f, k);
            float s = Mathf.SmoothStep(0.5f, 1.1f, k);
            text.transform.localScale = Vector3.one * s;

            yield return null;
        }
        text.transform.localScale = Vector3.one;
    }

    // 🌀 Host-only restart: resets round state and NPC colors
    public void RestartGame()
    {
        if (!Object.HasStateAuthority)
            return;

        // Unlock players
        RPC_LockAllPlayers(false);

        // Reset networked state
        RemainingTime = matchDuration;
        MatchEnded = false;

        // Reset result UI
        if (resultText)
        {
            resultText.gameObject.SetActive(false);
            resultText.alpha = 0f;
            resultText.transform.localScale = Vector3.one * 0.5f;
        }

        // 🔄 Reset all NPCs back to default color/unclaimed
        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            var rend = npc.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                foreach (var m in rend.materials)
                {
                    if (m == null) continue;
                    // Reset to white or any base color you want
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
                }
            }

            // Reset claim flag
            npc.SetHasChangedColor(false);
        }

        // Rebuild scoreboard to 0
        UpdateScoreboardUI(force: true);

        // Hide restart button until next results
        if (restartButton)
            restartButton.SetActive(false);
    }
}
