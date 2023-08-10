using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField roomtitleField;

    [SerializeField]
    private GameObject friendsOnlyToggle;

    [SerializeField]
    private GameObject privateToggle;

    [SerializeField]
    private GameObject createRoomButton;

    private void Awake()
    {
        createRoomButton.GetComponent<Button>().onClick.AddListener(CreateRoom);
    }

    private async void  CreateRoom()
    {
        string roomTitle = roomtitleField.text;
        if (roomTitle != "")
        {
            await Server.CreateRoom(new Mvm.CreateRoomRequest
            {
                Title = roomTitle,
                IsPrivate = privateToggle.GetComponent<Toggle>().isOn,
                FriendsOnly = friendsOnlyToggle.GetComponent<Toggle>().isOn,
            });
            UserProfile.Instance.GetMyProfile(true);
        }
    }
}