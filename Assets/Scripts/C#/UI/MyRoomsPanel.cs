using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomsPanel : MonoBehaviour
{
    [SerializeField]
    private Transform roomsScrollView;

    [SerializeField]
    private GameObject roomRowPrefab;

    [SerializeField]
    private SignalingServerController signalingServer;

    private void OnEnable()
    {
        int childCount = roomsScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = roomsScrollView.GetChild(i);
            Destroy(child.gameObject);
        }

        Debug.Log(UserProfile.Instance.userData);


        foreach (var room in UserProfile.Instance.userData.Rooms)
        {
            var element = Instantiate(roomRowPrefab);

            element.transform.GetChild(0).GetComponent<TMP_Text>().text = room.Title;
            element.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Id));
            // TODO activate join button

            element.transform.SetParent(roomsScrollView);
        }
    }

    void JoinRoom(string roomId)
    { 
        if (roomId.Length != 0)
        {
            signalingServer.ConnectToRoom(roomId);
        }
    }
}
