using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TimestampController : MonoBehaviour
{
    private const string API_URL = "https://timeapi.io/api/TimeZone/zone?timeZone=Asia/Damascus";
    public static DateTime apiDate;

    void Start()
    {
        GetTimeFromAPI();
    }

    private void Update()
    {
        if (apiDate != null)
        {
            apiDate = apiDate.AddSeconds(Time.deltaTime);
        }
    }

    private async void GetTimeFromAPI()
    {
        using HttpClient client = new HttpClient();
        try
        {
            HttpResponseMessage response = await client.GetAsync(API_URL);
            response.EnsureSuccessStatusCode(); 

            string responseContent = await response.Content.ReadAsStringAsync();

            TimeApiResponse jsonResponse = JsonUtility.FromJson<TimeApiResponse>(responseContent);

            apiDate = DateTime.Parse(jsonResponse.currentLocalTime);

            Debug.Log($"Current Date : {apiDate}");
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"Error retrieving data from API: {ex.Message}");
        }
    }

    [System.Serializable]
    public class TimeApiResponse
    {
        public string currentLocalTime;
        public string utc_offset;
    }
}
