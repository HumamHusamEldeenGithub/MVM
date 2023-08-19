using Google.MaterialDesign.Icons;
using Mvm;
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
        chatBtn.gameObject.SetActive(false);
        chatBtn.onClick.RemoveAllListeners();
        var profile = await Server.GetProfile(userId);
        if (profile != null)
        {
            usernameField.text = profile.Profile.Username;
            emailField.text = profile.Profile.Email;
            phonenumberField.text = profile.Profile.Phonenumber;
            SetUpAddRemoveFriendBtn(userId);

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
        }

        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(() =>
        {
            GetComponent<Animator>().SetTrigger("FadeOut");
            prevPanel.SetTrigger("FadeIn");
        });

        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    private void SetUpAddRemoveFriendBtn(string userId)
    {
        addRemoveBtn.onClick.RemoveAllListeners();
        foreach (var user in UserProfile.Instance.userData.Friends)
        {
            if (user.Id == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
                btnIcon.iconUnicode = removeFriendIcon;
                btnIcon.color = Color.red;
                addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Unfriend";
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
                    var res = await Server.DeleteFriend(new Mvm.DeleteFriendRequest
                    {
                        FriendId = userId
                    });
                    if (res != null)
                    {
                        EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend has been deleted successfully", 2, Color.black);
                        await UserProfile.Instance.GetMyFriends();
                        SetUpAddRemoveFriendBtn(userId);
                        await SignalingServerController.Instance.SendRefreshFriendsEvent();
                    }
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
                });
                return;
            }
        }

        foreach (var req in UserProfile.Instance.userData.PendingFriendRequests)
        {
            if (req == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();

                addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Accept request";
                btnIcon.iconUnicode = addFriendIcon;
                btnIcon.color = Color.green;
                addRemoveBtn.onClick.AddListener(async () =>
                {

                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
                    var res = await Server.AddFriend(new Mvm.AddFriendRequest
                    {
                        FriendId = userId
                    });
                    if (res != null)
                    {
                        EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been accepted successfully", 2, Color.black);
                        await UserProfile.Instance.GetMyFriends();
                        SetUpAddRemoveFriendBtn(userId);
                        await SignalingServerController.Instance.SendRefreshFriendsEvent();
                    }
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
                });
                return;
            }
        }

        foreach (var req in UserProfile.Instance.userData.SentFriendRequests)
        {
            if (req == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
                btnIcon.iconUnicode = removeFriendIcon;
                btnIcon.color = Color.red;
                addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel request";
                addRemoveBtn.onClick.AddListener(async () =>
                {

                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
                    var res = await Server.DeleteFriendRequest(new Mvm.DeleteFriendRequestRequest
                    {
                        FriendId = userId
                    });
                    if(res != null)
                    {
                        EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been deleted successfully", 2, Color.black);
                        await UserProfile.Instance.GetMyFriends();
                        SetUpAddRemoveFriendBtn(userId);
                    }
                    EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
                });
                return;
            }
        }

        var icon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
        icon.iconUnicode = addFriendIcon;
        icon.color = Color.white;
        addRemoveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Send request";
        addRemoveBtn.onClick.AddListener(async () =>
        {

            EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
            var res = await Server.CreateFriendRequest(new Mvm.CreateFriendRequestRequest
            {
                FriendId = userId
            });

            if (res != null)
            {
                EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been sent", 2, Color.black);
                await UserProfile.Instance.GetMyFriends();
                SetUpAddRemoveFriendBtn(userId);
            }

            EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
        });
        return;
    }
}
