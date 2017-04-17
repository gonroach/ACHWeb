using Aplicacion.Process;
using WindowsService_HostAPI.Contract;
using Dominio.Core;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService_HostAPI.Service
{
    public class SchedulerService : ISchedulerService
    {
        #region Miembros

        /// <summary>
        /// 
        /// </summary>
        private IScheduler scheduler;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public SchedulerService(IScheduler _scheduler)
        {
            this.scheduler = _scheduler;
        }

        #endregion

        /// <summary>
        /// Método que inicializa los schedulers en el caso de alguna configuración en el modulo General Settings
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        public void GeneralSettingsStart(GeneralSettingsConfigurationScheduler configuration, int countryId, int currencyId)
        {
            PostACHConfigurationScheduler postACHConfiguration = new PostACHConfigurationScheduler();
            SettlementConfigurationScheduler settlementConfiguration = new SettlementConfigurationScheduler();
            ClosingProcessesConfigurationScheduler closingProcessesConfiguration = new ClosingProcessesConfigurationScheduler();

            postACHConfiguration.PostACHConfiguration = configuration.PostACHConfiguration;
            postACHConfiguration.GeneralSettingConfiguration = configuration.GeneralSettingConfiguration;

            settlementConfiguration.GeneralSettingConfiguration = configuration.GeneralSettingConfiguration;
            settlementConfiguration.SettlementConfiguration = configuration.SettlementConfiguration;

            closingProcessesConfiguration.GeneralSettingConfiguration = configuration.GeneralSettingConfiguration;
            closingProcessesConfiguration.ClosingProcessesConfiguration = configuration.ClosingProcessesConfiguration;

            if (postACHConfiguration.PostACHConfiguration != null)
                this.PostACHStart(postACHConfiguration, countryId, currencyId);

            if (settlementConfiguration.SettlementConfiguration.Any())
                this.SettlementStart(settlementConfiguration, countryId, currencyId);

            if (closingProcessesConfiguration.ClosingProcessesConfiguration != null)
                this.ClosingProcessesStart(closingProcessesConfiguration, countryId, currencyId);
        }

        /// <summary>
        /// Método que inicializa la configuración del scheduler en el caso del proceso Post ACH
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        public void PostACHStart(PostACHConfigurationScheduler configuration, int countryId, int currencyId)
        {
            string countryName = configuration.PostACHConfiguration.Country.Name.Trim();

            string group = string.Format("{0}_{1}_postACHGroup", countryName, currencyId);
            string[] groupsToDelete = new string[] { group };

            if (RemoveJobsByGroup(groupsToDelete))
            {
                if (configuration.PostACHConfiguration.ExecutionModeId == Utilitarios.Base.ExecutionMode.Automatic)
                {
                    string daysToExecute = GenerateDaysOfWeek(configuration.GeneralSettingConfiguration);
                    TimeSpan startTime = (TimeSpan)configuration.PostACHConfiguration.StartTime;
                    TimeSpan finishTime = (TimeSpan)configuration.PostACHConfiguration.FinishTime;
                    TimeSpan importTimer = (TimeSpan)configuration.PostACHConfiguration.ImportTimer;

                    int minutesStartTime = startTime.Minutes;
                    int hoursStartTime = startTime.Hours;

                    int minutesFinishTime = finishTime.Minutes;
                    int hoursFinishTime = finishTime.Hours;

                    int cronMinutes = default(int);
                    int cronHours = default(int);

                    if (importTimer.Minutes < 60)
                    {
                        cronMinutes = importTimer.Minutes;
                        cronHours = 0;
                    }
                    else
                    {
                        cronMinutes = 0;
                        cronHours = importTimer.Minutes / 60;
                    }                    
                    
                    DateTime horaBase = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startTime.Hours, startTime.Minutes, 0);
                    DateTime horaTermino = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, finishTime.Hours, finishTime.Minutes, 0);                    

                    var id = Guid.NewGuid();

                    IJobDetail job = JobBuilder.Create<PostACHTask>()
                        .StoreDurably(true)
                        .WithIdentity(string.Format("job_PA_{0}_{1}_{2}", countryName, currencyId, id), group)
                        .UsingJobData("countryId", countryId.ToString())
                        .UsingJobData("currencyId", currencyId.ToString())
                        .Build();

                    this.scheduler.AddJob(job, true);

                    do
                    {                        
                        string cron = string.Format("0 {0} {1} ? * {2}", horaBase.Minute, horaBase.Hour, daysToExecute);

                        ITrigger trigger = TriggerBuilder.Create()
                                                       .WithIdentity(string.Format("tgr_PA_{0}_{1}_{2}", countryName, currencyId, id), group)
                                                       .ForJob(job)
                                                       .WithSchedule(CronScheduleBuilder.CronSchedule(cron)
                                                                                       .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(configuration.GeneralSettingConfiguration.TimeZone)))
                                                       .Build();

                        this.scheduler.ScheduleJob(trigger);

                        horaBase = horaBase.AddMinutes(importTimer.Minutes);
                        id = Guid.NewGuid();

                    } while (horaBase <= horaTermino);

                    this.scheduler.Start();
                }
            }
        }

        /// <summary>
        /// Método que inicializa la configuración del scheduler en el caso del proceso Settlement
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        public void SettlementStart(SettlementConfigurationScheduler configuration, int countryId, int currencyId)
        {
            string countryName = configuration.SettlementConfiguration.FirstOrDefault().TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.Country.Name.Trim();
            string group = string.Format("{0}_{1}_settlementGroup", countryName, currencyId);
            string[] groupsToDelete = new string[] { group };

            if (RemoveJobsByGroup(groupsToDelete))
            {
                if (configuration.SettlementConfiguration.Any())
                {
                    if (configuration.SettlementConfiguration.Any(c => c.ExecutionModeId == Utilitarios.Base.ExecutionMode.Automatic))
                    {
                        string daysToExecute = GenerateDaysOfWeek(configuration.GeneralSettingConfiguration);
                        TimeSpan morningSettlement;
                        TimeSpan? afternoonSettlement;

                        foreach (var item in configuration.SettlementConfiguration.Where(p => p.ExecutionModeId == Utilitarios.Base.ExecutionMode.Automatic))
                        {
                            if (item.MorningSettlement.HasValue)
                            {
                                morningSettlement = (TimeSpan)item.MorningSettlement;
                                afternoonSettlement = (TimeSpan?)item.AfternoonSettlement;

                                int morningSecondsToExecute = morningSettlement.Seconds;
                                int morningMinutesToExecute = morningSettlement.Minutes;
                                int morningHoursToExecute = morningSettlement.Hours;

                                string morningCronExpression = string.Format("{0} {1} {2} {3} {4} {5}", morningSecondsToExecute.ToString(),
                                                                                            morningMinutesToExecute.ToString(),
                                                                                            morningHoursToExecute.ToString(),
                                                                                            "?",
                                                                                            "*",
                                                                                            daysToExecute);



                                var id = Guid.NewGuid();

                                IJobDetail jobMorningSettlement = JobBuilder.Create<SettlementTask>()
                                    .StoreDurably(true)
                                    .WithIdentity(string.Format("job_ST_{0}_{1}_{2}_{3}_{4}", countryName, id, item.Description, item.MorningText, currencyId), group)
                                    .UsingJobData("countryId", countryId.ToString())
                                    .UsingJobData("currencyId", currencyId.ToString())
                                    .UsingJobData("transactionTypeConfigurationCountryCurrencyId", item.TransactionTypeConfigurationCountryCurrencyId.ToString())
                                    .Build();

                                ITrigger morningTrigger = TriggerBuilder.Create()
                                    .WithIdentity(string.Format("tgr_ST_AM_{0}_{1}_{2}_{3}_{4}", countryName, id, item.Description, item.MorningText, currencyId), group)
                                    .ForJob(jobMorningSettlement)
                                    .WithSchedule(CronScheduleBuilder.CronSchedule(morningCronExpression)
                                                                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(configuration.GeneralSettingConfiguration.TimeZone)))
                                    .Build();

                                this.scheduler.ScheduleJob(jobMorningSettlement, morningTrigger);

                                if (afternoonSettlement.HasValue)
                                {
                                    int afternoonSecondsToExecute = afternoonSettlement.Value.Seconds;
                                    int afternoonMinutesToExecute = afternoonSettlement.Value.Minutes;
                                    int afternoonHoursToExecute = afternoonSettlement.Value.Hours;

                                    string afternoonCronExpression = string.Format("{0} {1} {2} {3} {4} {5}", afternoonSecondsToExecute.ToString(),
                                                                                                afternoonMinutesToExecute.ToString(),
                                                                                                afternoonHoursToExecute.ToString(),
                                                                                                "?",
                                                                                                "*",
                                                                                                daysToExecute);


                                    IJobDetail jobAfternoonSettlement = JobBuilder.Create<SettlementTask>()
                                       .StoreDurably(true)
                                       .WithIdentity(string.Format("job_ST_{0}_{1}_{2}_{3}_{4}", countryName, id, item.Description, item.AfternoonText, currencyId), group)
                                       .UsingJobData("countryId", countryId.ToString())
                                       .UsingJobData("currencyId", currencyId.ToString())
                                       .UsingJobData("transactionTypeConfigurationCountryCurrencyId", item.TransactionTypeConfigurationCountryCurrencyId.ToString())
                                       .Build();

                                    ITrigger afternoonTrigger = TriggerBuilder.Create()
                                        .WithIdentity(string.Format("tgr_ST_PM_{0}_{1}_{2}_{3}_{4}", countryName, id, item.Description, item.AfternoonText, currencyId), group)
                                        .ForJob(jobAfternoonSettlement)
                                        .WithSchedule(CronScheduleBuilder.CronSchedule(afternoonCronExpression)
                                                                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(configuration.GeneralSettingConfiguration.TimeZone)))
                                        .Build();


                                    this.scheduler.ScheduleJob(jobAfternoonSettlement, afternoonTrigger);
                                }

                                this.scheduler.Start();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Método que inicializa la configuración del scheduler en el caso del proceso Close Batch
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        public void ClosingProcessesStart(ClosingProcessesConfigurationScheduler configuration, int countryId, int currencyId)
        {
            string countryName = configuration.ClosingProcessesConfiguration.Country.Name.Trim();
            string group = string.Format("{0}_{1}_closingProcessesGroup", countryName, currencyId);
            string[] groupsToDelete = new string[] { group };

            if (RemoveJobsByGroup(groupsToDelete))
            {
                if (configuration.ClosingProcessesConfiguration.ExecutionModeId == Utilitarios.Base.ExecutionMode.Automatic)
                {
                    string daysToExecute = GenerateDaysOfWeek(configuration.GeneralSettingConfiguration);
                    TimeSpan timeToExecute = (TimeSpan)configuration.ClosingProcessesConfiguration.CloseBatchTime;
                    int secondsToExecute = timeToExecute.Seconds;
                    int minutesToExecute = timeToExecute.Minutes;
                    int hoursToExecute = timeToExecute.Hours;
                    string cronExpression = string.Format("{0} {1} {2} {3} {4} {5}", secondsToExecute.ToString(),
                                                                                    minutesToExecute.ToString(),
                                                                                    hoursToExecute.ToString(),
                                                                                    "?",
                                                                                    "*",
                                                                                    daysToExecute);

                    //string cronExpression = "0 * 8-19 * * ?";
                    var id = Guid.NewGuid();                    

                    IJobDetail job = JobBuilder.Create<ClosingProcessesTask>()
                        .StoreDurably(true)
                        .WithIdentity(string.Format("job_CP_{0}_{1}_{2}", countryName, currencyId, id), group)
                        .UsingJobData("countryId", countryId.ToString())
                        .UsingJobData("currencyId", currencyId.ToString())                        
                        .Build();

                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(string.Format("tgr_CP_{0}_{1}_{2}", countryName, currencyId, id), group)                        
                        .ForJob(job)
                        .WithSchedule(CronScheduleBuilder.CronSchedule(cronExpression)
                                                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(configuration.GeneralSettingConfiguration.TimeZone)))
                        .Build();

                    this.scheduler.ScheduleJob(job, trigger);
                    this.scheduler.Start();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="generalSettings"></param>
        /// <returns></returns>
        private static string GenerateDaysOfWeek(SchedulerGeneralSettingConfiguration generalSettings)
        {
            List<string> daysOfTheWeek = new List<string>();

            if (generalSettings.InMonday == true) daysOfTheWeek.Add("MON");
            if (generalSettings.InTuesday == true) daysOfTheWeek.Add("TUE");
            if (generalSettings.InWednesday == true) daysOfTheWeek.Add("WED");
            if (generalSettings.InThursday == true) daysOfTheWeek.Add("THU");
            if (generalSettings.InFriday == true) daysOfTheWeek.Add("FRI");
            if (generalSettings.InSaturday == true) daysOfTheWeek.Add("SAT");
            if (generalSettings.InSunday == true) daysOfTheWeek.Add("SUN");

            return string.Join(",", daysOfTheWeek.ToArray());
        }

        /// <summary>
        /// Método que remueve los jobs por grupo
        /// </summary>
        /// <param name="groupsToRemove"></param>
        private bool RemoveJobsByGroup(string[] groupsToRemove)
        {            
            IList<string> jobGroups = scheduler.GetJobGroupNames();
            bool jobsDeleted = false;
            List<JobKey> listaTrabajos = new List<JobKey>();

            foreach (string group in groupsToRemove)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = this.scheduler.GetJobKeys(groupMatcher);

                foreach (JobKey item in jobKeys)
                {
                    if (scheduler.CheckExists(item))
                    {
                        listaTrabajos.Add(item);
                    }
                }
            }

            if (listaTrabajos.Any())
            {
                jobsDeleted = this.scheduler.DeleteJobs(listaTrabajos);
            }
            else
            {
                jobsDeleted = true;
            }

            return jobsDeleted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupsToDisable"></param>
        /// <returns></returns>
        private bool DisableJobsByGroup(string[] groupsToDisable)
        {            
            IList<string> jobGroups = this.scheduler.GetJobGroupNames();
            bool jobsDeleted = false;
            List<TriggerKey> listaTriggers = new List<TriggerKey>();

            foreach (string group in groupsToDisable)
            {
                var groupMatcher = GroupMatcher<TriggerKey>.GroupContains(group);
                var triggerKeys = this.scheduler.GetTriggerKeys(groupMatcher);

                foreach (TriggerKey item in triggerKeys)
                {
                    if (this.scheduler.CheckExists(item))
                    {
                        listaTriggers.Add(item);
                    }
                }
            }

            if (listaTriggers.Any())
            {


                jobsDeleted = this.scheduler.UnscheduleJobs(listaTriggers);
            }
            else
            {
                jobsDeleted = true;
            }

            return jobsDeleted;
        }

        #region IDisposable Implementation

        /// <summary>
        /// 
        /// </summary>
        ~SchedulerService() 
        {
            Dispose(false);
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                SchedulerDispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SchedulerDispose() 
        {
            if (this.scheduler != null) 
            {
                if (this.scheduler is IDisposable) 
                {
                    ((IDisposable)this.scheduler).Dispose();
                }

                this.scheduler = null;
            }
        }

        #endregion
    }
}
