using PadelInDubai.DAL;
using PadelInDubai.Mappings;
using PadelInDubai.Models.Dtos;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Services
{
    public class EventService(
        IEventRepository eventRepository,
        IExternalEventService client,
        TelegramService telegramService,
        IBookingService bookingService) : IEventService
    {
        private readonly IExternalEventService _client = client;
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly TelegramService _telegramService = telegramService;
        private readonly IBookingService _bookingService = bookingService;

        public async Task DeleteAllTgMessages()
        {
            var events = await _eventRepository.GetAllAsync();
            var messageIds = events
                .Where(e => e.MessageId.HasValue)
                .Select(e => e.MessageId.Value)
                .ToList();
                
            await _telegramService.DeleteMessages(messageIds);
            
            foreach (var evt in events)
            {
                evt.TextHash = null;
                evt.MessageId = null;
            }
            
            await _eventRepository.UpsertEventsAsync(events);
        }

        public async Task DeleteTgData()
        {
            var events = await _eventRepository.GetAllAsync();
            
            foreach (var evt in events)
            {
                evt.MessageId = null;
                evt.TextHash = null;
            }
            
            await _eventRepository.UpsertEventsAsync(events);
        }

        public async Task<IEnumerable<EventDto>> GetAll()
        {
            var events = await _eventRepository.GetAllAsync();
            return events.Select(e => e.ToDto());
        }

        public async Task<EventDto?> GetById(int id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            return eventEntity?.ToDto();
        }

        public async Task Sync(bool syncTillEndOfTheWeek = false)
        {
            var events = await _client.GetUpcomingEvents();
            
            if (syncTillEndOfTheWeek)
            {
                var endOfSunday = DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek).Date;
                events = events.Where(e => e.Date <= endOfSunday);
            }

            var entities = await _eventRepository.UpsertEventsAsync(events.Select(e => e.ToEntity()));
            
            var upcomingEvents = entities
                .Where(x => x.Date <= DateTime.UtcNow.AddDays(7))
                .OrderBy(x => x.Date);
                
            foreach (var evt in upcomingEvents)
            {
                await _bookingService.SyncById(evt.Id);
                var entity = await _eventRepository.GetByIdAsync(evt.Id);
                
                if (entity.MessageId.HasValue)
                {
                    await _telegramService.UpdateEventMessageAsync(entity.ToDto());
                }
                else
                {
                    await _telegramService.SendEventMessageAsync(entity.ToDto());
                }
            }
        }

        public async Task SyncPastDbEvents()
        {
            var pastEvents = (await _eventRepository.GetAllAsync())
                .Where(e => e.Date.Day == 9 && e.Date.Month == 4)
                //.Where(e => e.Date <= DateTime.Now && !e.LastUpdate && e.Date > DateTime.Now.AddDays(-7))
                .OrderByDescending(e => e.Date)
                .ToList();

            try
            {
                foreach (var pastEvent in pastEvents)
                {
                    if (pastEvent.Date.Day == 9)
                    {
                        var t = 0;
                    }
                    await _bookingService.SyncById(pastEvent.Id);
                    pastEvent.LastUpdate = true;
                }

                await _eventRepository.UpsertEventsAsync(pastEvents);
            }
            catch(Exception ex)
            {
                var t1 = ex;
            }
        }

        public async Task UpdateMessageId(int eventId, int messageId)
        {
            var evt = await _eventRepository.GetByIdAsync(eventId);
            if (evt != null)
            {
                evt.MessageId = messageId;
                await _eventRepository.UpsertEventsAsync([evt]);
            }
        }
    }
}
