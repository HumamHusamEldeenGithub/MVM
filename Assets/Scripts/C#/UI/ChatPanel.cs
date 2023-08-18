using Google.MaterialDesign.Icons;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI usernameField;

    [SerializeField]
    private Transform chatScrollContent;

    [SerializeField]
    private TMP_InputField chatMessageField;

    [SerializeField]
    private Button sendMessageBtn;

    [SerializeField]
    private Button backBtn;

    private string chatId ; 

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(ShowChatEvent), new Action<string,string, Animator>(ShowChat));
        EventsPool.Instance.AddListener(typeof(ChatMessageReceviedEvent), new Action<Mvm.SocketChatMessage>(UpdateChat));
    }

    public async void ShowChat(string userId,string username, Animator prevPanel)
    {
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);

        var res = await Server.GetChat(new Mvm.GetChatRequest { UserId = userId });
        if (res == null) return;

        chatId = res.Chat.Id;
        string receiverId = res.Chat.Participants[0] == UserProfile.Instance.userData.Id ?
            res.Chat.Participants[1] : res.Chat.Participants[0];

        usernameField.text = $"Chat with {username}";

        foreach (var msg in res.Chat.Messages)
        {
            // if receiverId == msg.UserId -> left 
            // else  -> right (My messages)
            Debug.Log($"{msg.UserId} -- {msg.Message}");
        }

        chatMessageField.onEndEdit.AddListener((text)=>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SignalingServerController.Instance.SendChatMessage(chatId, receiverId, chatMessageField.text);
                chatMessageField.text = "";
            }
        });

        sendMessageBtn.onClick.AddListener(() =>
        {
            SignalingServerController.Instance.SendChatMessage(chatId, receiverId, chatMessageField.text);
            chatMessageField.text = "";
        });

        backBtn.onClick.AddListener(() =>
        {
            GetComponent<Animator>().SetTrigger("FadeOut");
            prevPanel.SetTrigger("FadeIn");
        });

        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    private void UpdateChat(Mvm.SocketChatMessage chatMessage)
    {
        if (chatMessage.ChatId != chatId) return;
        // Append new message to scroll view 
    }
}
