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
    public static string PythonServerUrl = "localhost";
    public static string Port = "3000";
    #endregion

    #region POST - GET - Delete
    private static async Task<string> CreateGetCall(string route)
    {
        using HttpClient client = new HttpClient();
        try
        {
            // TODO : use userdata.token instead of the static token 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserProfile.Instance.Token);
            HttpResponseMessage response = await client.GetAsync("http://" + ServerUrl + ":" + Port + route );
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
    private static async Task<string> CreatePostCall(string route,System.Object body)
    {
        using HttpClient client = new HttpClient();
        try
        {
            // TODO : use userdata.token instead of the static token 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserProfile.Instance.Token);
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
    private static async Task<string> CreateDeleteCall(string route, System.Object body)
    {
        using HttpClient client = new HttpClient();
        try
        {
            // TODO : use userdata.token instead of the static token 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserProfile.Instance.Token);
            var jsonBody = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, "http://" + ServerUrl + ":" + Port + route)
            {
                Content = jsonBody
            };

            HttpResponseMessage response = await client.SendAsync(request);
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

    public static async Task<string> UploadFile(byte[] fileBytes)
    {
        string fileName = "Photo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        using HttpClient httpClient = new HttpClient();
        try
        {
            using MultipartFormDataContent form = new MultipartFormDataContent();
            ByteArrayContent content = new ByteArrayContent(fileBytes);
            content.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{fileName}\"");
            form.Add(content, "file", fileName);

            HttpResponseMessage response = await httpClient.PostAsync(PythonServerUrl, form);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log("File upload successful. Response: " + responseBody);
            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError("Error uploading file: " + ex.Message);
            return null;
        }
    }
    #endregion

    #region API
    public static async Task<LoginUserResponse> Login(LoginUserRequest req)
    {
        string res = await CreatePostCall("/login", req);

        return res != null ? JsonConvert.DeserializeObject<LoginUserResponse>(res) : null;
    }
    public static async Task<LoginByRefreshTokenResponse> LoginByRefreshToken(LoginByRefreshTokenRequest req)
    {
        string res = await CreatePostCall("/refresh_token", req);

        return res != null ? JsonConvert.DeserializeObject<LoginByRefreshTokenResponse>(res) : null;
    }
    public static async Task<CreateUserResponse>CreateUser(CreateUserRequest req)
    {
        string res = await CreatePostCall("/create", req);

        return res != null ? JsonConvert.DeserializeObject<CreateUserResponse>(res) : null;
    }
    public static async Task<CreateFriendRequestResponse> CreateFriendRequest(CreateFriendRequestRequest req)
    {
        string res = await CreatePostCall("/friends/requests/send", req);

        return res != null ? JsonConvert.DeserializeObject<CreateFriendRequestResponse>(res) : null;
    }
    public static async Task<DeleteFriendRequestResponse> DeleteFriendRequest(DeleteFriendRequestRequest req)
    {
        string res = await CreatePostCall("/friends/requests/delete", req);

        return res != null ? JsonConvert.DeserializeObject<DeleteFriendRequestResponse>(res) : null;
    }
    public static async Task<GetPendingFriendsResponse> GetPendingFriendRequests()
    {
        string res = await CreateGetCall("/friends/requests/pending");

        return res != null ? JsonConvert.DeserializeObject<GetPendingFriendsResponse>(res) : null;
    }
    public static async Task<AddFriendResponse> AddFriend(AddFriendRequest req)
    {
        string res = await CreatePostCall("/friends/accept", req);

        return res != null ? JsonConvert.DeserializeObject<AddFriendResponse>(res) : null;
    }
    public static async Task<DeleteFriendResponse> DeleteFriend(DeleteFriendRequest req)
    {
        string res = await CreatePostCall("/friends/delete", req);

        return res != null ? JsonConvert.DeserializeObject<DeleteFriendResponse>(res) : null;
    }
    public static async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest req)
    {
        string res = await CreatePostCall("/rooms", req);

        return res != null ? JsonConvert.DeserializeObject<CreateRoomResponse>(res) : null;
    }
    public static async Task<DeleteRoomResponse> DeleteRoom(DeleteRoomRequest req)
    {
        string res = await CreateDeleteCall("/rooms", req);

        return res != null ? JsonConvert.DeserializeObject<DeleteRoomResponse>(res) : null;
    }
    public static async Task<CreateRoomInvitationResponse> CreateRoomInvitation(CreateRoomInvitationRequest req)
    {
        string res = await CreatePostCall("/rooms/invitations", req);

        return res != null ? JsonConvert.DeserializeObject<CreateRoomInvitationResponse>(res) : null;
    }
    public static async Task<DeleteRoomInvitationResponse> DeleteRoomInvitation(DeleteRoomInvitationRequest req)
    {
        string res = await CreateDeleteCall("/rooms/invitations", req);

        return res != null ? JsonConvert.DeserializeObject<DeleteRoomInvitationResponse>(res) : null;
    }
    public static async Task<SearchForUsersResponse> SearchForUsers(SearchForUsersRequest req)
    {
        string res = await CreatePostCall("/user/search", req);

        return res != null ? JsonConvert.DeserializeObject<SearchForUsersResponse>(res) : null;
    }
    public static async Task<GetProfileResponse> GetProfile(string userId)
    {
        string res = await CreateGetCall($"/user?user={userId}");

        return res != null ? JsonConvert.DeserializeObject<GetProfileResponse>(res) : null;
    }
    public static async Task<GetRoomsResponse> GetRooms(string searchQuery)
    {
        string res = await CreateGetCall($"/rooms?search={searchQuery}");

        return res != null ? JsonConvert.DeserializeObject<GetRoomsResponse>(res) : null;
    }
    public static async Task<GetFriendsResponse> GetFriends()
    {
        string res = await CreateGetCall("/friends");

        return res != null ? JsonConvert.DeserializeObject<GetFriendsResponse>(res) : null;
    }
    public static async Task<GetUserByUsernameResponse> GetUserByUsername(GetUserByUsernameRequest req)
    {
        string res = await CreatePostCall("/user/get" , req);

        return res != null ? JsonConvert.DeserializeObject<GetUserByUsernameResponse>(res) : null;
    }

    public static async Task<UpsertAvatarSettingsResponse> UpsertAvatarSettings(UpsertAvatarSettingsRequest req)
    {
        string res = await CreatePostCall("/user/avatar", req);

        return res != null ? JsonConvert.DeserializeObject<UpsertAvatarSettingsResponse>(res) : null;
    }
    public static async Task<GetAvatarSettingsResponse> GetAvatarSettings()
    {
        string res = await CreateGetCall("/user/avatar");

        return res != null ? JsonConvert.DeserializeObject<GetAvatarSettingsResponse>(res) : null;
    }

    public static async Task<GetUserProfileFeaturesResponse> GetUserProfileFeatures(GetUserProfileFeaturesRequest req)
    {
        string res = await CreatePostCall("/user/features", req);

        return res != null ? JsonConvert.DeserializeObject<GetUserProfileFeaturesResponse>(res) : null;
    }

    #endregion
}
