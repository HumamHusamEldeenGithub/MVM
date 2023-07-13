using Mvm;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        get; private set;
    }

    public void Initialize(string peerID, RTCDataChannel dataChannel)
    {
        SelfInitialize();
        peerController.Initialize(peerID, dataChannel);
    }
    public void Initialize(ref BlendShapesReadyEvent evnt)
    {
        SelfInitialize();
        peerController.Initialize(ref evnt);
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
}
