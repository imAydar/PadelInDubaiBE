using PadelInDubai.DAL;
using PadelInDubai.DAL.Entities;
using PadelInDubai.Mappings;
using PadelInDubai.Models;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
        private readonly IEventRepository _eventRepository;
        private readonly IExternalEventService _client;
        private readonly TelegramService _telegramService;

        public BookingService(
            IBookingRepository repository, 
            IEventRepository eventRepository,
            IExternalEventService client, 
            TelegramService telegramService)
        {
            _repository = repository;
            _eventRepository = eventRepository;
            _client = client;
            _telegramService = telegramService;
        }

        public async Task Sync_v2(RecordData recordData)
        {
            await _repository.Create(recordData.ToEntity());
        }

        public async Task Delete(RecordData recordData)
        {
            await _repository.Delete(recordData.ToEntity());
        }

        public async Task Update(RecordData recordData)
        {
            await _repository.Update(recordData.ToEntity());
        }

        public async Task<List<Record>> Sync(RecordData recordData)
        {
            return await SyncById(recordData.ActivityId, sendMessages: true);
        }

        public async Task<List<Record>> SyncById(int activityId, bool sendMessages = false)
        {
            var eventDto = await _client.GetEvent(activityId);
            var evt = (await _eventRepository.UpsertEventsAsync([eventDto.ToEntity()]))
                .First();
            evt = await _eventRepository.GetByIdAsync(activityId);

            var records = await _client.GetRecords(activityId);
            var existingRecords = evt.Records?.ToList() ?? [];
            
            var recordsToDelete = existingRecords
                .Where(existing => !records.Any(r => r.Id == existing.Id))
                .ToList();

            foreach (var recordToDelete in recordsToDelete)
            {
                await _repository.Delete(recordToDelete);
            }

            var saved = await _repository.SaveRecordsAsync(records.Select(r => r.ToEntity()).ToList());
            //TODO: refactor.
            evt = await _eventRepository.GetByIdAsync(activityId);
            if (sendMessages)
            {
                if (!evt.MessageId.HasValue)
                {
                    if (evt.Date.Date >= DateTimeOffset.Now.Date && evt.Date.Date <= DateTimeOffset.Now.Date.AddDays(1))
                    {
                        await _telegramService.SendEventMessageAsync(evt.ToDto());
                    }
                }
                else
                {
                    try
                    {
                        await _telegramService.UpdateEventMessageAsync(evt.ToDto());
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Bad Request: message to edit not found")
                        {
                            await _telegramService.SendEventMessageAsync(evt.ToDto());
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return saved;
        }
    }
}
