using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
	#region Attributes

	[SerializeField]
	private TMP_InputField usernameInp;

    [SerializeField]
    private TMP_InputField passwordInp;

    [SerializeField]
    private Button loginBtn;

    [SerializeField]
    private GameObject errorMsgGO;

    #endregion
    private void OnEnable()
    {
        if(!usernameInp || !passwordInp || !loginBtn || !errorMsgGO)
        {
            Debug.LogWarning("Username or Password isn't populated");
        }

        loginBtn.onClick.AddListener(Login);

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
            new Action<bool>(OnLogin));

    }

    private void Login()
    {
        errorMsgGO.SetActive(false);
        EventsPool.Instance.InvokeEvent(typeof(SubmitLoginEvent),
            new object[] { usernameInp.text, passwordInp.text });

    }

    private void OnLogin(bool success)
    {
        if(!success)
        {
            Debug.Log("Active");
            errorMsgGO.gameObject.SetActive(true);
        }
    }

}
