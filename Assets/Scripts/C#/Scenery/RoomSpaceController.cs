using System;
using TMPro;
using Unity.WebRTC;
using UnityEngine;

public class RoomSpaceController : MonoBehaviour
{
    public class RoomRenderTexture
    {
        public RenderTexture renderTexture { get; }

        public RoomRenderTexture(ref RenderTexture rt)
        {
            renderTexture = rt;
        }
    }

    [SerializeField] PeerController peerController;
    [SerializeField] GameObject[] roomSpaces;
    [SerializeField] Camera roomCamera;
    [SerializeField] TextMeshProUGUI usernameText;

    private GameObject decorationBackground;
    private int backgroundIndex = 0;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(UserChangeBackgroundEvent), new Action(ChangeBackground));
    }


    public RoomRenderTexture CurrentRoomRenderTexture
    {
        get; set;
    }

    public PeerController PeerController { get { return peerController; } }

    public void Initialize(string peerID, RTCDataChannel dataChannel, UserProfile.PeerData user)
    {
        SelfInitialize(user.AvatarSettings.RoomBackgroundColor);
        peerController.Initialize(peerID, dataChannel, user);
        usernameText.text = user.Username;
        CurrentRoomRenderTexture.renderTexture.name = peerController.peerID;
    }

    private void SelfInitialize(string backgroundID)
    {
        if (int.TryParse(backgroundID, out backgroundIndex)) {
            backgroundIndex = Mathf.Clamp(backgroundIndex, 0, roomSpaces.Length - 1);
        }

        decorationBackground = Instantiate(roomSpaces[backgroundIndex]);
        decorationBackground.transform.SetParent(transform, false);
        decorationBackground.transform.localPosition = Vector3.zero;

        var rt = new RenderTexture(1920, 1080, 16);
        CurrentRoomRenderTexture = new RoomRenderTexture (ref rt);
        roomCamera.targetTexture = rt;
    }

    private void ChangeBackground()
    {
        backgroundIndex++;
        if(backgroundIndex >= roomSpaces.Length)
        {
            backgroundIndex = 0;
        }
        Destroy(decorationBackground);
        decorationBackground = Instantiate(roomSpaces[backgroundIndex]);
        decorationBackground.transform.SetParent(transform, false);
        decorationBackground.transform.localPosition = Vector3.zero;
    }

    private void OnDestroy()
    {
        CurrentRoomRenderTexture.renderTexture.Release();
    }

    public void Dispose()
    {
        CurrentRoomRenderTexture.renderTexture.Release();
        EventsPool.Instance.InvokeEvent(typeof(RemoveScreenEvent), CurrentRoomRenderTexture);
        Destroy(gameObject);
    }
}
