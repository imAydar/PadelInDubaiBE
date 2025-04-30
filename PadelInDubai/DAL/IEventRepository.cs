using PadelInDubai.DAL.Entities;

namespace PadelInDubai.DAL
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetByDate(DateTime date, int categoryId);
        Task<Event?> GetByIdAsync(int id);
        Task<List<Event>> GetByIdsAsync(List<int> ids);
        Task UpdateMessage(int eventId, int messageId, int hash);
        Task<IEnumerable<Event>> UpsertEventsAsync(IEnumerable<Event> events);
    }
}
