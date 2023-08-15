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
    private string addFriendIcon;
    [SerializeField]
    private string removeFriendIcon;
    [SerializeField]
    private string acceptFriendIcon;

    [SerializeField]
    private Button backBtn;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ShowProfileEvent), new Action<string, Transform>(ShowProfile));
    }

    public async void ShowProfile(string userId, Transform prevPanel)
    {
        var profile = await Server.GetProfile(userId);
        if (profile == null) return;

        usernameField.text = profile.Profile.Username;
        emailField.text = profile.Profile.Email;
        phonenumberField.text = profile.Profile.Phonenumber;
        StartCoroutine(SetUpAddRemoveFriendBtn(userId));

        backBtn.onClick.AddListener(() =>
        {
            GetComponent<Animator>().SetTrigger("FadeOut");
            prevPanel.GetComponent<Animator>().SetTrigger("FadeIn");
        });
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
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    await Server.DeleteFriend(new Mvm.DeleteFriendRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend has been deleted successfully");
                    await UserProfile.Instance.GetMyFriends();
                    StartCoroutine(SetUpAddRemoveFriendBtn(userId));
                    await SignalingServerController.SendMessageToServerAsync(new SignalingMessage
                    {
                        Type = "refreshFriends"
                    });
                });
                yield break; 
            }
        }

        foreach (var req in UserProfile.Instance.userData.PendingFriendRequests)
        {
            if (req == userId)
            {
                var btnIcon = addRemoveBtn.GetComponentInChildren<MaterialIcon>();
                btnIcon.iconUnicode = addFriendIcon;
                btnIcon.color = Color.green;
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    await Server.AddFriend(new Mvm.AddFriendRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been accepted successfully");
                    await UserProfile.Instance.GetMyFriends();
                    StartCoroutine(SetUpAddRemoveFriendBtn(userId));
                    await SignalingServerController.SendMessageToServerAsync(new SignalingMessage
                    {
                        Type = "refreshFriends"
                    });
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
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been deleted successfully");
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

            EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Friend request has been sent");
            await UserProfile.Instance.GetMyFriends();
            StartCoroutine(SetUpAddRemoveFriendBtn(userId));
        });
        yield break;
    }
}
