using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private void Start()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
    }

    // ===================== LOGIN (POST /api/player/login) =====================
    public void OnLoginButton()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginMessage.text = "Please enter username and password.";
            return;
        }

        var req = new PlayerApi.LoginRequest
        {
            username = username,
            password = password
        };

        var res = PlayerApi.Instance.Login(req);
        loginMessage.text = res.message;

        if (res.success)
        {
            // Go to game scene
            SceneManager.LoadScene("GameScene");
        }
    }

    // ===================== REGISTER (POST /api/player/register) =====================
    public void OnRegisterButton()
    {
        string username = regUsernameInput.text;
        string email = regEmailInput.text;
        string password = regPasswordInput.text;
        string repeatPass = regRepeatPasswordInput.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(repeatPass))
        {
            registerMessage.text = "Please fill in all fields.";
            return;
        }

        if (password != repeatPass)
        {
            registerMessage.text = "Passwords do not match.";
            return;
        }

        var req = new PlayerApi.RegisterRequest
        {
            username = username,
            email = email,
            password = password
        };

        var res = PlayerApi.Instance.Register(req);
        registerMessage.text = res.message;

        if (res.success)
        {
            // Switch back to login after short delay
            Invoke(nameof(SwitchToLogin), 1.5f);
        }
    }

    // ===================== DELETE (DELETE /api/delete/:playerId) =====================
    public void OnDeleteAccountButton()
    {
        string currentUser = PlayerPrefs.GetString("CurrentUser", "");
        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogWarning("No current user to delete.");
            return;
        }

        var res = PlayerApi.Instance.DeletePlayer(currentUser);
        Debug.Log(res.message);

        // Optional: go back to login screen or reset UI
        usernameInput.text = "";
        passwordInput.text = "";
        loginMessage.text = "Account deleted.";
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
