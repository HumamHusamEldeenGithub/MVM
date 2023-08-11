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
        EventsPool.Instance.AddListener(typeof(WebRTCConnectionClosedEvent), new Action<string>(ClosePeer));
        EventsPool.Instance.AddListener(typeof(RemoveScreenEvent), new Action<RoomSpaceController.RoomRenderTexture>(RemovePeer));
    }

    public RoomSpaceController CreateNewRoomSpace(string peerID = "self", RTCDataChannel dataChannel = null, UserProfile.PeerData user = null)
    {
        RoomSpaceController newPeer = InstantiateRoomSpace(peerID);
        pos += new Vector3(50, 0, 0);
        newPeer.transform.position = pos;

        newPeer.Initialize(peerID, dataChannel, user);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
        return newPeer;
    }

    private RoomSpaceController InstantiateRoomSpace(string id)
    {
        var roomSpace = Instantiate(roomPrefab);
        participantsRoomSpaces.Add(id, roomSpace);
        return roomSpace;
    }

    private void RemovePeer(RoomSpaceController.RoomRenderTexture rt)
    {
        participantsRoomSpaces.Remove(rt.renderTexture.name);
    }

    private void ClosePeer(string peerID)
    {
        RoomSpaceController ctrl;
        participantsRoomSpaces.TryGetValue(peerID, out ctrl);
        ctrl.Dispose();
    }

}
