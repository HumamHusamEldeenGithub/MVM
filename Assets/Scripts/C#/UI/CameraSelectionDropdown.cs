using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class CameraSelectionDropdown : MonoBehaviour
{
    private TMP_Dropdown dropDown;

    private void Awake()
    {
        dropDown= GetComponent<TMP_Dropdown>();
        WebCamDevice[] devices = WebCamTexture.devices;
        dropDown.options = new List<TMP_Dropdown.OptionData>();

        foreach (WebCamDevice device in devices)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(device.name);
            dropDown.options.Add(option);
        }

        dropDown.onValueChanged.AddListener((val) =>
        {
            GeneralManager.cameraIndex = val;
        });
    }
}
