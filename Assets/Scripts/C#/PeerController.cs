using Mvm;
using System;
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

    #endregion

    private void Awake()
    {
        orProcessor = GetComponent<OrientationProcessor>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(string peerID, RTCDataChannel dataChannel, UserProfile.PeerData userData)
    {
        this.peerID = peerID;
        this.dataChannel = dataChannel;

        if (userData == null || userData.AvatarSettings[(int)AvatarSettings.Gender] == Gender.Male.ToString())
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


        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
                PythonServerMessage responseMessage = PythonServerMessage.Parser.ParseFrom(bytes, 0, bytes.Length);

                SetTrackingData(responseMessage);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        if(dataChannel != null)
        {
            dataChannel.OnMessage += OnDataChannelMessage;

            Debug.Log(dataChannel.Id);
        }
        else
        {
            Debug.Log("No channel");
        }
    }

    public void SetTrackingData(PythonServerMessage message)
    {
        currentAnimator.SetBlendShapes(message.BlendShapes);
    }

    public void SetTrack(AudioStreamTrack track)
    {
        audioSource.SetTrack(track);
        audioSource.loop = true;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
