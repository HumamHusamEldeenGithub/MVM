using Google.MaterialDesign.Icons;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PublicProfilePanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI usernameField;

    [SerializeField]
    private TextMeshProUGUI emailField;

    [SerializeField]
    private TextMeshProUGUI phonenumberField;

    [SerializeField]
    private Button addRemoveBtn;

    [SerializeField]
    private Button chatBtn;

    [SerializeField]
    private Animator chatPanelAnimator;

    [SerializeField]
    private string addFriendIcon;
    [SerializeField]
    private string removeFriendIcon;
    [SerializeField]
    private string acceptFriendIcon;

    [SerializeField]
    private Button backBtn;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ShowProfileEvent), new Action<string, Animator>(ShowProfile));
    }

    public async void ShowProfile(string userId, Animator prevPanel)
    {
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
        var profile = await Server.GetProfile(userId);
        if (profile == null) return;

        usernameField.text = profile.Profile.Username;
        emailField.text = profile.Profile.Email;
        phonenumberField.text = profile.Profile.Phonenumber;
        StartCoroutine(SetUpAddRemoveFriendBtn(userId));

        chatBtn.gameObject.SetActive(false);
        foreach (var friend in UserProfile.Instance.userData.Friends)
        {
            if (friend.Id == profile.Profile.Id)
            {
                chatBtn.gameObject.SetActive(true);
                chatBtn.onClick.AddListener(() =>
                {
                    EventsPool.Instance.InvokeEvent(typeof(ShowChatEvent), profile.Profile.Id, profile.Profile.Username, GetComponent<Animator>());
                    GetComponent<Animator>().SetTrigger("FadeOut");
                    chatPanelAnimator.SetTrigger("FadeIn");
                });
            }
        }

        backBtn.onClick.AddListener(() =>
        {
            GetComponent<Animator>().SetTrigger("FadeOut");
            prevPanel.SetTrigger("FadeIn");
        });

        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    private IEnumerator SetUpAddRemoveFriendBtn(string userId)
    {
        addRemoveBtn.onClick.RemoveAllListeners();
        while (UserProfile.Instance.userData.Friends == null)
        {
            yield return null; 
        }
        foreach (var user in UserProfile.Instance.userData.Friends)
        {
            if (user.Id == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
                btnIcon.iconUnicode = removeFriendIcon;
                btnIcon.color = Color.red;
                addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Remove friend";
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
                    await Server.DeleteFriend(new Mvm.DeleteFriendRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend has been deleted successfully", 2, Color.black);
                    await UserProfile.Instance.GetMyFriends();
                    StartCoroutine(SetUpAddRemoveFriendBtn(userId));
                    await SignalingServerController.Instance.SendRefreshFriendsEvent();
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
                });
                yield break; 
            }
        }

        foreach (var req in UserProfile.Instance.userData.PendingFriendRequests)
        {
            if (req == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();

                addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Remove request";
                btnIcon.iconUnicode = addFriendIcon;
                btnIcon.color = Color.green;
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    await Server.AddFriend(new Mvm.AddFriendRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been accepted successfully", 2, Color.black);
                    await UserProfile.Instance.GetMyFriends();
                    StartCoroutine(SetUpAddRemoveFriendBtn(userId));
                    await SignalingServerController.Instance.SendRefreshFriendsEvent();
                });
                yield break;
            }
        }

        foreach (var req in UserProfile.Instance.userData.SentFriendRequests)
        {
            if (req == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
                btnIcon.iconUnicode = removeFriendIcon;
                btnIcon.color = Color.red;
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    await Server.DeleteFriendRequest(new Mvm.DeleteFriendRequestRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been deleted successfully", 2, Color.black);
                    await UserProfile.Instance.GetMyFriends();
                    StartCoroutine(SetUpAddRemoveFriendBtn(userId));
                });
                yield break;
            }
        }

        var icon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
        icon.iconUnicode = addFriendIcon;
        icon.color = Color.white;
        addRemoveBtn.onClick.AddListener(async () =>
        {

            await Server.CreateFriendRequest(new Mvm.CreateFriendRequestRequest
            {
                FriendId = userId
            });

            EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been sent", 2, Color.black);
            await UserProfile.Instance.GetMyFriends();
            StartCoroutine(SetUpAddRemoveFriendBtn(userId));
        });
        yield break;
    }
}
