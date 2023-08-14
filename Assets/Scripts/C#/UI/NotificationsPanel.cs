using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationsPanel : MonoBehaviour
{
    [SerializeField]
    private Transform notificationsScrollView;

    [SerializeField]
    private GameObject notificationRowPrefab;

    [SerializeField]
    private PublicProfilePanel publicProfilePanel;

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
    }

    private void DestroyPrevoiusNotifications()
    {
        int childCount = notificationsScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = notificationsScrollView.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private async void OnEnable()
    {
        await UserProfile.Instance.GetMyNotifications();

        DestroyPrevoiusNotifications();

        foreach (var notification in UserProfile.Instance.userData.Notifications)
        {
            var element = Instantiate(notificationRowPrefab);

            element.transform.GetChild(0).GetComponent<TMP_Text>().text = notification.Message;
            element.GetComponent<Button>().onClick.AddListener(() => {
                OnClickNotification(notification);
            });

            element.transform.SetParent(notificationsScrollView);
        }
    }

    void OnClickNotification(Mvm.Notification notification)
    {
        if (notification.Type == 1 || notification.Type == 3)
        {
            publicProfilePanel.ShowProfile(notification.FromUser, this.transform);
        }
        if (notification.Type == 2)
        {
            // TODO : add join room action
        }
    }
}
