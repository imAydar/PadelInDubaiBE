using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PadelInDubai.DAL.Entities;
using PadelInDubai.Extensions;
using PadelInDubai.Mappings;
using PadelInDubai.Services;

namespace PadelInDubai.DAL
{
    public class BookingRepository(ApplicationDbContext context, TelegramService tgService, 
        IEventRepository eventRepository, IExternalEventService client, IMemoryCache cache) : IBookingRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly TelegramService _tgService = tgService;
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IExternalEventService _client = client;
        private readonly IMemoryCache _cache = cache;

        public async Task<Record> Create(Record record)
        {
            /*_context.Records.Add(entity);
            await _context.SaveChangesAsync();*/
            var entity = await CreateOrUpdateAsync(record);

            var evt = await _eventRepository.GetByIdAsync(record.EventId);
            if (evt == null)
            {
                var eventDto = await _client.GetEvent(record.EventId);
                if (eventDto != null)
                {
                    await _eventRepository.UpsertEventsAsync([eventDto.ToEntity()]);
                    await _tgService.SendEventMessageAsync(eventDto.ToEntity().ToDto());
                }
                //_tgService.CreateTelegramMessage(evt.ToDto());
            }
            else
            {
                if (evt.MessageId == null)
                {
                    await _tgService.SendEventMessageAsync(evt.ToDto());
                }
                else
                {
                    await _tgService.UpdateEventMessageAsync(evt.ToDto());
                }
            }

            _cache.Clear();
            return entity;
        }

        public async Task<Record> Update(Record record)
        {
            var entity = await _context.Records.FindAsync(record.Id);
            if (entity == null)
            {
                entity = record;
                _context.Records.Add(entity);
            }
            else
            {
                //TODO: Check!
                entity = record;
            }
            
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<Record> Delete(Record record)
        {
            var entity = await _context.Records.FindAsync(record.Id);
            if (entity == null)
            {
                return null;
            }
            _context.Records.Remove(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        private async Task<Record> CreateOrUpdateAsync(Record record)
        {
            var existingRecord = await _context.Records
                .Include(r => r.Services)
                .Include(r => r.Client)
                .ThenInclude(c => c.ClientTags)
                .Include(r => r.Staff)
                .FirstOrDefaultAsync(r => r.Id == record.Id);

            // Update scenario
            if (existingRecord != null)
            {
                existingRecord.Status = record.Status;
                existingRecord.CompanyId = record.CompanyId;
                existingRecord.StaffId = record.StaffId;
                existingRecord.ClientsCount = record.ClientsCount;
                existingRecord.Date = record.Date;
                existingRecord.DateTime = record.DateTime;
                existingRecord.CreateDate = record.CreateDate;
                existingRecord.Comment = record.Comment;
                existingRecord.Online = record.Online;
                existingRecord.Confirmed = record.Confirmed;
                existingRecord.Notified = record.Notified;
                existingRecord.FromUrl = record.FromUrl;
                existingRecord.VisitId = record.VisitId;
                existingRecord.CreatedUserId = record.CreatedUserId;
                existingRecord.Deleted = record.Deleted;
                existingRecord.PaidFull = record.PaidFull;
                existingRecord.Prepaid = record.Prepaid;
                existingRecord.PrepaidConfirmed = record.PrepaidConfirmed;
                existingRecord.LastChangeDate = record.LastChangeDate;
                existingRecord.EventId = record.EventId;

                // Update Services
                if (record.Services != null)
                {
                    var serviceIds = record.Services.Select(s => s.Id).ToList();
                    var existingServices = await _context.Services
                        .Where(s => serviceIds.Contains(s.Id))
                        .ToListAsync();

                    existingRecord.Services.Clear();
                    foreach (var service in record.Services)
                    {
                        var existing = existingServices.FirstOrDefault(s => s.Id == service.Id);
                        existingRecord.Services.Add(existing ?? service);

                        if (existing == null)
                        {
                            service.Category = null;
                            service.CategoryId = null;
                            _context.Services.Add(service);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return existingRecord;
            }

            // Create scenario (your original logic)
            if (record.Staff != null)
            {
                var existingStaff = await _context.Staffs.FindAsync(record.Staff.Id);
                record.Staff = existingStaff ?? record.Staff;
                if (existingStaff == null)
                    _context.Staffs.Add(record.Staff);
            }

            if (record.Client != null)
            {
                var existingClient = await _context.Clients
                    .Include(c => c.ClientTags)
                    .FirstOrDefaultAsync(c => c.Id == record.Client.Id);

                if (existingClient != null)
                {
                    record.Client = existingClient;
                }
                else
                {
                    if (record.Client.ClientTags?.Count > 0)
                    {
                        foreach (var tag in record.Client.ClientTags)
                        {
                            var exists = await _context.ClientTags.AnyAsync(t => t.Id == tag.Id);
                            if (!exists)
                                _context.ClientTags.Add(tag);
                        }
                    }
                    _context.Clients.Add(record.Client);
                }
            }

            if (record.Services != null && record.Services.Any())
            {
                var serviceIds = record.Services.Select(s => s.Id).ToList();
                var existingServices = await _context.Services
                    .Where(s => serviceIds.Contains(s.Id))
                    .ToListAsync();

                var finalServices = new List<PadelInDubai.DAL.Entities.Service>();
                foreach (var service in record.Services)
                {
                    var existing = existingServices.FirstOrDefault(s => s.Id == service.Id);
                    finalServices.Add(existing ?? service);

                    if (existing == null)
                    {
                        service.Category = null;
                        service.CategoryId = null;
                        _context.Services.Add(service);
                    }
                }

                record.Services = finalServices;
            }

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            _cache.Clear();
            return record;
        }
        public async Task<List<Record>> SaveRecordsAsync(List<Record> recordGot)
        {
            if (recordGot == null || recordGot.Count == 0)
                return null;

            // Preload existing IDs for related entities and records.
            var staffIds = await _context.Staffs.Select(s => s.Id).ToHashSetAsync();
            var clientIds = await _context.Clients.Select(c => c.Id).ToHashSetAsync();
            var serviceIds = await _context.Services.Select(s => s.Id).ToHashSetAsync();
            // Existing record IDs currently in the database.
            var existingRecordIds = await _context.Records.Select(r => r.Id).ToHashSetAsync();

            // Lists for new related entities and for new records.
            var newStaff = new List<Staff>();
            var newClients = new List<Client>();
            var newServices = new List<Service>();
            var recordsToInsert = new List<Record>();

            // Use negative numbers for new Client IDs if they're missing.
            int leastId = clientIds.Where(c => c < 0).DefaultIfEmpty(0).Min();

            // Process each incoming record.
            foreach (var record in recordGot)
            {
                // If no ClientId is set, assign a new negative one.
                if (record.ClientId == 0)
                {
                    leastId--;
                    record.ClientId = leastId;
                    if (record.Client != null)
                        record.Client.Id = leastId;
                }

                // Queue new Staff if not already in DB.
                if (record.Staff != null && !staffIds.Contains(record.Staff.Id))
                {
                    newStaff.Add(record.Staff);
                    staffIds.Add(record.Staff.Id);
                }

                // Queue new Client if not already in DB.
                if (record.Client != null)
                {
                    if (clientIds.Contains(record.Client.Id))
                    {
                        // The client exists—don't update it directly, just keep the reference
                        // This avoids the duplicate tracking issue
                        var existingClient = await _context.Clients.FindAsync(record.Client.Id);
                        if (existingClient != null)
                        {
                            // Update properties of the existing client
                            _context.Entry(existingClient).CurrentValues.SetValues(record.Client);
                        }
                    }
                    else
                    {
                        // New client—add it.
                        newClients.Add(record.Client);
                        clientIds.Add(record.Client.Id);
                    }
                }

                // Queue new Services if needed.
                foreach (var service in record.Services ?? new List<Service>())
                    {
                        if (!serviceIds.Contains(service.Id))
                        {
                            newServices.Add(service);
                            serviceIds.Add(service.Id);
                        }
                    }

                // Detach navigation properties to prevent duplicate tracking.
                // We assume that the FK properties (e.g. ClientId, StaffId) are already set.
                record.Staff = null;
                record.Client = null;
                record.Services = null;

                // If the record exists in the DB (and its Id is not zero), update it.
                if (record.Id != 0 && existingRecordIds.Contains(record.Id))
                {
                    _context.Records.Update(record);
                }
                else
                {
                    // Otherwise, it is new.
                    recordsToInsert.Add(record);
                }
            }

            // Insert any new related entities first.
            if (newStaff.Any())
                _context.Staffs.AddRange(newStaff);
            if (newClients.Any())
                _context.Clients.AddRange(newClients);
            if (newServices.Any())
                _context.Services.AddRange(newServices);

            await _context.SaveChangesAsync();

            // Insert new records.
            if (recordsToInsert.Any())
                _context.Records.AddRange(recordsToInsert);

            // Save changes to update existing records and insert new ones.
            await _context.SaveChangesAsync();

            //// At this point, recordGot should reflect both updated and inserted records.
            //// Build a set of the IDs for records we want to keep.
            //var incomingRecordIds = recordGot.Select(r => r.Id).ToHashSet();

            //// Find and remove any records that exist in the database but were not in the incoming list.
            //var recordsToDelete = await _context.Records
            //    .Where(r => !incomingRecordIds.Contains(r.Id))
            //    .ToListAsync();

            //if (recordsToDelete.Any())
            //{
            //    _context.Records.RemoveRange(recordsToDelete);
            //    await _context.SaveChangesAsync();
            //}

            _cache.Clear();
            return recordGot;
        }


        //public async Task<List<Record>> SaveRecordsAsync(List<Record> recordGot)
        //{
        //    if (recordGot == null || recordGot.Count == 0)
        //        return null;

        //    // Preload existing IDs for related entities and records.
        //    var staffIds = await _context.Staffs.Select(s => s.Id).ToHashSetAsync();
        //    var clientIds = await _context.Clients.Select(c => c.Id).ToHashSetAsync();
        //    var serviceIds = await _context.Services.Select(s => s.Id).ToHashSetAsync();
        //    var recordIds = await _context.Records.Select(r => r.Id).ToHashSetAsync();

        //    // Lists to hold new entities and to separate records to insert vs. update.
        //    var newStaff = new List<Staff>();
        //    var newClients = new List<Client>();
        //    var newServices = new List<Service>();
        //    var recordsToInsert = new List<Record>();

        //    // We'll use negative numbers for new Client IDs if they're missing.
        //    int leastId = clientIds.Where(c => c < 0).DefaultIfEmpty(0).Min();

        //    // Loop through each record
        //    foreach (var record in recordGot)
        //    {
        //        // If no client ID is set, assign a new negative one.
        //        if (record.ClientId == 0)
        //        {
        //            leastId--;
        //            record.ClientId = leastId;
        //            if (record.Client != null)
        //                record.Client.Id = leastId;
        //        }

        //        // Queue new Staff if not already in DB.
        //        if (record.Staff != null && !staffIds.Contains(record.Staff.Id))
        //        {
        //            newStaff.Add(record.Staff);
        //            staffIds.Add(record.Staff.Id);
        //        }

        //        // Queue new Client if not already in DB.
        //        if (record.Client != null && !clientIds.Contains(record.Client.Id))
        //        {
        //            newClients.Add(record.Client);
        //            clientIds.Add(record.Client.Id);
        //        }

        //        // Queue new Services if needed.
        //        foreach (var service in record.Services ?? new List<Service>())
        //        {
        //            if (!serviceIds.Contains(service.Id))
        //            {
        //                newServices.Add(service);
        //                serviceIds.Add(service.Id);
        //            }
        //        }

        //        // Detach navigation properties to prevent duplicate tracking.
        //        // We assume that the FK properties (e.g. ClientId, StaffId) are already set.
        //        record.Staff = null;
        //        record.Client = null;
        //        record.Services = null;

        //        // If the record exists, update it.
        //        // (Using Update() is safe if you know that the record exists.)
        //        if (recordIds.Contains(record.Id))
        //        {
        //            // Attach the record and mark it as modified.
        //            _context.Records.Update(record);
        //        }
        //        else
        //        {
        //            // Otherwise, it's a new record.
        //            recordsToInsert.Add(record);
        //        }
        //    }

        //    // Insert any new related entities first.
        //    if (newStaff.Any())
        //        _context.Staffs.AddRange(newStaff);
        //    if (newClients.Any())
        //        _context.Clients.AddRange(newClients);
        //    if (newServices.Any())
        //        _context.Services.AddRange(newServices);

        //    await _context.SaveChangesAsync();

        //    // Add new records.
        //    if (recordsToInsert.Any())
        //        _context.Records.AddRange(recordsToInsert);

        //    // Finally, update existing records and insert new records.
        //    await _context.SaveChangesAsync();

        //    // Optionally, return the original list of records (which now reflects your operations)
        //    return recordGot;
        //}


        //public async Task<List<Record>> SaveRecordsAsync(List<Record> recordGot)
        //{
        //    if (recordGot == null || recordGot.Count == 0)
        //        return null;

        //    // Preload existing IDs
        //    var staffIds = await _context.Staffs.Select(s => s.Id).ToHashSetAsync();
        //    var clientIds = await _context.Clients.Select(c => c.Id).ToHashSetAsync();
        //    var serviceIds = await _context.Services.Select(s => s.Id).ToHashSetAsync();

        //    var newStaff = new List<Staff>();
        //    var newClients = new List<Client>();
        //    var newServices = new List<Service>();
        //    var newRecords = new List<Record>();

        //    int leastId = clientIds.Where(c => c < 0)?.Order().FirstOrDefault() ?? 0;
        //    foreach (var record in recordGot)
        //    {
        //        if (record.ClientId == 0)
        //        {
        //            leastId -= 1;
        //            record.ClientId = leastId;
        //            record.Client.Id = leastId;
        //        }
        //        // Queue new staff if needed
        //        if (record.Staff != null && !staffIds.Contains(record.Staff.Id))
        //        {
        //            newStaff.Add(record.Staff);
        //            staffIds.Add(record.Staff.Id);
        //        }

        //        // Queue new client if needed
        //        if (record.Client != null && !clientIds.Contains(record.Client.Id))
        //        {
        //            newClients.Add(record.Client);
        //            clientIds.Add(record.Client.Id);
        //        }

        //        // Queue any new services
        //        foreach (var service in record.Services ?? new List<Service>())
        //        {
        //            if (!serviceIds.Contains(service.Id))
        //            {
        //                newServices.Add(service);
        //                serviceIds.Add(service.Id);
        //            }
        //        }

        //        // Strip navigation properties to avoid duplicate tracking
        //        record.Staff = null;
        //        record.Client = null;
        //        record.Services = null;

        //        newRecords.Add(record);
        //    }

        //    // Save related entities
        //    if (newStaff.Any()) _context.Staffs.AddRange(newStaff);
        //    if (newClients.Any()) _context.Clients.AddRange(newClients);
        //    if (newServices.Any()) _context.Services.AddRange(newServices);

        //    await _context.SaveChangesAsync();

        //    // Save records last
        //    _context.Records.AddRange(newRecords);
        //    await _context.SaveChangesAsync();
        //    return newRecords;
        //}

        //private async Task<Record> CreateAsync(Record record)
        //{
        //    var exist = _context.Records.Find(record.Id);
        //    if (exist != null)
        //    {
        //        return exist;
        //    }
        //    if (record.Staff != null)
        //    {
        //        var existingStaff = await _context.Staffs.FindAsync(record.Staff.Id);
        //        record.Staff = existingStaff ?? record.Staff;
        //        if (existingStaff == null)
        //            _context.Staffs.Add(record.Staff);
        //    }

        //    // 2. Client
        //    if (record.Client != null)
        //    {
        //        var existingClient = await _context.Clients
        //            .Include(c => c.ClientTags)
        //            .FirstOrDefaultAsync(c => c.Id == record.Client.Id);

        //        if (existingClient != null)
        //        {
        //            record.Client = existingClient;
        //        }
        //        else
        //        {
        //            // Optional: de-dupe client tags before adding
        //            if (record.Client.ClientTags?.Count > 0)
        //            {
        //                foreach (var tag in record.Client.ClientTags)
        //                {
        //                    var exists = await _context.ClientTags.AnyAsync(t => t.Id == tag.Id);
        //                    if (!exists)
        //                        _context.ClientTags.Add(tag);
        //                }
        //            }
        //            _context.Clients.Add(record.Client);
        //        }
        //    }

        //    // 3. Services
        //    if (record.Services != null && record.Services.Any())
        //    {
        //        //var existingCategories = _context.Categories.Where(c => record.Services.Select(s => s.CategoryId).Contains(c.Id));

        //        var serviceIds = record.Services.Select(s => s.Id).ToList();
        //        var existingServices = await _context.Services
        //            .Where(s => serviceIds.Contains(s.Id))
        //            .ToListAsync();

        //        var finalServices = new List<PadelInDubai.DAL.Entities.Service>();
        //        foreach (var service in record.Services)
        //        {
        //            var existing = existingServices.FirstOrDefault(s => s.Id == service.Id);
        //            finalServices.Add(existing ?? service);

        //            if (existing == null)
        //            {
        //                service.Category = null;
        //                service.CategoryId = null;
        //                _context.Services.Add(service);
        //            }
        //        }

        //        record.Services = finalServices;
        //    }

        //    // 4. Finally Add Record
        //    _context.Records.Add(record);
        //    await _context.SaveChangesAsync();

        //    return record;
        //}
    }
}
