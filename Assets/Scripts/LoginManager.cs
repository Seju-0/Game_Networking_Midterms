using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    public GameObject loginPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text loginMessage;

    [Header("Register UI")]
    public GameObject registerPanel;
    public TMP_InputField regUsernameInput;
    public TMP_InputField regEmailInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regRepeatPasswordInput;
    public TMP_Text registerMessage;

    private Dictionary<string, UserData> users = new Dictionary<string, UserData>();

    private void Start()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        LoadAllUsers();
    }

    // ===================== LOGIN =====================
    public void OnLoginButton()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginMessage.text = "Please enter username and password.";
            return;
        }

        if (!users.ContainsKey(username))
        {
            loginMessage.text = "Account not found. Please register.";
            return;
        }

        UserData user = users[username];

        if (password == user.Password)
        {
            loginMessage.text = "Login successful!";
            PlayerPrefs.SetString("CurrentUser", username);

            // Debug last login difference
            if (!string.IsNullOrEmpty(user.LastLoggedIn))
            {
                DateTime lastLogin = DateTime.Parse(user.LastLoggedIn);
                TimeSpan diff = DateTime.Now - lastLogin;

                string ago = "";
                if (diff.TotalDays >= 1)
                    ago = $"{Mathf.FloorToInt((float)diff.TotalDays)} days ago.";
                else if (diff.TotalHours >= 1)
                    ago = $"{Mathf.FloorToInt((float)diff.TotalHours)}h {diff.Minutes} minutes ago.";
                else
                    ago = $"{Mathf.FloorToInt((float)diff.TotalMinutes)} minutes ago.";

                Debug.Log($"{username} last logged in {ago}");
            }

            // Update last login
            user.LastLoggedIn = DateTime.Now.ToString();
            SaveUser(user);

            SceneManager.LoadScene("GameScene");
        }
        else
        {
            loginMessage.text = "Incorrect password.";
        }
    }

    // ===================== REGISTER =====================
    public void OnRegisterButton()
    {
        string username = regUsernameInput.text;
        string email = regEmailInput.text;
        string password = regPasswordInput.text;
        string repeatPass = regRepeatPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPass))
        {
            registerMessage.text = "Please fill in all fields.";
            return;
        }

        if (password != repeatPass)
        {
            registerMessage.text = "Passwords do not match.";
            return;
        }

        if (users.ContainsKey(username))
        {
            registerMessage.text = "Username already exists.";
            return;
        }

        UserData newUser = new UserData
        {
            Username = username,
            Email = email,
            Password = password,
            Wins = 0,
            Losses = 0,
            AccountCreationDate = DateTime.Now.ToString(),
            LastLoggedIn = DateTime.Now.ToString()
        };

        users.Add(username, newUser);
        SaveUser(newUser);

        registerMessage.text = "Account created successfully!";
        Invoke(nameof(SwitchToLogin), 1.5f);
    }

    // ===================== ACCOUNT DELETION =====================
    public void DeleteAccount(string username)
    {
        if (users.ContainsKey(username))
        {
            users.Remove(username);
            PlayerPrefs.DeleteKey(username + "_Data");
            PlayerPrefsUtility.RemoveUserKey(username + "_Data");
            PlayerPrefs.Save();
            Debug.Log($"{username} account deleted.");
        }
        else
        {
            Debug.LogWarning("Account not found.");
        }
    }
    public void OnDeleteAccountButton()
    {
        string currentUser = PlayerPrefs.GetString("CurrentUser");
        DeleteAccount(currentUser);
    }

    // ===================== JSON SAVE/LOAD =====================
    private void SaveUser(UserData user)
    {
        string json = JsonUtility.ToJson(user);
        PlayerPrefs.SetString(user.Username + "_Data", json);
        PlayerPrefsUtility.AddUserKey(user.Username + "_Data");
        PlayerPrefs.Save();
    }

    private void LoadAllUsers()
    {
        users.Clear();
        foreach (string key in PlayerPrefsKeys())
        {
            if (key.EndsWith("_Data"))
            {
                string json = PlayerPrefs.GetString(key);
                UserData user = JsonUtility.FromJson<UserData>(json);
                users[user.Username] = user;
            }
        }
    }
    public float GetWinRate(UserData user)
    {
        int totalGames = user.Wins + user.Losses;
        return totalGames == 0 ? 0 : (float)user.Wins / totalGames * 100f;
    }

    private List<string> PlayerPrefsKeys()
    {
        // Unity doesn’t provide PlayerPrefs.GetAllKeys(), so we track keys manually if needed.
        // For simplicity, we assume keys end with "_Data"
        List<string> keys = new List<string>();
        foreach (string key in PlayerPrefsUtility.GetAllKeys())
            keys.Add(key);
        return keys;
    }

    // ===================== PANEL SWITCHES =====================
    public void SwitchToRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        loginMessage.text = "";
    }

    public void SwitchToLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        registerMessage.text = "";
    }
}
