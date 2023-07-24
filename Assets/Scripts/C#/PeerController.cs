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

    public void Initialize(string peerID, RTCDataChannel dataChannel, UserProfile user)
    {
        this.peerID = peerID;
        this.dataChannel = dataChannel;

        if (user != null && user.userData.UserGender == Gender.Male)
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
        currentAnimator.InitializeFace(user);


        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
                /*
                DateTime now = DateTime.Now;
                DateTime unixEpoch = new DateTime(2023, 7, 15, 20, 0, 0, DateTimeKind.Utc);

                float seconds = (float)(now - unixEpoch).TotalSeconds;
                */
                BlendShapes responseMessage = BlendShapes.Parser.ParseFrom(bytes, 0, bytes.Length);

                SetBlendShapes(responseMessage);
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

    public void SetBlendShapes(BlendShapes blendshapes)
    {
        currentAnimator.SetBlendShapes(blendshapes);
    }

    public void SetTrack(AudioStreamTrack track)
    {
        audioSource.SetTrack(track);
        audioSource.loop = true;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
