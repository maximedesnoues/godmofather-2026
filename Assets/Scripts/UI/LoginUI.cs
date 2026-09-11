using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    // [SerializeField] private TMP_InputField usernameInput;
    // [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject applicationMenu;

    public void Login()
    {
        // if (string.IsNullOrWhiteSpace(usernameInput.text)) return;
        // if (string.IsNullOrWhiteSpace(passwordInput.text)) return;

        loginMenu.SetActive(false);
        applicationMenu.SetActive(true);
    }
}