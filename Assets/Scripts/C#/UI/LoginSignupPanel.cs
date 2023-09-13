using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginSignupPanel : MonoBehaviour
{
    #region Attributes

    [SerializeField]
    private Animator loginPanel;

    [SerializeField]
    private Animator takePicturePanel;

    [SerializeField]
    private Animator createAccountPanel;

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
    private CameraPanel cameraPanel;

    #endregion
    private void OnEnable()
    {
        loginPanel.SetTrigger("FadeIn");
        cameraPanel.ActiveEventListener();
        if (UserProfile.Instance.userData != null && UserProfile.Instance.userData.Token != "")
        {
            OnLogin(true);
            return;
        }

        if (!usernameInp || !passwordInp || !loginBtn)
        {
            Debug.LogWarning("Username or Password isn't populated");
        }

        loginBtn.onClick.AddListener(Login);
        signupBtn.onClick.AddListener(Signup);

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
            new Action<bool>(OnLogin));

        EventsPool.Instance.AddListener(typeof(ShowTakePictuePanelEvent), new Action(OnCreateAccountSuccess));

        EventsPool.Instance.AddListener(typeof(LogoutEvent), new Action(OnLogout));

    }

    private void Login()
    {
        //errorMsgGO.SetActive(false);
        EventsPool.Instance.InvokeEvent(typeof(SubmitLoginEvent),
            new object[] { usernameInp.text, passwordInp.text,"" });

        signupBtn.interactable = false;

    }

    private void Signup()
    {
        if(signupPasswordField.text != signipPasswordConfirmationField.text)
        {
            EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Passwords do not match", 2, Color.black);
            return;
        }

        EventsPool.Instance.InvokeEvent(typeof(SubmitCreateUserEvent),
            new object[] { signupUsernameField.text,signupEmailField.text,signupPhonenumberField.text, signupPasswordField.text });

        loginBtn.interactable = false;

    }

    private void OnCreateAccountSuccess()
    {
        createAccountPanel.SetTrigger("FadeOut");
        takePicturePanel.SetTrigger("FadeIn");
    }

    private void OnLogin(bool success)
    {
        if (!success)
        {
            loginBtn.interactable = true;
        }
    }

    private void OnLogout()
    {
        if (loginPanel != null && loginPanel.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"))
            loginPanel.SetTrigger("FadeIn");

        if (createAccountPanel != null && createAccountPanel.GetCurrentAnimatorStateInfo(0).IsName("FadeIn"))
            createAccountPanel.SetTrigger("FadeOut");

        if (takePicturePanel != null && takePicturePanel.GetCurrentAnimatorStateInfo(0).IsName("FadeIn"))
            takePicturePanel.SetTrigger("FadeOut");

        loginBtn.interactable = true;
        signupBtn.interactable = true;

    }
}
