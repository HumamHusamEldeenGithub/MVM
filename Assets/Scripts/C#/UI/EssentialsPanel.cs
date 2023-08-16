using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EssentialsPanel : MonoBehaviour
{
    [SerializeField]
    private Animator mainPanel;

    [SerializeField]
    private Animator joinRoomPanel;

    [SerializeField]
    private Animator myProfilePanel;

    [SerializeField]
    private Animator myRoomPanel;

    [SerializeField]
    private Animator createRoomPanel;

    [SerializeField]
    private Animator searchForRoomPanel;

    [SerializeField]
    private Animator searchForUsersPanel;

    [SerializeField]
    private Animator publicProfilePanel;

    [SerializeField]
    private Animator takePicturePanel;

    [SerializeField]
    private Animator notificationPanel;

    private List<Animator> animators;

    private void Awake()
    {
        mainPanel.SetTrigger("FadeIn");
        animators = new List<Animator>()
        {
            mainPanel,
            joinRoomPanel,
            myProfilePanel,
            myRoomPanel,
            createRoomPanel,
            searchForRoomPanel,
            searchForUsersPanel,
            publicProfilePanel,
            takePicturePanel,
            notificationPanel,
        };

        if (UserProfile.Instance.userData != null)
        {
            ForceToPanel(mainPanel);
        }

        EventsPool.Instance.AddListener(typeof(HangupEvent), new Action(() => ForceToPanel(mainPanel)));
    }

    public void ForceToPanel(Animator activePanel)
    {
        foreach(var panel in animators)
        {
            if(panel == activePanel)
            {
                if (panel.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"))
                {
                    panel.SetTrigger("FadeIn");
                }
            }
            else
            {
                if (panel.GetCurrentAnimatorStateInfo(0).IsName("FadeIn"))
                {
                    panel.SetTrigger("FadeOut");
                }
            }
        }
    }
}
