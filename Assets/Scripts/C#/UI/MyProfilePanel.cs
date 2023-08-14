using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyProfilePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameField;

    [SerializeField]
    private TMP_InputField emailField;

    [SerializeField]
    private Button avatarSettingsBtn;

    private void Awake()
    {
        avatarSettingsBtn.onClick.AddListener(TransitionToAvatarSettingsScene);
        if (UserProfile.Instance.userData != null)
        {
            Initialize();
        }
        else
        {
            EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
                new Action<bool>((bool s) =>
                {
                    if (s)
                        Initialize();
                }));
        }
    }

    private void Initialize()
    {
        usernameField.text = UserProfile.Instance.userData.Username;
        emailField.text = UserProfile.Instance.userData.Email;
    }

    private void TransitionToAvatarSettingsScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
