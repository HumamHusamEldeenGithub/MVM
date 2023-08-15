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

        EventsPool.Instance.AddListener(typeof(ShowPopupEvent), new Action<string, float, Color>(OnShowPopup));
        EventsPool.Instance.AddListener(typeof(ToggleLoadingPanelEvent), new Action<bool>(OnToggleLoadingPanel));

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(OnLogin));

        popupPanel.SetActive(false);

        if (UserProfile.Instance.userData == null)
        {
            var refreshToken = RefreshTokenManager.Instance.GetRefreshToken();
            if (refreshToken != null && refreshToken != "")
            {
                Debug.Log("RefreshToken found");
                EventsPool.Instance.InvokeEvent(typeof(SubmitLoginEvent), "","",refreshToken);
                return;
            }
            else
            {
                SwitchToPanel(null, loginPanel);
            }
        }
        else
        {
            SwitchToPanel(loginPanel, mainPanel);
        }
    }

    private void SwitchToPanel(Animator panel_1, Animator panel_2)
    {
        if(panel_1 != null)
            panel_1?.SetTrigger("FadeOut");

        if (panel_2 != null)
            panel_2?.SetTrigger("FadeIn");
    }

    private void OnLogin(bool sucess)
    {
        if (sucess)
        {
            SwitchToPanel(loginPanel, mainPanel);
        }
        else
        {
            SwitchToPanel(null, loginPanel);
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
        if (loadingPanel == null) return;
        loadingPanel.SetTrigger(status == false ? "FadeOut" : "FadeIn");
    }


    public void OnShowPopup(string popupText, float seconds, Color color)
    {
        if (popupPanel == null) return;
        var tmp = popupPanel.GetComponentInChildren<TMP_Text>();
        tmp.color = color;
        tmp.text = popupText;
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
