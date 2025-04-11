namespace PadelInDubai.HostedServices
{
    public interface IMessagesHandler
    {
        Task Start(CancellationToken cancellationToken);
    }
}
