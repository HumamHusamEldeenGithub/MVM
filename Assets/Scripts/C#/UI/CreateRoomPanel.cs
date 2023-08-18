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
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
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
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
        EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Room Created Sucessfully", 3, Color.black);
    }
}
