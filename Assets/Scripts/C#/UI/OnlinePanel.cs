using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mvm;
using UnityEngine.UI;

public class OnlinePanel : MonoBehaviour
{
    [SerializeField]
    private Transform onlineUsersScrollView;

    [SerializeField]
    private GameObject onlineUserPrefab;

    [SerializeField]
    private PublicProfilePanel publicProfilePanel;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(UsersOnlineStatusEvent),
            new Action<OnlineStatuses>(SetupOnlinePanel));
    }

    private void OnEnable()
    {
        SetupOnlinePanel(SignalingServerController.Instance.usersOnlineStatus);
    }

    public void SetupOnlinePanel(OnlineStatuses statuses)
    {
        int childCount = onlineUsersScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = onlineUsersScrollView.GetChild(i);
            Destroy(child.gameObject);
        }

        if (statuses == null)
        {
            Debug.Log("Statuses is null");
            return;
        }

        foreach (var user in statuses.Users)
        {
            var element = Instantiate(onlineUserPrefab);

            var usernameElement = element.transform.GetChild(1);

            usernameElement.GetComponent<TMP_Text>().text = user.Username;
            usernameElement.GetComponent<Button>().onClick.AddListener(() =>
            {
                publicProfilePanel.ShowProfile(user.Id, this.transform);
            });

            element.transform.SetParent(onlineUsersScrollView);
        }
    }
}
