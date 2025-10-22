using UnityEngine;
using TMPro;
using UnityEngine.UI;
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

    //Login
    public void OnLoginButton()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginMessage.text = "Please enter username and password.";
            return;
        }

        if (!PlayerPrefs.HasKey(username + "_Password"))
        {
            loginMessage.text = "Account not found. Please register.";
            return;
        }

        string storedPass = PlayerPrefs.GetString(username + "_Password");
        if (password == storedPass)
        {
            loginMessage.text = "Login successful!";
            PlayerPrefs.SetString("CurrentUser", username);
            PlayerPrefs.Save();

            SceneManager.LoadScene("GameScene");
        }
        else
        {
            loginMessage.text = "Incorrect password.";
        }
    }

    //Register
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

        if (PlayerPrefs.HasKey(username + "_Password"))
        {
            registerMessage.text = "Username already exists.";
            return;
        }

        PlayerPrefs.SetString(username + "_Email", email);
        PlayerPrefs.SetString(username + "_Password", password);
        PlayerPrefs.Save();

        registerMessage.text = "Account created successfully!";

        Invoke(nameof(SwitchToLogin), 1.5f);
    }

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
