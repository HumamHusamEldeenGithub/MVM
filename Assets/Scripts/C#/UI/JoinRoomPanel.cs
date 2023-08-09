using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class JoinRoomPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField roomIdField;

    [SerializeField]
    private Button joinRoomBtn;

    [SerializeField]
    private SignalingServerController signalingServer;

    [SerializeField]
    private GameObject roomPanel;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(RoomConnectedStatusEvent), new Action<bool>(OnRoomConnected));
    }

    private void OnEnable()
    {
        joinRoomBtn.onClick.AddListener(JoinRoom);
    }

    void JoinRoom()
    {
        string roomId = roomIdField.text;
        if (roomId.Length != 0)
        {
            signalingServer.ConnectToRoom(roomId);
        }
    }

    private void OnRoomConnected(bool success)
    {
        if (success)
        {
            this.gameObject.SetActive(false);
            roomPanel.SetActive(true);
        }
    }
}
