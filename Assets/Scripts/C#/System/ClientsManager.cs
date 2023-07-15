using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

public class ClientsManager : Singleton<ClientsManager>
{
    [SerializeField]
    RoomSpaceController roomPrefab;

    private Vector3 pos = Vector3.zero;

    Dictionary<string, RoomSpaceController> participantsRoomSpaces = new Dictionary<string, RoomSpaceController>();

    override protected void Awake()
    {
        base.Awake();
        EventsPool.Instance.AddListener(typeof(RemoveScreenEvent), new Action<RoomSpaceController.RoomRenderTexture>(RemovePeer));
    }

    public void CreateNewRoomSpace(string peerID, RTCDataChannel dataChannel)
    {
        RoomSpaceController newPeer = InstantiateRoomSpace(peerID);
        pos += new Vector3(50, 0, 0);
        newPeer.transform.position = pos;
        newPeer.Initialize(peerID, dataChannel);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
    }

    public void CreateNewRoomSpace(ref BlendShapesReadyEvent evnt, string peerID = "self")
    {
        RoomSpaceController newPeer = InstantiateRoomSpace(peerID);
        pos += new Vector3(50, 0, 0);
        newPeer.transform.position = pos;
        newPeer.Initialize(ref evnt);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
    }

    private RoomSpaceController InstantiateRoomSpace(string id = "self")
    {
        var roomSpace = Instantiate(roomPrefab);
        participantsRoomSpaces.Add(id, roomSpace);
        return roomSpace;
    }
    private void RemovePeer(RoomSpaceController.RoomRenderTexture rt)
    {
        participantsRoomSpaces.Remove(rt.renderTexture.name);
    }

}
