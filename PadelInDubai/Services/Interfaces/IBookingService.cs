using PadelInDubai.DAL.Entities;
using PadelInDubai.Models;

namespace PadelInDubai.Services.Interfaces
{
    public interface IBookingService
    {
        Task Sync_v2(RecordData record);
        Task Delete(RecordData record);
        Task<List<Record>> SyncById(int activityId, bool sendMessages = false);
        Task Update(RecordData record);
        Task<List<Record>> Sync(RecordData recordData);
    }
}
