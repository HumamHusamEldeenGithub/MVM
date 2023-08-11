using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TimestampController : MonoBehaviour
{
    private const string API_URL = "http://worldtimeapi.org/api/timezone/Asia/Damascus";
    public static DateTime apiDate;

    void Start()
    {
        StartCoroutine(GetTimeFromAPI());
    }

    private void Update()
    {
        if (apiDate != null)
        {
            apiDate = apiDate.AddSeconds(Time.deltaTime);
        }
    }

    IEnumerator GetTimeFromAPI()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;

                TimeApiResponse jsonResponse = JsonUtility.FromJson<TimeApiResponse>(responseText);

                apiDate = DateTime.Parse(jsonResponse.datetime);

                Debug.Log("Current DateTime: " + apiDate.ToString());
            }
        }
    }

    [System.Serializable]
    public class TimeApiResponse
    {
        public string datetime;
        public string utc_offset;
    }
}
