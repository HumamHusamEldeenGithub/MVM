using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchForRoomPanel : MonoBehaviour
{
    [SerializeField]
    private Transform roomsScrollView;

    [SerializeField]
    private TMP_InputField searchField;

    [SerializeField]
    private GameObject roomRowPrefab;

    [SerializeField]
    private GameObject searchButton;

    private void Awake()
    {
        searchButton.transform.GetComponent<Button>().onClick.AddListener(SearchForRoom);
    }

    private async void SearchForRoom()
    {
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
        int childCount = roomsScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = roomsScrollView.GetChild(i);
            Destroy(child.gameObject);
        }

        string searchQuery = searchField.text.Trim();

        var rooms = await Server.GetRooms(searchQuery);

        if (rooms != null)
        {

            foreach (var room in rooms.Rooms)
            {
                var element = Instantiate(roomRowPrefab);

                element.transform.GetChild(0).GetComponent<TMP_Text>().text = room.Title;
                element.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Id));

                element.transform.SetParent(roomsScrollView);
            }
        }
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    void JoinRoom(string roomId)
    {
        if (roomId.Length != 0)
        {
            SignalingServerController.Instance.ConnectToRoom(roomId);
        }
    }
}
