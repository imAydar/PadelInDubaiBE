using PadelInDubai.Models;

namespace PadelInDubai.Services.Interfaces
{
    public interface IExternalEventService
    {
        Task<EventData> GetEvent(int activityId);
        Task<IEnumerable<RecordData>> GetRecords(int activityId);
        Task<IEnumerable<EventData>> GetUpcomingEvents();
    }
}
