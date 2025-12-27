namespace Audit.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    /// <summary>
    /// Executes a placeholder loop until the worker receives a cancellation token.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop processing.</param>
    /// <returns>Task representing the background execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
