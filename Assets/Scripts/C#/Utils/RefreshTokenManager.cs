using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;

public class RefreshTokenManager : Singleton<RefreshTokenManager>
{
    const string refreshTokenKey = "refreshToken";
    SynchronizationContext syncContext; 
    override protected void Awake()
    {
        base.Awake();
        syncContext = SynchronizationContext.Current;
    }

    public void StoreRefreshToken(string refreshToken)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            PlayerPrefs.SetString(refreshTokenKey, refreshToken);
            PlayerPrefs.Save(); 
        }), null);

    }
    public string GetRefreshToken()
    {
        // TODO : use syncContext
        return PlayerPrefs.GetString(refreshTokenKey);
    }

    public void ClearRefreshToken()
    {
        // Humam al mfzlak
        syncContext.Post(new SendOrPostCallback(o =>
        {
            PlayerPrefs.DeleteKey(refreshTokenKey);
            PlayerPrefs.Save();
        }), null);

    }
}
