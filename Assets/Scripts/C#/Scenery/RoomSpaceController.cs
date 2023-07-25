using Unity.VisualScripting;
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
    [SerializeField] RoomSpace roomSpace;
    [SerializeField] MeshRenderer backgroundMesh;
    [SerializeField] Camera roomCamera;



    public RoomRenderTexture CurrentRoomRenderTexture
    {
        get; set;
    }

    public PeerController PeerController { get { return peerController; } }

    public void Initialize(string peerID, RTCDataChannel dataChannel, UserProfile.PeerData user)
    {
        SelfInitialize();
        peerController.Initialize(peerID, dataChannel, user);
        CurrentRoomRenderTexture.renderTexture.name = peerController.peerID;

        if (dataChannel != null)
        {
            dataChannel.OnClose += Dispose;
        }
    }

    private void SelfInitialize()
    {
        if (roomSpace != null && backgroundMesh != null)
        {
            backgroundMesh.material.color = roomSpace.BackgroundColor;
        }
        var rt = new RenderTexture(1920, 1080, 16);
        CurrentRoomRenderTexture = new RoomRenderTexture (ref rt);

        roomCamera.targetTexture = rt;
    }

    private void OnDestroy()
    {
        CurrentRoomRenderTexture.renderTexture.Release();
    }

    private void Dispose()
    {
        CurrentRoomRenderTexture.renderTexture.Release();
        EventsPool.Instance.InvokeEvent(typeof(RemoveScreenEvent), CurrentRoomRenderTexture);
        Destroy(gameObject);
    }
}
