using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.BackgroundServices
{
    public class EventSyncService(
        IScheduledApiService scheduledApiService,
        ILogger<EventSyncService> logger) : BackgroundService
    {
        private readonly IScheduledApiService _scheduledApiService = scheduledApiService;
        private readonly ILogger<EventSyncService> _logger = logger;
        private static readonly TimeZoneInfo DubaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(
                _scheduledApiService.CallApiAtRecurringTimeAsync(
                    "/Event/Sync",
                    GetNext10AM,
                    stoppingToken),

                _scheduledApiService.CallApiAtRecurringTimeAsync(
                    "/Event/SyncPastEvents",
                    GetNext11PM,
                    stoppingToken)
            );
        }

        private static DateTime GetNext10AM(DateTime fromDate)
        {
            var dubaiTime = TimeZoneInfo.ConvertTime(fromDate, DubaiTimeZone);
            var next10AM = dubaiTime.Date.AddHours(10);
            if (dubaiTime.TimeOfDay >= new TimeSpan(10, 0, 0))
            {
                next10AM = next10AM.AddDays(1);
            }
            return TimeZoneInfo.ConvertTime(next10AM, DubaiTimeZone, TimeZoneInfo.Utc);
        }

        private static DateTime GetNextMonday(DateTime fromDate)
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && fromDate.TimeOfDay > TimeSpan.Zero)
            {
                daysUntilMonday = 7;
            }

            return fromDate.Date
                .AddDays(daysUntilMonday);
        }

        private static DateTime GetNext11PM(DateTime fromDate)
        {
            var dubaiTime = TimeZoneInfo.ConvertTime(fromDate, DubaiTimeZone);
            var next11PM = dubaiTime.Date.AddHours(23);
            if (dubaiTime.TimeOfDay >= new TimeSpan(23, 0, 0))
            {
                next11PM = next11PM.AddDays(1);
            }
            return TimeZoneInfo.ConvertTime(next11PM, DubaiTimeZone, TimeZoneInfo.Utc);
        }
    }
}