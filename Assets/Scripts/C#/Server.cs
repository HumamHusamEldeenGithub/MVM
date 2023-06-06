using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Mvm;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

public class Server : MonoBehaviour
{
    #region Static
    public static string ServerUrl = "ec2-16-170-170-2.eu-north-1.compute.amazonaws.com";
    public static string Port = "3000";
    #endregion

    #region POST - GET
    private static async Task<string> createGetCall(string route)
    {
        using HttpClient client = new HttpClient();
        try
        {
            HttpResponseMessage response = await client.GetAsync("http://" + ServerUrl + ":" + Port + route );
            response.EnsureSuccessStatusCode(); // throws exception if HTTP status code is not success (i.e. 200-299)

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            // handle exception
            Debug.LogError($"Error retrieving data from API: {ex.Message}");
            return null;
        }
    }
    private static async Task<string> createPostCall(string route,System.Object body)
    {
        using HttpClient client = new HttpClient();
        try
        {
            Debug.Log(body);
            var jsonBody = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://" + ServerUrl + ":" + Port + route , jsonBody);
            response.EnsureSuccessStatusCode(); // throws exception if HTTP status code is not success (i.e. 200-299)

            string responseContent = await response.Content.ReadAsStringAsync();
            Debug.Log(responseContent);
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            // handle exception
            Debug.LogError($"Error retrieving data from API: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region API
    public static async Task<LoginUserResponse> Login(LoginUserRequest req)
    {
        string res = await createPostCall("/login", req);
        return JsonConvert.DeserializeObject<LoginUserResponse>(res);
    }
    #endregion
}
