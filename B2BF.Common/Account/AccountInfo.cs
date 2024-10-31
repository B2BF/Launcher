using B2BF.Common.Data;
using B2BF.Common.Networking.GameSpy.Login.Packets;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace B2BF.Common.Account
{
    public static class AccountInfo
    {
        private static HttpClient _client;

        private static string? _code_verifier;
        private static string? _state;

        public static string? AccessToken;
        public static string? Username;
        public static string? UId;
        public static string? CdKey;

        public static Action? OnAccountInfoChanged;

        static AccountInfo()
        {
            _client = new HttpClient();
            AccessToken = Settings.RememberMeContainer;
        }

        public static bool HasLoggedInUser()
        {
            return !string.IsNullOrEmpty(AccessToken);
        }

        public static string GetLoginUrl()
        {
            _code_verifier = LoginPacket.RandomString(32);
            _state = LoginPacket.RandomString(32);
            var code_verifier_hash = "";
            using (var sha = SHA256.Create())
            {
                byte[] input = Encoding.UTF8.GetBytes(_code_verifier);
                var hash = sha.ComputeHash(input);
                code_verifier_hash = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            }
            return $"https://accounts.phoenixnetwork.net/account/authorize" +
                $"?redirect_uri=http://localhost:8888/oauthloginreturn" +
                $"&state={_state}" +
                $"&client_id=cc715845-80f4-4086-8074-64a47fda5474&scope=openid%20bf2&code_challenge={code_verifier_hash}&code_challenge_method=S256";
        }

        public static async void ValidateLoginResult(string code, string state)
        {
            if (_state != state)
            {
                return;
            }

            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", "authorization_code");
            dict.Add("client_id", "cc715845-80f4-4086-8074-64a47fda5474");
            dict.Add("code", code);
            dict.Add("code_verifier", _code_verifier);
            var tokenResponse = await _client.PostAsync("https://accounts.phoenixnetwork.net/oauth/token", new FormUrlEncodedContent(dict));
            tokenResponse.EnsureSuccessStatusCode();
            var responseData = await tokenResponse.Content.ReadAsStringAsync();
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);

            AccessToken = responseDict["access_token"];

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var userResponse = await _client.GetStringAsync("https://accounts.phoenixnetwork.net/oauth/user");
            var userDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(userResponse);
            UId = (string)userDict["uid"];
            Username = (string)userDict["username"];

            Settings.RememberMeContainer = AccessToken;

            OnAccountInfoChanged();
        }

        public static async Task<bool> ValidateTokenAsync()
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                var loginResponse = await _client.GetAsync("https://accounts.phoenixnetwork.net/oauth/user");

                loginResponse.EnsureSuccessStatusCode();

                var userDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(await loginResponse.Content.ReadAsStringAsync());
                UId = (string)userDict["uid"];
                Username = (string)userDict["username"];

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<string> GetCdKeyAsync()
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                var loginResponse = await _client.GetAsync("https://b2bf2.net/api/gamespy/cdkey");

                loginResponse.EnsureSuccessStatusCode();

                return await loginResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}