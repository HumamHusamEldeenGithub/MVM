using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CameraPanel : Singleton<MonoBehaviour>
{
    [SerializeField]
    private RawImage cameraImage;

    [SerializeField]
    private Button CaptureBtn;

    static WebCamTexture cameraTexture;

    protected override void Awake()
    {
        CaptureBtn.onClick.AddListener(CaptureAndSavePhoto);

        EventsPool.Instance.AddListener(typeof(ShowTakePictuePanelEvent), new Action(Enable));

        EventsPool.Instance.AddListener(typeof(LoginStatusEvent), new Action<bool>(Disable));
    }

    private void Enable()
    {

        IEnumerator enable()
        {
            if (cameraTexture == null)
            {
                cameraTexture = new WebCamTexture();
            }

            cameraImage.texture = cameraTexture;

            if (!cameraTexture.isPlaying)
                cameraTexture.Play();

            Debug.Log(cameraTexture.width);
            Debug.Log(cameraTexture.height);

            var aspectFitter = cameraImage.GetComponent<AspectRatioFitter>();
            float webcamAspect = (float)cameraTexture.width / (float)cameraTexture.height;
            aspectFitter.aspectRatio = webcamAspect;
            yield return null;
        }

        StartCoroutine(enable());

    }

    private void Disable(bool f)
    {
        if(cameraTexture != null)
            cameraTexture?.Stop();
    }

    public void ActiveEventListener()
    {
        EventsPool.Instance.AddListener(typeof(ShowTakePictuePanelEvent), new Action(EnablePanel));
    }

    private void EnablePanel()
    {
        gameObject.SetActive(true);
    }

    public async void CaptureAndSavePhoto()
    {
        Texture2D photoTexture = new Texture2D(cameraTexture.width, cameraTexture.height);
        photoTexture.SetPixels(cameraTexture.GetPixels());
        photoTexture.Apply();

        byte[] bytes = photoTexture.EncodeToPNG();

        var res = await Server.SendImageToAIPipeline(bytes);

        AIPipelineResponse aiPipelineResponse = DeserializeFlaskResponse(res);

        await Server.UpsertAvatarSettings(new Mvm.UpsertAvatarSettingsRequest
        {
            Settings = new Mvm.AvatarSettings
            {
                HeadStyle = "0",
                BrowsColor = StandardColors.GetStandardColors(aiPipelineResponse.HairColor),
                EyeStyle = "0",
                EyebrowsStyle = "1",
                HairStyle = "1",
                MouthStyle = "0",
                NoseStyle = "0",
                // Anoos you had one job
                SkinColor = StandardColors.RGBToHex(aiPipelineResponse.SkinColorRGB[2] , aiPipelineResponse.SkinColorRGB[1], aiPipelineResponse.SkinColorRGB[0]),
                EyeColor = "#FFFFFFFF",
                HairColor = StandardColors.GetStandardColors(aiPipelineResponse.HairColor),
                Gender = aiPipelineResponse.Gender,
                RoomBackgroundColor = "#FFFFFFFF",
                SkinImperfection = "0",
                Tattoo = "0",
            }
        });

        Destroy(photoTexture);
        Debug.Log("Photo Sent");

        EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
    }

    public void Skip()
    {
        EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
    }

    AIPipelineResponse DeserializeFlaskResponse (string jsonString)
    {
        AIPipelineResponse res = JsonConvert.DeserializeObject<AIPipelineResponse>(jsonString);

        string pattern = @"\b\d+\b";
        MatchCollection matches = Regex.Matches(res.SkinColorStr, pattern);
        res.SkinColorRGB = new int[] { int.Parse(matches[0].Value), int.Parse(matches[1].Value), int.Parse(matches[2].Value) };
        return res;
    }

    class AIPipelineResponse
    {
        public string Beard { get; set; }
        public string Gender { get; set; }
        public bool Glasses { get; set; }
        [JsonProperty("hair_color")]
        public string HairColor { get; set; }
        [JsonProperty("skin_color_rgb")]
        public string SkinColorStr { get; set; }
        public int[] SkinColorRGB { get; set; }
    }
}
