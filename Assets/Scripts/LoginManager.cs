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

    // -------------------------
    // LOGIN FUNCTION
    // -------------------------
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
            // Save the logged-in user
            PlayerPrefs.SetString("CurrentUser", username);

            // Load next scene
            SceneManager.LoadScene("GameScene");
        }
    }

    // -------------------------
    // REGISTER FUNCTION
    // -------------------------
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

        // If registered successfully, switch back to login screen
        if (res.success)
        {
            Invoke(nameof(SwitchToLogin), 1.5f);
        }
    }

    // -------------------------
    // DELETE ACCOUNT FUNCTION
    // -------------------------
    public void OnDeleteAccountButton()
    {
        string currentUser = PlayerPrefs.GetString("CurrentUser", "");

        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogWarning("No current user saved in PlayerPrefs.");
            loginMessage.text = "No account logged in.";
            return;
        }

        // Call the delete API
        var res = PlayerApi.Instance.DeletePlayer(currentUser);
        loginMessage.text = res.message;

        if (res.success)
        {
            // Clear PlayerPrefs
            PlayerPrefs.DeleteKey("CurrentUser");

            // Reset UI
            usernameInput.text = "";
            passwordInput.text = "";

            loginMessage.text = "Account deleted.";
        }
    }

    // -------------------------
    // UI SWITCHES
    // -------------------------
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
