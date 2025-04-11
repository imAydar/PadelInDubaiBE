using PadelInDubai.DAL.Entities;
using System.Threading;
using Telegram.Bot.Polling;

namespace PadelInDubai.HostedServices
{
    public class TelegramBotHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope _botScope;

        public TelegramBotHostedService(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _botScope = _scopeFactory.CreateScope();
            var botUpdateService = _botScope.ServiceProvider.GetRequiredService<IMessagesHandler>();

            Task.Run(() => botUpdateService.Start(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _botScope.Dispose();
            return null;
        }
    }
}
