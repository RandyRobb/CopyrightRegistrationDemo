using CopyrightRegistrationDemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace CopyrightRegistrationDemo.Services
{
    public class VCService
    {
        public HttpClient _httpClient;
        protected IMemoryCache _cache;


        public VCService(HttpClient client, IMemoryCache memoryCache)
        {
            _httpClient = client;
            _cache = memoryCache;

        }


        
        public async Task<string> PostPayloadAsync(string access_token, VCPayload myPayload)
        {
            string errorReturn = null;
            string payload = JsonConvert.SerializeObject(myPayload);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            //_httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");
            HttpContent dataString = new StringContent(payload, Encoding.UTF8, "application/json");
            //dataString.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            //var buffer = System.Text.Encoding.UTF8.GetBytes(myPolicy);
            //var byteContent = new ByteArrayContent(buffer);
            var response = await _httpClient.PostAsync("tenantid/verifiablecredentials/request", dataString);
            string content = "Something Went Wrong";
            try
            {
                response.EnsureSuccessStatusCode();
                content = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    JObject requestConfig = JObject.Parse(content);
                    var cacheData = new
                    {
                        status = "notscanned",
                        message = "Request ready, please scan with Authenticator",
                        expiry = requestConfig["expiry"].ToString()
                    };
                    _cache.Set(myPayload.callback.state, JsonConvert.SerializeObject(cacheData));
                    //For debugging
                    string cacheValue;
                    _cache.TryGetValue("states", out cacheValue);
                    if(cacheValue == null)
                    {
                        cacheValue = myPayload.callback.state;
                    }
                    else
                    {
                        cacheValue = cacheValue + ";" + myPayload.callback.state;
                    }
                    _cache.Set("states", cacheValue);
                }
                
            }
            catch (HttpRequestException ex)
            {
                StreamReader sr = new StreamReader(response.Content.ReadAsStream());
                string thisone = sr.ReadToEnd();
                PublishError policyError = new PublishError();
                policyError = JsonConvert.DeserializeObject<PublishError>(thisone);
                content = policyError.error.message;
            }
            
            //string content = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return content;
        }


        public async Task<string> PostVPayloadAsync(string access_token, VCVPayload myPayload)
        {
            string errorReturn = null;
            string payload = JsonConvert.SerializeObject(myPayload);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            //_httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");
            HttpContent dataString = new StringContent(payload, Encoding.UTF8, "application/json");
            //dataString.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            //var buffer = System.Text.Encoding.UTF8.GetBytes(myPolicy);
            //var byteContent = new ByteArrayContent(buffer);
            var response = await _httpClient.PostAsync("tenantid/verifiablecredentials/request", dataString);
            string content = "Something Went Wrong";
            try
            {
                response.EnsureSuccessStatusCode();
                content = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    JObject requestConfig = JObject.Parse(content);
                    var cacheData = new
                    {
                        status = "notscanned",
                        message = "Request ready, please scan with Authenticator",
                        expiry = requestConfig["expiry"].ToString()
                    };
                    _cache.Set(myPayload.callback.state, JsonConvert.SerializeObject(cacheData));
                    //for debugging
                    string cacheValue;
                    _cache.TryGetValue("states", out cacheValue);
                    if (cacheValue == null)
                    {
                        cacheValue = myPayload.callback.state;
                    }
                    else
                    {
                        cacheValue = cacheValue + ";ID:" + myPayload.callback.state;
                    }
                    _cache.Set("states", cacheValue);
                }

            }
            catch (HttpRequestException ex)
            {
                StreamReader sr = new StreamReader(response.Content.ReadAsStream());
                string thisone = sr.ReadToEnd();
                PublishError policyError = new PublishError();
                policyError = JsonConvert.DeserializeObject<PublishError>(thisone);
                content = policyError.error.message;
            }

            //string content = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return content;
        }

    }
}
