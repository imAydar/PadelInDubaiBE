namespace PadelInDubai.Services.Interfaces
{
    public interface IScheduledApiService
    {
        //Task CallApiAtTimeAsync(
        //    string endpoint,
        //    DateTime scheduledTime,
        //    CancellationToken cancellationToken = default);

        Task CallApiAtRecurringTimeAsync(
            string endpoint,
            Func<DateTime, DateTime> getNextRunTime,
            CancellationToken cancellationToken = default);
    }
}
