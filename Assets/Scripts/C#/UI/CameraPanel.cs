using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraPanel : Singleton<MonoBehaviour>
{
    [SerializeField]
    private GameObject cameraObj;

    [SerializeField]
    private Button CaptureBtn;

    [SerializeField]
    private Button backBtn;

    static WebCamTexture cameraTexture;

    protected override void Awake()
    {
        CaptureBtn.onClick.AddListener(CaptureAndSavePhoto);
    }

    void OnEnable()
    {
        if (cameraTexture == null)
        {
            cameraTexture = new WebCamTexture();
        }

        var rawImage = cameraObj.GetComponent<RawImage>();
        rawImage.texture = cameraTexture;

        if (!cameraTexture.isPlaying)
            cameraTexture.Play();

        float webcamAspect = (float)cameraTexture.width / cameraTexture.height;

        rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.sizeDelta.y * webcamAspect, rawImage.rectTransform.sizeDelta.y);
    }
    private void OnDisable()
    {
        cameraTexture.Stop();
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

        var res = await Server.UploadFile(bytes);

        //await Server.UpsertAvatarSettings(new Mvm.UpsertAvatarSettingsRequest { });

        Destroy(photoTexture);
        Debug.Log("Photo Sent");

        backBtn.onClick.Invoke();
    }
}
