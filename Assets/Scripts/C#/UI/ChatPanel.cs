using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
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

    [SerializeField]
    private RectTransform scrollContent;

    [SerializeField]
    private UIChatMessage sentMessage;

    [SerializeField]
    private UIChatMessage receivedMessage;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private Scrollbar scrollbar;


    private string chatId;

    private bool isFocused = false;

    private bool isUpdating = false;

    private void Awake()
    {
        chatMessageField.onValueChanged.AddListener(FixInputFieldArabic);
        EventsPool.Instance.AddListener(typeof(ShowChatEvent), new Action<string,string, Animator>(ShowChat));
        EventsPool.Instance.AddListener(typeof(ChatMessageReceviedEvent), new Action<Mvm.SocketChatMessage>(UpdateChat));
    }

    private void Update()
    {
        chatMessageField.onDeselect.AddListener((string s) => { isFocused = false; });
        chatMessageField.onSelect.AddListener((string s) => { isFocused = true; });
    }


    private void FixInputFieldArabic(string s)
    {
        if (chatMessageField != null && !isUpdating)
        {
            isUpdating = true;
            chatMessageField.text = s;
            isUpdating = false;
        }
    }

    public async void ShowChat(string userId,string username, Animator prevPanel)
    {
        GameObject temp = new GameObject(chatId);
        int childCount = scrollContent.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = scrollContent.GetChild(i);
            child.parent = temp.transform;
        }
        sendMessageBtn.onClick.RemoveAllListeners();
        chatMessageField.onEndEdit.RemoveAllListeners();

        Destroy(temp);

        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);

        var res = await Server.GetChat(new Mvm.GetChatRequest { UserId = userId });
        if (res != null)
        {

            chatId = res.Chat.Id;
            string receiverId = res.Chat.Participants[0] == UserProfile.Instance.userData.Id ?
                res.Chat.Participants[1] : res.Chat.Participants[0];

            usernameField.text = $"Chat with {username}";

            foreach (var msg in res.Chat.Messages)
            {
                if (receiverId == msg.UserId)
                {
                    CreateReceivedMessage(msg.Message);
                }
                else
                {
                    CreateSentMessage(msg.Message);
                }
                Debug.Log($"{msg.UserId} -- {msg.Message}");
            }
            UpdateLayout();

            chatMessageField.onEndEdit.AddListener((text) =>
            {
                if (isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    if (chatMessageField.text.Length <= 0)
                        return;
                    SignalingServerController.Instance.SendChatMessage(chatId, receiverId, chatMessageField.text);
                    CreateSentMessage(chatMessageField.text, true);
                    chatMessageField.text = "";
                    chatMessageField.Select();
                    chatMessageField.ActivateInputField();
                }
            });

            sendMessageBtn.onClick.AddListener(() =>
            {
                if (chatMessageField.text.Length <= 0)
                    return;

                SignalingServerController.Instance.SendChatMessage(chatId, receiverId, chatMessageField.text);
                CreateSentMessage(chatMessageField.text);
                chatMessageField.text = "";

            });
        }

        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(() =>
        {
            chatId = "";
            GetComponent<Animator>().SetTrigger("FadeOut");
            prevPanel.SetTrigger("FadeIn");
        });

        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    private void UpdateChat(Mvm.SocketChatMessage chatMessage)
    {
        if (chatMessage.ChatId != chatId) return;
        CreateReceivedMessage(chatMessage.Message, true);
    }

    private async void UpdateLayout()
    {
        await Task.Delay((int)(Time.deltaTime * 1000));
        Canvas.ForceUpdateCanvases();
        scrollContent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        scrollContent.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void CreateSentMessage(string text, bool update = false)
    {
        UIChatMessage chatMsg = Instantiate(sentMessage);
        chatMsg.transform.parent = scrollContent.transform;
        chatMsg.SetText(text);
        if(update)
            UpdateLayout();
    }

    private void CreateReceivedMessage(string text,bool update = false)
    {
        UIChatMessage chatMsg = Instantiate(receivedMessage);
        chatMsg.transform.parent = scrollContent.transform;
        chatMsg.SetText(text);
        if (update)
            UpdateLayout();
    }
}
