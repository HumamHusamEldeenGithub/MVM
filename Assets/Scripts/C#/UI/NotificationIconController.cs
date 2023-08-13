using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationIconController : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(SetupNotificationCounter());
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
}
