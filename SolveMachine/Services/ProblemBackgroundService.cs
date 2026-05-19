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

                    var expriredProblemsResult = await selectionRepository.GetCandidatesForComplete();
                    if (expriredProblemsResult.IsSuccess)
                    {
                        List<Problem> expiredProblems = expriredProblemsResult.Problems;
                        _logger.LogInformation(DateOnly.FromDateTime(DateTime.UtcNow).ToString());
                        _logger.LogInformation(expiredProblems.Count.ToString());
                        foreach (var problem in expiredProblems)
                        {
                            _logger.LogInformation($"Problem by id {problem.Id} has deadline date {problem.DeadlineDate}");
                            if (DateOnly.FromDateTime(DateTime.UtcNow) > problem.DeadlineDate)
                            {
                                _logger.LogInformation($"Problem by id {problem.Id} has been completed");
                                await modificationRepository.SetProblemAsCompleted(problem.Id);
                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
