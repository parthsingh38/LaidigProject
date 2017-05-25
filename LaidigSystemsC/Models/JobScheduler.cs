using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
namespace LaidigSystemsC.Models
{
    public class JobScheduler
    {
        public static void Start(string delayedScheduleTime)
        {
            IJobDetail job = JobBuilder.Create<DelayedJob>()
                                  .WithIdentity("job1")
                                  .Build();
            
            ITrigger trigger = TriggerBuilder.Create()
                                            .WithDailyTimeIntervalSchedule
                                              (s =>
                                                 s.WithIntervalInSeconds(30)
                                              .OnEveryDay()
                                              )
                                             .ForJob(job)
                                             .WithIdentity("trigger1")
                                             .StartNow()
                                              .WithCronSchedule(delayedScheduleTime)                                           
                                             .Build();

            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sc = sf.GetScheduler();
            sc.ScheduleJob(job, trigger);
          
            sc.Start();


        }


    }
}
