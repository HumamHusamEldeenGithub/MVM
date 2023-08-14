using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Animator loginPanel;

    [SerializeField]
    private Animator mainPanel;

    [SerializeField]
    private Animator roomPanel;

    [SerializeField]
    private Animator loadingPanel;

    [SerializeField]
    private GameObject popupPanel;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(OnLogin));
        EventsPool.Instance.AddListener(typeof(RoomConnectedStatusEvent), new Action<bool>(OnRoomConnected));

        EventsPool.Instance.AddListener(typeof(ShowPopupEvent), new Action<string, float>(OnShowPopup));
        EventsPool.Instance.AddListener(typeof(ToggleLoadingPanelEvent), new Action<bool>(OnToggleLoadingPanel));

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(OnLogin));
    }

    private void SwitchToPanel(Animator panel_1, Animator panel_2)
    {
        Debug.Log("Wa22el");
        panel_1.SetTrigger("FadeOut");
        panel_2.SetTrigger("FadeIn");
    }

    private void OnLogin(bool sucess)
    {
        if (sucess)
        {
            SwitchToPanel(loginPanel, mainPanel);
        }
    }

    private void OnRoomConnected(bool success)
    {
        if(success)
        {
            SwitchToPanel(mainPanel, roomPanel);
        }
    }

    private void OnToggleLoadingPanel(bool status)
    {
        loadingPanel?.SetTrigger(status == false ? "FadeOut" : "FadeIn");
    }


    public void OnShowPopup(string popupText, float seconds)
    {
        if (popupPanel == null) return;
        popupPanel.GetComponentInChildren<TMP_Text>().text = popupText;
        popupPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(HidePopup(seconds));
    }

    private IEnumerator HidePopup(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        popupPanel.SetActive(false);
    }
}
