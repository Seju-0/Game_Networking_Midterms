using UnityEngine;
using TMPro;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text usernameText;
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text winRateText;
    public TMP_Text lastLoginText;
    public TMP_Text creationDateText;  //  added this

    private UserData currentUser;

    void Start()
    {
        string username = PlayerPrefs.GetString("CurrentUser", "");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("No current user logged in.");
            return;
        }

        string json = PlayerPrefs.GetString(username + "_Data", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("User data not found for " + username);
            return;
        }

        currentUser = JsonUtility.FromJson<UserData>(json);
        DisplayUserData();
        InvokeRepeating(nameof(RefreshStatsFromPrefs), 5f, 5f);

    }

    public void RefreshStatsFromPrefs()
    {
        string username = PlayerPrefs.GetString("CurrentUser", "");
        if (string.IsNullOrEmpty(username)) return;

        string json = PlayerPrefs.GetString(username + "_Data", "");
        if (string.IsNullOrEmpty(json)) return;

        currentUser = JsonUtility.FromJson<UserData>(json);
        DisplayUserData();
    }


    void DisplayUserData()
    {
        if (currentUser == null) return;

        usernameText.text = currentUser.Username;
        winsText.text = $"Wins: {currentUser.Wins}";
        lossesText.text = $"Losses: {currentUser.Losses}";

        //  Compute win rate dynamically
        float rate = GetWinRate(currentUser);
        winRateText.text = $"Win Rate: {rate:0.0}%";

        //  Account creation date
        creationDateText.text = $"Created: {currentUser.AccountCreationDate}";

        //  Last login time difference
        if (!string.IsNullOrEmpty(currentUser.LastLoggedIn))
        {
            DateTime lastLogin = DateTime.Parse(currentUser.LastLoggedIn);
            TimeSpan diff = DateTime.Now - lastLogin;

            string ago = "";
            if (diff.TotalDays >= 1)
                ago = $"{Mathf.FloorToInt((float)diff.TotalDays)} days ago";
            else if (diff.TotalHours >= 1)
                ago = $"{Mathf.FloorToInt((float)diff.TotalHours)}h {diff.Minutes}m ago";
            else
                ago = $"{Mathf.FloorToInt((float)diff.TotalMinutes)} minutes ago";

            lastLoginText.text = $"Last Login: {ago}";
        }
    }

    float GetWinRate(UserData user)
    {
        int total = user.Wins + user.Losses;
        return total == 0 ? 0 : (float)user.Wins / total * 100f;
    }
}
