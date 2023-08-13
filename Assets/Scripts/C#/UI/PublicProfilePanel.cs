using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicProfilePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameField;

    [SerializeField]
    private TMP_InputField emailField;

    [SerializeField]
    private TMP_InputField phonenumberField;

    [SerializeField]
    private Button addRemoveBtn;

    [SerializeField]
    private Button backBtn;

    public async void ShowProfile(string userId,Transform prevPanel)
    {
        SwitchPanel(true, prevPanel);

        var profile = await Server.GetProfile(userId);
        if (profile == null) return;

        usernameField.text = profile.Profile.Username;
        emailField.text = profile.Profile.Email;
        phonenumberField.text = profile.Profile.Phonenumber;
        StartCoroutine(SetUpAddRemoveFriendBtn(userId));
        backBtn.onClick.AddListener(() =>
        {
            SwitchPanel(false, prevPanel);
        });
    }

    private IEnumerator SetUpAddRemoveFriendBtn(string userId)
    {
        while (UserProfile.Instance.userData.Friends == null)
        {
            yield return null; 
        }
        foreach (var user in UserProfile.Instance.userData.Friends)
        {
            if (user.Id == userId)
            {
                addRemoveBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Unfriend";
                addRemoveBtn.onClick.AddListener(async () =>
                {
                    await Server.DeleteFriend(new Mvm.DeleteFriendRequest
                    {
                        FriendId = userId
                    });
                    EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Freind has been deleted successfully");
                });
                yield break; 
            }
        }

        addRemoveBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Add friend";
        addRemoveBtn.onClick.AddListener(async () =>
        {

            await Server.CreateFriendRequest(new Mvm.CreateFriendRequestRequest
            {
                FriendId = userId
            });
            EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), "Freind request has been sent");
        });
        yield break;
    }

    private void SwitchPanel(bool switchCase, Transform prevPanel)
    {
        this.transform.gameObject.SetActive(switchCase);
        prevPanel.gameObject.SetActive(!switchCase);
    }
}
