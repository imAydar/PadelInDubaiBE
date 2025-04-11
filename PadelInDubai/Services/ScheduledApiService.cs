using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Services
{
    public class ScheduledApiService : IScheduledApiService
    {
        private readonly ILogger<ScheduledApiService> _logger;
        private readonly HttpClient _httpClient;

        public ScheduledApiService(
            ILogger<ScheduledApiService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            var baseUrl = configuration["BaseUrl"] ?? "http://localhost:5152";
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task CallApiAtRecurringTimeAsync(
                    string endpoint,
                    Func<DateTime, DateTime> getNextRunTime,
                    CancellationToken cancellationToken = default)
        {
            DateTime nextRun = getNextRunTime(DateTime.UtcNow);

            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                while (nextRun <= now)
                {
                    nextRun = getNextRunTime(nextRun);
                }

                var delay = nextRun - now;
                _logger.LogInformation($"Next API call to {endpoint} scheduled for: {nextRun:yyyy-MM-dd HH:mm:ss} UTC");

                try
                {
                    await Task.Delay(delay, cancellationToken);

                    var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    _logger.LogInformation($"Successfully called {endpoint} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed calling {endpoint}");
                }

                nextRun = getNextRunTime(nextRun);
            }
        }
    }
}
