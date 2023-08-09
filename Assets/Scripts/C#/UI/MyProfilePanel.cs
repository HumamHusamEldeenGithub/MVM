using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MyProfilePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameField;

    [SerializeField]
    private TMP_InputField emailField;

    private void OnEnable()
    {
        usernameField.text = UserProfile.Instance.userData.Username;
        emailField.text = UserProfile.Instance.userData.Email;
    }
}
