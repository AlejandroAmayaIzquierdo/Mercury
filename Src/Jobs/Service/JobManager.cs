using Mercury.Db;
using Mercury.Models.Jobs;
using Mercury.Util;
using Quartz;

namespace Mercury.Jobs;

public class JobManager(MysqlContext dbContext, ISchedulerFactory schedulerFactory)
{
    private readonly MysqlContext _dbContext = dbContext;
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

    public async Task RegisterJobsAsync()
    {
        LogService.Get()?.Info("Registering Jobs from database");

        var scheduler = await _schedulerFactory.GetScheduler();

        var jobs = GetActiveJobs();

        foreach (var job in jobs)
        {
            string jobKey = job.Name;

            if (await scheduler.CheckExists(new JobKey(jobKey)))
            {
                LogService.Get()?.Info($"Job with name {jobKey} already exist");
                continue;
            }

            try
            {
                var quartzJob = JobBuilder
                    .Create(Type.GetType(job.JobType)!)
                    .WithIdentity(jobKey)
                    .Build();

                var jobTrigger = TriggerBuilder
                    .Create()
                    .ForJob(quartzJob)
                    .WithIdentity($"{jobKey}-trigger")
                    .WithCronSchedule(job.Schedule.Trim())
                    .Build();

                await scheduler.ScheduleJob(quartzJob, jobTrigger);

                LogService.Get()?.Info($"Registered job '{jobKey} with cron '{job.Schedule}'");
            }
            catch (Exception ex)
            {
                LogService.Get()?.Error($"Error registering job with key '{jobKey}': {ex.Message}");
            }
        }
    }

    public async Task RefreshJobsAsync()
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        await scheduler.Clear();

        await RegisterJobsAsync();
    }

    private List<Job> GetActiveJobs()
    {
        return [.. _dbContext.Jobs.Where(j => j.Active)];
    }
}
