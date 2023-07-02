using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject mainPanel;

    private void Awake()
    {
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(OnLogin));
    }

    private void OnLogin(bool sucess)
    {
        if (sucess)
        {
            loginPanel.SetActive(false);
            mainPanel.SetActive(true);
        }
    }
}
