using PadelInDubai.DAL;
using PadelInDubai.DAL.Entities;
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

        private const int _gamesId = 10759477;
        private const int _trainsId = 10761747;

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

        public async Task Sync()
        {
            var events = await _client.GetUpcomingEvents();

            var endOfNextDay = DateTime.Today.AddDays(2).AddSeconds(-1);
            events = events.Where(e => e.Date <= endOfNextDay);

            var upcomingEvents = await _eventRepository.UpsertEventsAsync(events.Select(e => e.ToEntity()));
            foreach (var evt in upcomingEvents)
            {
                await _bookingService.SyncById(evt.Id);
                var entity = await _eventRepository.GetByIdAsync(evt.Id);

                if (entity.MessageId.HasValue)
                {
                    try
                    {
                        await _telegramService.UpdateEventMessageAsync(entity.ToDto());
                    }
                    catch(Exception ex)
                    {
                        if (ex.Message == "Bad Request: message to edit not found")
                        {
                            await _telegramService.SendEventMessageAsync(entity.ToDto(), pin: true);
                        }
                    }
                }
                else
                {
                    await _telegramService.SendEventMessageAsync(entity.ToDto(), pin: true);
                }
            }
        }

        public async Task SyncPastDbEvents()
        {
            var pastEvents = (await _eventRepository.GetAllAsync())
                .Where(e => e.Date <= DateTime.Now && !e.LastUpdate)
                .OrderByDescending(e => e.Date)
                .ToList();

            try
            {
                foreach (var pastEvent in pastEvents)
                {
                    await _bookingService.SyncById(pastEvent.Id);
                    pastEvent.LastUpdate = true;
                    //if (pastEvent.Pinned)
                    //{
                    //    _telegramService.Unpin(pastEvent);
                    //}
                }
                await _telegramService.UnpinAll();
                await _eventRepository.UpsertEventsAsync(pastEvents);
            }
            catch(Exception ex)
            {
                var dbg = ex;
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

        public async Task<IEnumerable<ClientDto>> GetClients(DateTime dateTime, Group type)
        {
            var categoryId = type == Group.Game ? _gamesId : _trainsId;
            var evts = await _eventRepository.GetByDate(dateTime, categoryId);
            var evt = evts.Where(e => e.Date == dateTime).FirstOrDefault() ?? evts.FirstOrDefault();
            return evt?.ToDto().Records?.Select(r => new ClientDto
            {
                ClientTags = r.Client.ClientTags,
                DisplayName = r.Client.DisplayName,
                Level = r.Client.Level,
                Phone = r.Client.Phone,
                ClientsCount = r.ClientsCount
            });
        }
    }
}
