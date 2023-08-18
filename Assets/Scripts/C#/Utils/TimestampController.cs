using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TimestampController : MonoBehaviour
{
    private const string API_URL = "http://worldclockapi.com/api/json/est/now";
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

            apiDate = DateTime.Parse(jsonResponse.currentDateTime);

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
        public string currentDateTime;
        public string utc_offset;
    }
}
