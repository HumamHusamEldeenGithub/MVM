using UnityEngine;
using UnityEngine.UI;
using Mvm;

public class AvatarSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private SwitchButton GenderSwitch;
    [SerializeField]
    private Slider HeadStyleSlider;
    [SerializeField]
    private Button SkinColorPicker;
    [SerializeField]
    private Slider HairStyleSlider;
    [SerializeField]
    private Button HairColorPicker;
    [SerializeField]
    private Slider EyesStyleSlider;
    [SerializeField]
    private Button EyesColorPicker;
    [SerializeField]
    private Slider EyesBrowsStyleSlider;
    [SerializeField]
    private Button EyesBrowsColorPicker;
    [SerializeField]
    private Slider NoseStyleSlider;
    [SerializeField]
    private Slider MouthStyleSlider;
    [SerializeField]
    private Slider SkinImperfectionSlider;
    [SerializeField]
    private Slider TattooSlider;

    [SerializeField]
    private ColorPicker colorPicker;

    [SerializeField]
    private GameObject SaveChangesButton;

    [SerializeField]
    private HeadCustomizer headCustomizer;

    private AvatarSettings avatarSettings = new AvatarSettings()
    {
        HeadStyle = "0",
        BrowsColor = "#FFFFFFFF",
        EyeStyle = "0",
        EyebrowsStyle = "0",
        HairStyle = "0", 
        MouthStyle = "0", 
        NoseStyle = "0",
        SkinColor = "#FFFFFFFF",
        EyeColor = "#FFFFFFFF",
        HairColor = "#FFFFFFFF",
        Gender = "male",
        RoomBackgroundColor = "#FFFFFFFF",
        SkinImperfection = "0",
        Tattoo = "0",
    };


    private void Awake()
    {
        SaveChangesButton.GetComponent<Button>().onClick.AddListener(SaveChanges);
    }

    private void Start()
    {
        InitializeActions();
        headCustomizer.SetSettings(avatarSettings);
    }

    private void OnEnable()
    {
        if (UserProfile.Instance.userData != null)
            avatarSettings = UserProfile.Instance.userData.AvatarSettings;

        GenderSwitch.value = avatarSettings.Gender.ToLower() == "male" ? 0 : 1;
        HeadStyleSlider.value = float.Parse(avatarSettings.HeadStyle);
        HairStyleSlider.value = float.Parse(avatarSettings.HairStyle);
        EyesStyleSlider.value = float.Parse(avatarSettings.EyeStyle);
        EyesBrowsStyleSlider.value = float.Parse(avatarSettings.EyebrowsStyle);
        NoseStyleSlider.value = float.Parse(avatarSettings.NoseStyle);
        SkinImperfectionSlider.value = float.Parse(avatarSettings.SkinImperfection);
        TattooSlider.value = float.Parse(avatarSettings.Tattoo);

        Color color;
        if (ColorUtility.TryParseHtmlString(avatarSettings.SkinColor, out color))
            SkinColorPicker.GetComponent<Image>().color = color;
        if (ColorUtility.TryParseHtmlString(avatarSettings.HairColor, out color))
            HairColorPicker.GetComponent<Image>().color = color;
        if (ColorUtility.TryParseHtmlString(avatarSettings.EyeColor, out color))
            EyesColorPicker.GetComponent<Image>().color = color;
        if (ColorUtility.TryParseHtmlString(avatarSettings.BrowsColor, out color))
            EyesBrowsColorPicker.GetComponent<Image>().color = color;
    }

    private async void SaveChanges()
    {
       await Server.UpsertAvatarSettings(new UpsertAvatarSettingsRequest {
            Settings = avatarSettings
       });
    }


    private void InitializeActions()
    {
        GenderSwitch.onValueChange.AddListener((int option) => { avatarSettings.Gender = option == 1 ? "female" : "male"; headCustomizer.SetSettings(avatarSettings); });

        HeadStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.HeadStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        HairStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.HairStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        EyesStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.EyeStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        EyesBrowsStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.EyebrowsStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        NoseStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.NoseStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        MouthStyleSlider.onValueChanged.AddListener((float option) => { avatarSettings.MouthStyle = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        SkinImperfectionSlider.onValueChanged.AddListener((float option) => { avatarSettings.SkinImperfection = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });
        TattooSlider.onValueChanged.AddListener((float option) => { avatarSettings.Tattoo = ((int)option).ToString(); headCustomizer.SetSettings(avatarSettings); });

        SkinColorPicker.onClick.AddListener(() =>
        {
            Image img = SkinColorPicker.GetComponent<Image>();
            colorPicker.gameObject.SetActive(true);
            colorPicker.onColorChanged.RemoveAllListeners();

            colorPicker.color = img.color;
            colorPicker.onColorChanged.AddListener((Color color) =>
            {
                img.color = color;
                avatarSettings.SkinColor = ColorUtility.ToHtmlStringRGB(color);
                headCustomizer.SetSettings(avatarSettings);
            });
        });
        HairColorPicker.onClick.AddListener(() =>
        {
            Image img = HairColorPicker.GetComponent<Image>();
            colorPicker.gameObject.SetActive(true);
            colorPicker.onColorChanged.RemoveAllListeners();

            colorPicker.color = img.color;
            colorPicker.onColorChanged.AddListener((Color color) =>
            {
                img.color = color;
                avatarSettings.HairColor = ColorUtility.ToHtmlStringRGB(color);
                headCustomizer.SetSettings(avatarSettings);
            });
        });
        EyesBrowsColorPicker.onClick.AddListener(() =>
        {
            Image img = EyesBrowsColorPicker.GetComponent<Image>();
            colorPicker.gameObject.SetActive(true);
            colorPicker.onColorChanged.RemoveAllListeners();

            colorPicker.color = img.color;
            colorPicker.onColorChanged.AddListener((Color color) =>
            {
                img.color = color;
                avatarSettings.BrowsColor = ColorUtility.ToHtmlStringRGB(color);
                headCustomizer.SetSettings(avatarSettings);
            });
        });
        EyesColorPicker.onClick.AddListener(() =>
        {
            Image img = EyesColorPicker.GetComponent<Image>();
            colorPicker.gameObject.SetActive(true);
            colorPicker.onColorChanged.RemoveAllListeners();

            colorPicker.color = img.color;
            colorPicker.onColorChanged.AddListener((Color color) =>
            {
                img.color = color;
                avatarSettings.EyeColor = ColorUtility.ToHtmlStringRGB(color);
                headCustomizer.SetSettings(avatarSettings);
            });
        });
    }
}
