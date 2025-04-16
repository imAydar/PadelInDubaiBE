using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.BackgroundServices
{
    public class EventSyncService(
        IScheduledApiService scheduledApiService,
        ILogger<EventSyncService> logger) : BackgroundService
    {
        private readonly IScheduledApiService _scheduledApiService = scheduledApiService;
        private readonly ILogger<EventSyncService> _logger = logger;

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
            var next10AM = fromDate.Date.AddHours(10);
            if (fromDate.TimeOfDay >= new TimeSpan(10, 0, 0))
            {
                next10AM = next10AM.AddDays(1);
            }
            return next10AM;
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
            var next11PM = fromDate.Date.AddHours(23);
            if (fromDate.TimeOfDay >= new TimeSpan(23, 0, 0))
            {
                next11PM = next11PM.AddDays(1);
            }

            return next11PM;
        }
    }
}