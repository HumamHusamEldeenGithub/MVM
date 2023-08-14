using System;
using UnityEngine;

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

    private void Awake()
    {
        mainPanel.SetTrigger("FadeIn");
    }

    private void SwitchToPanel(GameObject panel_1, GameObject panel_2)
    {

    }

    private void ResetToMainMenu()
    {
        mainPanel.SetTrigger("FadeIn");
        joinRoomPanel.SetTrigger("FadeOut");
        myProfilePanel.SetTrigger("FadeOut"); ;
        myRoomPanel.SetTrigger("FadeOut"); ;
        createRoomPanel.SetTrigger("FadeOut"); ;
        searchForRoomPanel.SetTrigger("FadeOut"); ;
        searchForUsersPanel.SetTrigger("FadeOut"); ;
        publicProfilePanel.SetTrigger("FadeOut"); ;
        takePicturePanel.SetTrigger("FadeOut"); ;
        notificationPanel.SetTrigger("FadeOut"); ;
    }
}
