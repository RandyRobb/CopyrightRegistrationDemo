using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;
using CopyrightRegistrationDemo.Models;
using CopyrightRegistrationDemo.Services;

namespace CopyrightRegistrationDemo.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IssuerController : ControllerBase
    {
        const string ISSUANCEPAYLOAD = "issuance_request_config.json";

        protected readonly AppSettingsModel AppSettings;
        protected IMemoryCache _cache;
        protected readonly ILogger<IssuerController> _log;
        private IHttpClientFactory _httpClientFactory;
        private string _apiKey;
        public IssuerController(IOptions<AppSettingsModel> appSettings, IMemoryCache memoryCache, ILogger<IssuerController> log, IHttpClientFactory httpClientFactory)
        {
            this.AppSettings = appSettings.Value;
            _cache = memoryCache;
            _log = log;
            _httpClientFactory = httpClientFactory;
            _apiKey = System.Environment.GetEnvironmentVariable("API-KEY");
        }



        /// <summary>
        /// This method is called by the VC Request API when the user scans a QR code and accepts the issued Verifiable Credential
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> IssuanceCallback()
        {
            try
            {
                string content = await new System.IO.StreamReader(this.Request.Body).ReadToEndAsync();
                this.Request.Headers.TryGetValue("api-key", out var apiKey);
                if (this._apiKey != apiKey)
                {
                    return new ContentResult() { StatusCode = (int)HttpStatusCode.Unauthorized, Content = "api-key wrong or missing" };
                }
                VCCallback issuanceResponse = JsonConvert.DeserializeObject<VCCallback>(content);
                string state = issuanceResponse.state;

                //there are 2 different callbacks. 1 if the QR code is scanned (or deeplink has been followed)
                //Scanning the QR code makes Authenticator download the specific request from the server
                //the request will be deleted from the server immediately.
                //That's why it is so important to capture this callback and relay this to the UI so the UI can hide
                //the QR code to prevent the user from scanning it twice (resulting in an error since the request is already deleted)
                if (issuanceResponse.code == "request_retrieved")
                {
                    var cacheData = new
                    {
                        status = "request_retrieved",
                        message = "QR Code is scanned. Waiting for issuance...",
                    };
                    _cache.Set(state, JsonConvert.SerializeObject(cacheData));
                }

                //
                //This callback is called when issuance is completed.
                //
                if (issuanceResponse.code == "issuance_successful")
                {
                    var cacheData = new
                    {
                        status = "issuance_successful",
                        message = "Credential successfully issued",
                    };
                    _cache.Set(state, JsonConvert.SerializeObject(cacheData));
                }
                //
                //We capture if something goes wrong during issuance. See documentation with the different error codes
                //
                if (issuanceResponse.code == "issuance_error")
                {
                    JObject issuanceResponseError = JObject.Parse(content);
                    var cacheData = new
                    {
                        status = "issuance_error",
                        payload = issuanceResponseError["error"]["code"].ToString(),
                        //at the moment there isn't a specific error for incorrect entry of a pincode.
                        //So assume this error happens when the users entered the incorrect pincode and ask to try again.
                        message = issuanceResponseError["error"]["message"].ToString()

                    };
                    _cache.Set(state, JsonConvert.SerializeObject(cacheData));
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        //
        //this function is called from the UI polling for a response from the AAD VC Service.
        //when a callback is recieved at the issuanceCallback service the session will be updated
        //this method will respond with the status so the UI can reflect if the QR code was scanned and with the result of the issuance process
        //
        [HttpGet("/api/issuer/issuance-response")]
        public ActionResult IssuanceResponse()
        {
            try
            {
                //the id is the state value initially created when the issuanc request was requested from the request API
                //the in-memory database uses this as key to get and store the state of the process so the UI can be updated
                string state = this.Request.Query["id"];
                if (string.IsNullOrEmpty(state))
                {
                    return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
                }
                JObject value = null;
                if (_cache.TryGetValue(state, out string buf))
                {
                    value = JObject.Parse(buf);

                    Debug.WriteLine("check if there was a response yet: " + value);
                    return new ContentResult { ContentType = "application/json", Content = JsonConvert.SerializeObject(value) };
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        
        protected string GetRequestHostName()
        {
            string scheme = "https";// : this.Request.Scheme;
            string originalHost = this.Request.Headers["x-original-host"];
            string hostname = "";
            if (!string.IsNullOrEmpty(originalHost))
                hostname = string.Format("{0}://{1}", scheme, originalHost);
            else hostname = string.Format("{0}://{1}", scheme, this.Request.Host);
            return hostname;
        }

        protected bool IsMobile()
        {
            string userAgent = this.Request.Headers["User-Agent"];

            if (userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return true;
            else
                return false;
        }
    }
}
