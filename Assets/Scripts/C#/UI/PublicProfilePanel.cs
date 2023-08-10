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
    private Button backBtn;

    public async void ShowProfile(string userId,Transform prevPanel)
    {
        SwitchPanel(true, prevPanel);

        var profile = await Server.GetProfile(userId);
        if (profile == null) return;

        usernameField.text = profile.Profile.Username;
        emailField.text = profile.Profile.Email;
        phonenumberField.text = profile.Profile.Phonenumber;
        backBtn.onClick.AddListener(() =>
        {
            SwitchPanel(false, prevPanel);
        });
    }

    private void SwitchPanel(bool switchCase, Transform prevPanel)
    {
        this.transform.gameObject.SetActive(switchCase);
        prevPanel.gameObject.SetActive(!switchCase);
    }
}
