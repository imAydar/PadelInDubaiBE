namespace PadelInDubai.Models
{
    public class EventsResponse
    {
        public bool Success { get; set; }
        public List<EventData> Data { get; set; }
        public Meta Meta { get; set; }
    }

}
