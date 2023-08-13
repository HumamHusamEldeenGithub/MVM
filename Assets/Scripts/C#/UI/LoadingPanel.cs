using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ShowLoadingPanelEvent), 
            new Action<bool>(ShowPanel));
    }

    public void ShowPanel(bool status)
    {
        if (panel != null)
            panel.SetActive(status);
    }
}
