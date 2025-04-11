using PadelInDubai.DAL.Entities;

namespace PadelInDubai.DAL
{
    public interface IBookingRepository
    {
        Task<Record> Create(Record record);
        Task<Record> Delete(Record record);
        Task<List<Record>> SaveRecordsAsync(List<Record> recordGot);
        Task<Record> Update(Record record);
    }
}
