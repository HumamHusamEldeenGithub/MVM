using Mvm;
using Unity.WebRTC;
using UnityEngine.Events;

public class CreatePeerEvent : UnityEvent<string, RTCDataChannel>
{
}
