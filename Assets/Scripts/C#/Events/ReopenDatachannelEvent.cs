using UnityEngine;
using UnityEngine.Events;
using Unity.WebRTC;

public class ReopenDatachannelEvent : UnityEvent<string,RTCDataChannel> { }
