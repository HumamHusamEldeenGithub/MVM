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
    private GameObject roomPanel;

    private void OnEnable()
    {
        joinRoomBtn.onClick.AddListener(JoinRoom);
    }

    void JoinRoom()
    {
        string roomId = roomIdField.text;
        if (roomId.Length != 0)
        {
            SignalingServerController.Instance.ConnectToRoom(roomId);
        }
    }
}
