
using backend_server_mvc.Data;

namespace backend_server_mvc.Service
{

    public class DatabaseCleanupService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseCleanupService> _logger;

        public DatabaseCleanupService(IServiceProvider serviceProvider, ILogger<DatabaseCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timer = new Timer((state) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    CleanupDatabase(dbContext);
                }
            },
            null,
            Timeout.InfiniteTimeSpan, 
            Timeout.InfiniteTimeSpan
            );
        }

        private void CleanupDatabase(AppDbContext dbContext)
        {
            var expiredSessions = dbContext.DeviceSessions.Where(s => s.IssuedOn.AddSeconds(s.TTL) < DateTime.Now);
            int count = expiredSessions.Count();
            if(count > 0)
            {
                foreach(var session in expiredSessions)
                {
                   //remove channels;
                    var channel = dbContext.ChannelHeaders.Where(c => c.DeviceSession.Token == session.Token).FirstOrDefault();
                    if (channel != null) dbContext.ChannelHeaders.Remove(channel);
                }
                dbContext.DeviceSessions.RemoveRange(expiredSessions);
                dbContext.SaveChanges();
                _logger.LogInformation("Removed {count} expired records", count);
            }
            
            
        }

        //interaface impl
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }
    }
}
