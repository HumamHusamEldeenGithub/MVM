using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupController : MonoBehaviour
{
    [SerializeField]
    private GameObject popup;

    [SerializeField]
    private float secondsToDestroy = 2.0f;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ShowPopupEvent),
            new Action<String>(ShowPopup));
    }

    public void ShowPopup(string popupText)
    {
        if (popup == null) return;
        popup.GetComponentInChildren<TMP_Text>().text = popupText;
        popup.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Hide(secondsToDestroy));

    }
    private IEnumerator Hide(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        popup.SetActive(false);
    }
}
