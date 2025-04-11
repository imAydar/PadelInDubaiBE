using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.BackgroundServices
{
    public class EventSyncService : BackgroundService
    {
        private readonly IScheduledApiService _scheduledApiService;
        private readonly ILogger<EventSyncService> _logger;

        public EventSyncService(
            IScheduledApiService scheduledApiService,
            ILogger<EventSyncService> logger)
        {
            _scheduledApiService = scheduledApiService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(
                _scheduledApiService.CallApiAtRecurringTimeAsync(
                    "/Event/Sync",
                    GetNextSundayAt11PM,
                    stoppingToken),

                _scheduledApiService.CallApiAtRecurringTimeAsync(
                    "/Event/SyncPastEvents",
                    GetNext11PM,
                    stoppingToken)
            );
        }

        private static DateTime GetNextSundayAt11PM(DateTime fromDate)
        {
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)fromDate.DayOfWeek + 7) % 7;

            if (daysUntilSunday == 0 && fromDate.TimeOfDay >= new TimeSpan(23, 0, 0))
            {
                daysUntilSunday = 7;
            }

            return fromDate.Date
                .AddDays(daysUntilSunday)
                .AddHours(23);
        }

        private static DateTime GetNext11PM(DateTime fromDate)
        {
            return fromDate.AddSeconds(5);
            var next11PM = fromDate.Date.AddHours(23);

            if (fromDate.TimeOfDay >= new TimeSpan(23, 0, 0))
            {
                next11PM = next11PM.AddDays(1);
            }

            return next11PM;
        }
    }
}