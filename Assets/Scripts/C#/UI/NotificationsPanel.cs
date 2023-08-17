using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mvm;

public class NotificationsPanel : MonoBehaviour
{
    [SerializeField]
    private Transform notificationsScrollView;

    [SerializeField]
    private GameObject notificationRowPrefab;

    [SerializeField]
    private Animator publicProfilePanel;

    [SerializeField]
    private Button clearBtn;

    private void Awake()
    {
        clearBtn.onClick.AddListener(async () =>
        {
            DestroyPrevoiusNotifications();
            await Server.DeleteNotifications();
            UserProfile.Instance.userData.Notifications.Clear();
        });

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>((bool s) =>
        {
            if (s)
                Initialize();
        }));
        EventsPool.Instance.AddListener(typeof(ReceivedNotificationEvent), new Action(Initialize));
        EventsPool.Instance.AddListener(typeof(ProfileUpdatedEvent), new Action(Initialize));
    }

    private void DestroyPrevoiusNotifications()
    {
        if (notificationsScrollView == null)
            return;
        int childCount = notificationsScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = notificationsScrollView.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private async void Initialize()
    {
        await UserProfile.Instance.GetMyNotifications();

        DestroyPrevoiusNotifications();

        foreach (var notification in UserProfile.Instance.userData.Notifications)
        {
            var element = Instantiate(notificationRowPrefab);

            element.GetComponentInChildren<TMP_Text>().text = notification.Message;
            element.GetComponent<Button>().onClick.AddListener(() => {
                OnClickNotification(notification);
            });
            element.GetComponentsInChildren<Button>()[1].onClick.AddListener(()=> {
                OnClickDeleteNotification(element, notification.Id);
            });

            element.transform.SetParent(notificationsScrollView);
        }
    }

    void OnClickNotification(Mvm.Notification notification)
    {
        if (notification.Type == (int)NotificationType.FriendRequest || notification.Type == (int)NotificationType.AcceptRequest)
        {
            EventsPool.Instance.InvokeEvent(typeof(ShowProfileEvent), notification.FromUser, GetComponent<Animator>());
            GetComponent<Animator>().SetTrigger("FadeOut");
            publicProfilePanel.SetTrigger("FadeIn");
        }
        if (notification.Type == (int)NotificationType.RoomInvitation)
        {
            SignalingServerController.Instance.ConnectToRoom(notification.EntityId);
        }
    }

    async void OnClickDeleteNotification(GameObject gameObj , string notificationId)
    {
        await Server.DeleteNotification(new DeleteNotificationRequest { NotificationId=notificationId });
        Destroy(gameObj);
    }
}
