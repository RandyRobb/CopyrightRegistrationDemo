using Newtonsoft.Json;

namespace CopyrightRegistrationDemo.Models
{
    public class Headers
    {
        [JsonProperty("api-key")]
        public string ApiKey { get; set; }
    }

    public class Callback
    {
        public string url { get; set; }
        public string state { get; set; }
        public Headers headers { get; set; }
    }

    public class Registration
    {
        public string clientName { get; set; }
    }

    public class Pin
    {
        public string value { get; set; }
        public int length { get; set; }
    }

    public class Claims
    {
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string image_file { get; set; }
        public string name { get; set; }
        public string id { get; set; }


    }

    public class Issuance
    {
        public string type { get; set; }
        public string manifest { get; set; }
        public Pin pin { get; set; }
        public Claims claims { get; set; }
    }

    public class VCPayload
    {
        public bool includeQRCode { get; set; }
        public Callback callback { get; set; }
        public string authority { get; set; }
        public Registration registration { get; set; }
        public Issuance issuance { get; set; }
    }


}
