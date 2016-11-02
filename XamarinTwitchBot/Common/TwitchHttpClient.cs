// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   This class provides an interface to the basic twitch.tv site functionality
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using Newtonsoft.Json.Linq;

    public class TwitchHttpClient
    {
        protected readonly HttpClient Client;
        private const string CookiesPrefsKey = "cookies";
        private readonly string cookiesName;
        private readonly CookieContainer cookies;

        public TwitchHttpClient(string cookiesName)
        {
            this.cookiesName = cookiesName;

            this.cookies = this.LoadCookies();

            var handler = new HttpClientHandler { CookieContainer = this.cookies, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            this.Client = new HttpClient(handler);
            this.Client.DefaultRequestHeaders.ExpectContinue = false;
            this.Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
            this.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            this.Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        }

        public string OAuth { get; private set; }

        public bool IsAuthenticated() =>
            this.cookies.Count > 5; // not the best solution, but it's working...

        public async Task LogInAsync(string username, string password)
        {
            HttpResponseMessage response;

            using (var request = new HttpRequestMessage(HttpMethod.Get, "https://www.twitch.tv/login"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                response = await this.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

            var referrer = new Uri(response.RequestMessage.RequestUri.AbsoluteUri);

            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://passport.twitch.tv/authentications/new"))
            {
                var parsedQuery = HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
                request.Content = new StringContent(
                    "username=" + username +
                    "&password=" + password +
                    "&client_id=" + parsedQuery["client_id"] +
                    "&nonce=" + parsedQuery["nonce"] +
                    "&scope=" + parsedQuery["scope"] +
                    "&state=" + parsedQuery["state"] + "&redirect_uri=" + "https://www.twitch.tv/");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Referrer = referrer;

                response = await this.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

            var strResponse = await response.Content.ReadAsStringAsync();
            var redirect = (string)JObject.Parse(strResponse)["redirect"];

            using (var request = new HttpRequestMessage(HttpMethod.Get, redirect))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Referrer = referrer;

                response = await this.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

            Utils.LogInfo("Login status " + response.StatusCode + " (" + (int)response.StatusCode + ")");

            this.SaveCookies();

            Utils.LogInfo("Writing cookies ok");
        }
        
        public async Task<string> GetOAuthAsync()
        {
            HttpResponseMessage response;
            var allCookies = this.GetAllCookies();

            using (var request = new HttpRequestMessage(HttpMethod.Get, "https://www.twitch.tv/tmilibs/tmi-v3.js"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                response = await this.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

            var strResponse = await response.Content.ReadAsStringAsync();
            var clientId = Regex.Match(strResponse, "\"Client-ID\"\\s*:\\s*\"(\\w+)\"").Groups[1].Value;

            Utils.LogInfo("Client-ID: " + clientId);

            using (var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/api/me?on_site=1"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v2+json"));
                request.Headers.Add("Client-ID", clientId);
                var csrfToken = allCookies["csrf_token"];
                if (csrfToken == null) Utils.LogError("CSRF_TOKEN IS NULL");
                else request.Headers.Add("X-CSRF-Token", csrfToken.Value);
                var apiToken = allCookies["api_token"];
                if (apiToken == null) Utils.LogError("API_TOKEN IS NULL");
                else request.Headers.Add("Twitch-Api-Token", apiToken.Value);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                response = await this.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

            strResponse = await response.Content.ReadAsStringAsync();
            this.OAuth = (string)JObject.Parse(strResponse)["chat_oauth_token"];

            Utils.LogInfo("OAuth: " + this.OAuth);

            return this.OAuth;
        }

        public async Task<JToken> GetChattersAsync(string channelName, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "https://tmi.twitch.tv/group/user/" + channelName + "/chatters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                var response = await this.Client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var strResponse = await response.Content.ReadAsStringAsync();
                var chatters = JObject.Parse(strResponse)["chatters"]["viewers"];

                return chatters;
            }
        }

        public async Task<JToken> FindGroupMembershipAsync(string groupName, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "https://chatdepot.twitch.tv/room_memberships?oauth_token=" + this.OAuth))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("Authorization", "OAuth " + this.OAuth);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                var response = await this.Client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var strResponse = await response.Content.ReadAsStringAsync();
                var memberships = JObject.Parse(strResponse)["memberships"];
                foreach (var member in memberships)
                    if (string.Equals(groupName, (string)member["room"]["display_name"], StringComparison.OrdinalIgnoreCase))
                        return member;

                return null;
            }
        }

        public void DeleteCookies()
        {
            SharedPreferences.Delete(this.cookiesName);
        }

        private void SaveCookies()
        {
            SharedPreferences.SaveObject(this.cookies, CookiesPrefsKey, this.cookiesName);
        }

        private CookieContainer LoadCookies()
        {
            try
            {
                return (CookieContainer)SharedPreferences.LoadObject(CookiesPrefsKey, this.cookiesName);
            }
            catch (Exception ex)
            {
                Utils.LogError("Failed loading textCommands, creating new.\n" + ex);
                return new CookieContainer();
            }
        }

        private CookieCollection GetAllCookies()
        {
            var cookieCollection = new CookieCollection();

            var table = (Hashtable)this.cookies.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                null,
                this.cookies,
                new object[] { });

            foreach (var tableKey in table.Keys)
            {
                var strTableKey = (string)tableKey;

                if (strTableKey[0] == '.')
                {
                    strTableKey = strTableKey.Substring(1);
                }

                var list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                    BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                    null,
                    table[tableKey],
                    new object[] { });

                foreach (var listKey in list.Keys)
                {
                    var url = "https://" + strTableKey + (string)listKey;
                    cookieCollection.Add(this.cookies.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }
    }
}