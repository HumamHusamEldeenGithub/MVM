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
    private Animator publicProfilePanel;

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

            element.GetComponentInChildren<TextMeshProUGUI>().text = user.Username;
            element.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                EventsPool.Instance.InvokeEvent(typeof(ShowProfileEvent), user.Id, transform);
                transform.parent.GetComponent<Animator>().SetTrigger("FadeOut");
                publicProfilePanel.SetTrigger("FadeIn");
            });

            element.transform.SetParent(onlineUsersScrollView);
        }
    }
}
