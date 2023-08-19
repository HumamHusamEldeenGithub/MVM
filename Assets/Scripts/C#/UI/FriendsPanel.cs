using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mvm;
using UnityEngine.UI;

public class FriendsPanel : MonoBehaviour
{
    [SerializeField]
    private Transform onlineUsersScrollView;

    [SerializeField]
    private GameObject onlineUserPrefab;

    [SerializeField]
    private Animator homeMenuPanel;

    [SerializeField]
    private Animator publicProfilePanel;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(UsersOnlineStatusEvent),
            new Action<OnlineStatuses>(SetupOnlinePanel));
    }

    private void OnEnable()
    {
        if (UserProfile.Instance.userData != null)
        {
            SetupOnlinePanel(SignalingServerController.usersOnlineStatus);
        }
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

        statuses = AddOfflineFriends(statuses);

        foreach (var user in statuses.Users)
        {
            var element = Instantiate(onlineUserPrefab);

            element.GetComponentInChildren<TextMeshProUGUI>().text = user.Username;
            element.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                EventsPool.Instance.InvokeEvent(typeof(ShowProfileEvent), user.Id, homeMenuPanel);
                homeMenuPanel.SetTrigger("FadeOut");
                publicProfilePanel.SetTrigger("FadeIn");
            });
            if (!user.IsOnline)
            {
                element.GetComponentsInChildren<Image>()[1].color = new Color(118,118,118);
            }

            element.transform.SetParent(onlineUsersScrollView);
        }
    }

    public OnlineStatuses AddOfflineFriends(OnlineStatuses statuses)
    {
        var friends = UserProfile.Instance.userData.Friends;
        foreach(var friend in friends)
        {
            bool found = false;
            foreach (var status in statuses.Users)
            {
                if (status.Id == friend.Id)
                {
                    found = true;
                    break; 
                }
            }
            if (!found)
            {
                statuses.Users.Add(new OnlineStatus { Id = friend.Id, Username = friend.Username, IsOnline = false });
            }
        }

        return statuses;

    }
}
