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
    }

    public void CreateNewRoomSpace(string peerID, RTCDataChannel dataChannel)
    {
        RoomSpaceController newPeer = InstantiateRoomSpace(peerID);
        newPeer.transform.position = pos;
        pos += new Vector3(50, 0, 0);
        newPeer.Initialize(peerID, dataChannel);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
    }

    public void CreateNewRoomSpace(ref BlendShapesReadyEvent evnt, string peerID = "self")
    {
        RoomSpaceController newPeer = InstantiateRoomSpace(peerID);
        newPeer.Initialize(ref evnt);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
    }

    private RoomSpaceController InstantiateRoomSpace(string id = "self")
    {
        var roomSpace = Instantiate(roomPrefab);
        participantsRoomSpaces.Add(id, roomSpace);
        return roomSpace;
    }

}
