namespace PadelInDubai.Models
{
    public class WebhookRequest
    {
        public DateTime Timestamp { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public WebhookBody Body { get; set; }
    }
}
