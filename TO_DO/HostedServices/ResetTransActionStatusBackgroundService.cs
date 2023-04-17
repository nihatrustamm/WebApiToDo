using Microsoft.EntityFrameworkCore;
using TO_DO.Data;

namespace TO_DO.HostedServices
{
    public class ResetTransActionStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ResetTransActionStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
                var pendingTransActions = await dbContext.Transactions
                    .Where(t => t.Status == TransactionStatus.Processing)
                    .ToListAsync();
                foreach (var transaction in pendingTransActions)
                {
                    transaction.Status = TransactionStatus.Created;
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
