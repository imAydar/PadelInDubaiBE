namespace PadelInDubai.Models
{
    public class WebhookBody
    {
        public int CompanyId { get; set; }
        public string Resource { get; set; }
        public int ResourceId { get; set; }
        public string Status { get; set; }
        public RecordData Data { get; set; }
    }
}
