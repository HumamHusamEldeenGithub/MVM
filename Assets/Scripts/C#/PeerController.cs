using Mvm;
using System;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public class PeerController : MonoBehaviour
{
    [SerializeField]
    private FaceAnimator maleAnimator;

    [SerializeField]
    private FaceAnimator femaleAnimator;

    #region Private

    private FaceAnimator currentAnimator;
    public string peerID;
    private RTCDataChannel dataChannel;

    private OrientationProcessor orProcessor;
    private AudioSource audioSource;

    private const float delayThreshold = 5.0f;
    private const int delayMaxcounter = 30;
    private int delayCounter = 0;
    private Queue<DataChannelMessage> dataChannelMessages = new Queue<DataChannelMessage>();

    #endregion

    private void Awake()
    {
        orProcessor = GetComponent<OrientationProcessor>();
        audioSource = GetComponent<AudioSource>();
        EventsPool.Instance.AddListener(typeof(ReopenDatachannelEvent),
            new Action<string,RTCDataChannel>(ReopenDatachannel));
    }

    private void Update()
    {
        if(dataChannelMessages.Count > 0)
        {
            var dm = dataChannelMessages.Dequeue();
            SetTrackingData(dm);
        }
    }

    public void Initialize(string peerID, RTCDataChannel dataChannel, UserProfile.PeerData userData)
    {
        this.peerID = peerID;
        this.dataChannel = dataChannel;

        if (userData == null) userData = UserProfile.GetDefaultAvatarSettings();

        if (userData.AvatarSettings.Gender.ToLower() == "male")
        {
            femaleAnimator.gameObject.SetActive(false);

            currentAnimator = maleAnimator;
        }
        else
        {
            maleAnimator.gameObject.SetActive(false);

            currentAnimator = femaleAnimator;
        }

        currentAnimator.gameObject.SetActive(true);
        currentAnimator.InitializeFace(userData);

        SetupDatachannel(this.dataChannel);
    }

    public void SetTrackingData(DataChannelMessage message)
    {
        currentAnimator.SetBlendShapes(message.TrackingMessage.BlendShapes);
        orProcessor.SetPoints(new List<Keypoint>()
                {
                    message.FaceRotationMessage.Point1,
                    message.FaceRotationMessage.Point2,
                    message.FaceRotationMessage.Point3,
                    message.FaceRotationMessage.Point4
                });
    }

    public void SetTrack(AudioStreamTrack track)
    {
        audioSource.SetTrack(track);
        audioSource.loop = true;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }

    public void CheckDelayThreshold(string responseDate)
    {
        var delayTime = (float)(TimestampController.apiDate - 
            DateTime.Parse(responseDate)).TotalSeconds;
        Debug.Log($"Time difference = {delayTime}");

        if (delayTime > delayThreshold)
        {
            delayCounter++; 
            if (delayCounter >= delayMaxcounter)
            {
                // Send event to drop the current datachannel
                EventsPool.Instance.InvokeEvent(typeof(MaxDelayPacketsReachedEvent), peerID);
                delayCounter = 0;
            }
        }
        else
        {
            if (delayCounter > 0)
            {
                delayCounter--;
            }
        }
    }
    
    public void SetupDatachannel(RTCDataChannel dataChannel)
    {
        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
                Debug.Log("Bytes size : " + bytes.Length);

                DataChannelMessage responseMessage = DataChannelMessage.Parser.ParseFrom(bytes, 0, bytes.Length);

                if (responseMessage.AvatarMessage != null)
                {
                    Debug.Log($"Received avatar message {responseMessage.AvatarMessage}");
                }
                else if (responseMessage.TrackingMessage != null)
                {
                    CheckDelayThreshold(responseMessage.TrackingMessage.Date);

                    dataChannelMessages.Enqueue(responseMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        if (dataChannel != null)
        {
            dataChannel.OnMessage += OnDataChannelMessage;

            Debug.Log(dataChannel.Id);
        }
        else
        {
            Debug.Log("No channel");
        }
    }

    void ReopenDatachannel(string peerId , RTCDataChannel dataChannel)
    {
        if (peerId == this.peerID)
        {
            SetupDatachannel(dataChannel);
            dataChannelMessages.Clear();
        }
    }
}
