namespace CopyrightRegistrationDemo.Models
{
    public class VCVPayload
    {
        public bool includeQRCode { get; set; }
        public Callback callback { get; set; }
        public string authority { get; set; }
        public Registration registration { get; set; }
        public Presentation presentation { get; set; }
    }

    public class Presentation
    {
        public bool includeReceipt { get; set; }
        public List<RequestedCredential> requestedCredentials { get; set; }
    }

    public class RequestedCredential
    {
        public string type { get; set; }
        public string purpose { get; set; }
        public List<string> acceptedIssuers { get; set; }
    }

}
