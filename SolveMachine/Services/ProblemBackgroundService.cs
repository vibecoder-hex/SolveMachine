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

                    List<Problem> expiredProblems = await selectionRepository.GetCandidatesForComplete();
                    DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow); 
                    foreach (var problem in expiredProblems) 
                    { 
                        _logger.LogInformation($"Problem by id {problem.Id} has deadline date {problem.DeadlineDate}"); 
                        if (today > problem.DeadlineDate && problem.Status != ProblemStatus.Completed) 
                        { 
                            _logger.LogInformation($"Problem by id {problem.Id} has been completed"); 
                            await modificationRepository.SetProblemAsCompleted(problem);
                        }
                    } 
                } 
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
