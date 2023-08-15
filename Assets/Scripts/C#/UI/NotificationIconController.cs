using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationIconController : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource; 

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ReceivedNotificationEvent), new Action(() =>
        {
            StartCoroutine(SetupNotificationCounter());
            //PlayNotificationSound();
        }));

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
            new Action<bool>((bool s) =>
            {
                if (s)
                    StartCoroutine(SetupNotificationCounter());
            }));
    }

    IEnumerator SetupNotificationCounter()
    {
        while (UserProfile.Instance.userData.Notifications == null)
        {
            yield return null;
        }
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text =
            UserProfile.Instance.userData.Notifications.Count.ToString();
    }


    public void PlayNotificationSound()
    {
        audioSource.Play();
    }
}
