using CopyrightRegistrationDemo.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CopyrightRegistrationDemo.Services
{
    public class ModernAuthService
    {
        public HttpClient _httpClient;
        public IConfiguration appSettings;

        public ModernAuthService(HttpClient client, IConfiguration configuration)
        {
            _httpClient = client;
            appSettings = configuration;
        }

        public async Task<authtoken> GetAccessTokenByCC()
        {
            string TenantID = appSettings["AppSettings:TenantId"];
            string ClientID = appSettings["AppSettings:ClientId"];
            string ClientSecret = appSettings["AppSettings:ClientSecret"];
            string Scope = appSettings["AppSettings:VCServiceScope"];
            var req = new HttpRequestMessage(HttpMethod.Post, TenantID + "/oauth2/v2.0/token");
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", ClientID },
                { "client_secret", ClientSecret },
                { "scope", Scope },
                { "tenant", TenantID }
            });

            var response = await _httpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();
            string content = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            var mytoken = JsonConvert.DeserializeObject<authtoken>(content);
            return mytoken;
        }

    }
}
