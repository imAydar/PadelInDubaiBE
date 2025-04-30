using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PadelInDubai.DAL.Entities;
using PadelInDubai.Extensions;

namespace PadelInDubai.DAL
{
    public class EventRepository(ApplicationDbContext context, IMemoryCache cache) : IEventRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMemoryCache _cache = cache;

        public async Task UpdateMessage(int eventId, int messageId, int hash)
        {
            var entity = await _context.Events.FindAsync(eventId);
            if (entity != null)
            {
                entity.MessageId = messageId;
                entity.TextHash = hash;
                await _context.SaveChangesAsync();
            }

            _cache.Clear();
        }

        public async Task<IEnumerable<Event>> UpsertEventsAsync(IEnumerable<Event> events)
        {
            _cache.Clear();
            
            // Validate input
            var eventsList = events.ToList();
            if (eventsList.Any(e => e.Service == null || e.Staff == null))
            {
                throw new ArgumentException("All events must have a non-null Service and Staff.");
            }

            // Extract all related entities
            var categories = eventsList
                .Select(x => x.Service.Category)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            var services = eventsList
                .Select(x => x.Service)
                .Distinct()
                .ToList();

            var staff = eventsList
                .Select(x => x.Staff)
                .Distinct()
                .ToList();

            // Process Categories - manual upsert approach
            if (categories.Any())
            {
                var categoryIds = categories.Select(c => c.Id).ToList();
                var existingCategories = await _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id);

                foreach (var category in categories)
                {
                    if (existingCategories.TryGetValue(category.Id, out var existingCategory))
                    {
                        // Update existing category
                        _context.Entry(existingCategory).CurrentValues.SetValues(category);
                    }
                    else
                    {
                        // Add new category
                        _context.Categories.Add(category);
                    }
                }
                
                await _context.SaveChangesAsync();
            }

            // Process Services - manual upsert approach
            if (services.Any())
            {
                var serviceIds = services.Select(s => s.Id).ToList();
                var existingServices = await _context.Services
                    .Where(s => serviceIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);

                foreach (var service in services)
                {
                    // Detach Category reference to avoid tracking conflicts
                    service.Category = null;
                    
                    if (existingServices.TryGetValue(service.Id, out var existingService))
                    {
                        // Update existing service
                        _context.Entry(existingService).CurrentValues.SetValues(service);
                    }
                    else
                    {
                        // Add new service
                        _context.Services.Add(service);
                    }
                }
                
                await _context.SaveChangesAsync();
            }

            // Process Staff - manual upsert approach
            if (staff.Any())
            {
                var staffIds = staff.Select(s => s.Id).ToList();
                var existingStaff = await _context.Staffs
                    .Where(s => staffIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);

                foreach (var staffMember in staff)
                {
                    if (existingStaff.TryGetValue(staffMember.Id, out var existingStaffMember))
                    {
                        staffMember.LocationUrl = existingStaffMember.LocationUrl;
                        // Update existing staff
                        _context.Entry(existingStaffMember).CurrentValues.SetValues(staffMember);
                    }
                    else
                    {
                        // Add new staff
                        _context.Staffs.Add(staffMember);
                    }
                }
                
                await _context.SaveChangesAsync();
            }

            // Process Events - we need to preserve MessageId and TextHash for existing events
            var eventIds = eventsList.Select(e => e.Id).Distinct().ToList();
            var existingEvents = await _context.Events
                .Where(e => eventIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id);

            foreach (var evt in eventsList)
            {
                if (existingEvents.TryGetValue(evt.Id, out var existingEvent))
                {
                    // Preserve MessageId and TextHash from existing event
                    evt.MessageId = existingEvent.MessageId;
                    evt.TextHash = existingEvent.TextHash;
                    
                    // Update existing event
                    _context.Entry(existingEvent).CurrentValues.SetValues(evt);
                }
                else
                {
                    // Detach navigation properties to avoid tracking conflicts
                    evt.Service = null;
                    evt.Staff = null;
                    
                    // Add new event
                    _context.Events.Add(evt);
                }
            }
            
            await _context.SaveChangesAsync();

            _cache.Clear();
            return events;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            string cacheKey = "AllEvents";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Event> events))
            {
                events = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Service)
                        .ThenInclude(s => s.Category)
                    .Include(e => e.Records)
                        .ThenInclude(r => r.Client)
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, events, cacheEntryOptions);
            }

            return events;
        }

        public async Task<IEnumerable<Event>> GetByDate(DateTime date, int categoryId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Events
                .Where(e => e.Date >= startOfDay && e.Date < endOfDay && e.Service.CategoryId == categoryId)
                .Include(e => e.Staff)
                .Include(e => e.Service)
                    .ThenInclude(s => s.Category)
                .Include(e => e.Records)
                    .ThenInclude(r => r.Client)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            string cacheKey = $"Event_{id}";
            if (!_cache.TryGetValue(cacheKey, out Event? eventEntity))
            {
                eventEntity = await _context.Events
                    .Include(e => e.Records)
                        .ThenInclude(r => r.Client)
                        .ThenInclude(c => c.ClientTags)
                    .Include(e => e.Staff)
                    .Include(e => e.Service)
                        .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventEntity != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(cacheKey, eventEntity, cacheEntryOptions);
                }
            }

            return eventEntity;
        }

        public async Task<List<Event>> GetByIdsAsync(List<int> ids)
        {
            var events = new List<Event>();
            var idsToFetch = new List<int>();

            foreach (var id in ids)
            {
                string cacheKey = $"Event_{id}";
                if (_cache.TryGetValue(cacheKey, out Event cachedEvent))
                {
                    events.Add(cachedEvent);
                }
                else
                {
                    idsToFetch.Add(id);
                }
            }

            if (idsToFetch.Any())
            {
                var fetchedEvents = await _context.Events
                    .Include(e => e.Records)
                        .ThenInclude(r => r.Client)
                        .ThenInclude(c => c.ClientTags)
                    .Include(e => e.Staff)
                    .Include(e => e.Service)
                        .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .Where(e => idsToFetch.Contains(e.Id))
                    .ToListAsync();

                foreach (var ev in fetchedEvents)
                {
                    string cacheKey = $"Event_{ev.Id}";
                    _cache.Set(cacheKey, ev, new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5)));
                }

                events.AddRange(fetchedEvents);
            }

            return events;
        }
    }
}
