namespace SolveMachine.Services
{
    public class ProblemBackgroundService : BackgroundService
    {
        private readonly ILogger<ProblemBackgroundService> _logger;

        public ProblemBackgroundService(ILogger<ProblemBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Problem background service is running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
