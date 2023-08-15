using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsTest : MonoBehaviour
{
    public bool removeRefreshToken;


    private void OnValidate()
    {
        if (removeRefreshToken)
        {
            PlayerPrefs.SetString("refreshToken", "");
            PlayerPrefs.Save();
            removeRefreshToken = false;
        }
    }
}
