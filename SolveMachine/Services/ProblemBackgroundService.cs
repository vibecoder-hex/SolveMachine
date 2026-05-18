using SolveMachine.Repositories;
using SolveMachine.Models;

namespace SolveMachine.Services
{
    public class TimeMachineBackgroundService : BackgroundService
    {
        private readonly ILogger<TimeMachineBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TimeMachineBackgroundService(ILogger<TimeMachineBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var selectionRepository = scope.ServiceProvider.GetRequiredService<ISelectionProblemRepository>();
                    var modificationRepository = scope.ServiceProvider.GetRequiredService<IModificationProblemRepository>();

                    var expriredProblemsResult = await selectionRepository.GetExpiredProblemsForAllUsers();
                    if (!expriredProblemsResult.IsSuccess)
                        _logger.LogError($"Problem handling error by {expriredProblemsResult.ErrorMessage}");

                    List<Problem> expiredProblems = expriredProblemsResult.Problems;
                    foreach (var problem in expiredProblems)
                    {
                        if (DateOnly.FromDateTime(DateTime.UtcNow) > problem.DeadlineDate)
                        {
                            _logger.LogInformation($"Problem by id {problem.Id} has been completed");
                            await modificationRepository.SetProblemAsCompleted(problem.Id);
                        }
                    }

                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
