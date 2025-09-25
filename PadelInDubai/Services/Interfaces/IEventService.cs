using PadelInDubai.Mappings;
using PadelInDubai.Models.Dtos;

namespace PadelInDubai.Services.Interfaces
{
    public interface IEventService
    {
        Task DeleteAllTgMessages();
        Task DeleteTgData();
        Task<IEnumerable<EventDto>> GetAll();
        Task<EventDto?> GetById(int id);
        Task<IEnumerable<ClientDto>> GetClients(DateTimeOffset dateTime, Group type);
        Task Sync();
        Task SyncPastDbEvents();
    }
}
