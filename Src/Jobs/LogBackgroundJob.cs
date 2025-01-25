using Mercury.Util;
using Quartz;

namespace Mercury.Jobs;

public class LogBackgroundJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        LogService.Get()?.Info($"Logging job executed on {DateTime.UtcNow}");

        return Task.CompletedTask;
    }
}
