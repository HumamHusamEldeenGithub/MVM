using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mvm;

public class AvatarSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private Slider GenderSlider;
    [SerializeField]
    private Slider HeadStyleSlider;
    [SerializeField]
    private GameObject SkinColorPicker;
    [SerializeField]
    private Slider HairStyleSlider;
    [SerializeField]
    private GameObject HairColorPicker;
    [SerializeField]
    private Slider EyesStyleSlider;
    [SerializeField]
    private GameObject EyesColorPicker;
    [SerializeField]
    private Slider EyesBrowsStyleSlider;
    [SerializeField]
    private GameObject EyesBrowsColorPicker;
    [SerializeField]
    private Slider NoseStyleSlider;
    [SerializeField]
    private Slider MouthStyleSlider;
    [SerializeField]
    private Slider SkinImperfectionSlider;
    [SerializeField]
    private Slider TattooSlider;

    [SerializeField]
    private GameObject SaveChangesButton;


    private void Awake()
    {
        SaveChangesButton.GetComponent<Button>().onClick.AddListener(SaveChanges);
    }

    private void OnEnable()
    {
        var avatarSettings = UserProfile.Instance.userData.AvatarSettings;

        GenderSlider.value = avatarSettings.Gender == "male" ? 0 : 1;
        HeadStyleSlider.value = float.Parse(avatarSettings.HeadStyle);
        HairStyleSlider.value = float.Parse(avatarSettings.HairStyle);
        EyesStyleSlider.value = float.Parse(avatarSettings.EyeStyle);
        EyesBrowsStyleSlider.value = float.Parse(avatarSettings.EyebrowsStyle);
        NoseStyleSlider.value = float.Parse(avatarSettings.NoseStyle);
        SkinImperfectionSlider.value = float.Parse(avatarSettings.SkinImperfection);
        TattooSlider.value = float.Parse(avatarSettings.Tattoo);

        // TODO : modify this code to set the color to color picker
        /*
        SkinColorPicker.GetComponent<Slider>().value = float.Parse(avatarSettings.SkinColor);
        HairColorPicker.GetComponent<Slider>().value = float.Parse(avatarSettings.HairColor);
        EyesColorPicker.GetComponent<Slider>().value = float.Parse(avatarSettings.EyeColor);
        EyesBrowsColorPicker.GetComponent<Slider>().value = float.Parse(avatarSettings.BrowsColor);
        */
    }

    private async void SaveChanges()
    {
        Mvm.AvatarSettings avatarSettings = new Mvm.AvatarSettings { };

        avatarSettings.Gender = GenderSlider.GetComponent<Slider>().value == 0 ? "male" : "female";
        avatarSettings.HeadStyle = HeadStyleSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.HairStyle = HairStyleSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.EyeStyle = EyesStyleSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.EyebrowsStyle = EyesBrowsStyleSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.NoseStyle = NoseStyleSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.SkinImperfection = SkinImperfectionSlider.GetComponent<Slider>().value.ToString();
        avatarSettings.Tattoo = TattooSlider.GetComponent<Slider>().value.ToString();

        // TODO : modify this code to get the color from color picker
        /*
        avatarSettings.Tattoo = SkinColorPicker.GetComponent<>().value ;
        avatarSettings.HairColor =  HairColorPicker.GetComponent<>().value ;
        avatarSettings.EyesColor = EyesColorPicker.GetComponent<>().value;
        avatarSettings.BrowsColor =  EyesBrowsColorPicker.GetComponent<>().value ;
        */
       await Server.UpsertAvatarSettings(new UpsertAvatarSettingsRequest {
            Settings= avatarSettings
       });
       UserProfile.Instance.GetMyProfile(true);

    }
}
