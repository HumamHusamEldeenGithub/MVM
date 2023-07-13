using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private GameObject roomPanel;

    private void Awake()
    {
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(OnLogin));
        EventsPool.Instance.AddListener(typeof(RoomConnectedStatusEvent), new Action<bool>(OnRoomConnected));
    }

    private void OnLogin(bool sucess)
    {
        if (sucess)
        {
            loginPanel.SetActive(false);
            mainPanel.SetActive(true);
        }
    }

    private void OnRoomConnected(bool success)
    {
        if(success)
        {
            roomPanel.SetActive(true);
            mainPanel.SetActive(false);
        }
    }
}
