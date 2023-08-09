using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginSignupPanel : MonoBehaviour
{
    #region Attributes

    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private TMP_InputField usernameInp;

    [SerializeField]
    private TMP_InputField passwordInp;

    [SerializeField]
    private Button loginBtn;

    [SerializeField]
    private TMP_InputField signupUsernameField;

    [SerializeField]
    private TMP_InputField signupPhonenumberField;

    [SerializeField]
    private TMP_InputField signupEmailField;

    [SerializeField]
    private TMP_InputField signupPasswordField;

    [SerializeField]
    private TMP_InputField signipPasswordConfirmationField;

    [SerializeField]
    private Button signupBtn;

    [SerializeField]
    private GameObject errorMsgGO;

    #endregion
    private void OnEnable()
    {
        if (!usernameInp || !passwordInp || !loginBtn )
        {
            Debug.LogWarning("Username or Password isn't populated");
        }

        loginBtn.onClick.AddListener(Login);
        signupBtn.onClick.AddListener(Signup);

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
            new Action<bool>(OnLogin));

    }

    private void Login()
    {
        //errorMsgGO.SetActive(false);
        EventsPool.Instance.InvokeEvent(typeof(SubmitLoginEvent),
            new object[] { usernameInp.text, passwordInp.text });

        signupBtn.interactable = false;

    }

    private void Signup()
    {
        //errorMsgGO.SetActive(false);
        EventsPool.Instance.InvokeEvent(typeof(SubmitCreateUserEvent),
            new object[] { signupUsernameField.text,signupEmailField.text,signupPhonenumberField.text, signupPasswordField.text });

        loginBtn.interactable = false;

    }

    private void OnLogin(bool success)
    {
        if (!success)
        {
            Debug.Log("Active");
            //errorMsgGO.gameObject.SetActive(true);
        }
        loginBtn.interactable = true;

        this.gameObject.SetActive(false);
        mainMenuPanel.SetActive(true);

    }
}
