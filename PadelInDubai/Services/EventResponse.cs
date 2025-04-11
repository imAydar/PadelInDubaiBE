using PadelInDubai.Models;

namespace PadelInDubai.Services
{
    public class EventResponse
    {
        public bool Success { get; set; }
        public EventData Data { get; set; }
        public List<object> Meta { get; set; }
    }
}
