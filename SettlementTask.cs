using Aplicacion.Process;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quartz;
using System;
using System.Diagnostics;
using Utilitarios.CustomDatabaseTraceListener;
using Utilitarios.Excepciones;

namespace WindowsService_HostAPI
{
    [DisallowConcurrentExecutionAttribute]
    public class SettlementTask : IJob, IDisposable
    {
        #region Miembros

        /// <summary>
        /// 
        /// </summary>
        private SettlementServicioScheduler settlementServicio;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_processSchedulerServicio"></param>
        public SettlementTask()
        {
            this.settlementServicio = new SettlementServicioScheduler();
        }

        #endregion

        #region Métodos Base

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);

            using (traceManager.StartTrace("Scheduler Execution Error", newGuid))
            {
                try
                {
                    this.InsertarCustomLogEntryScheduler(logWritter, "Inicio Proceso Scheduler Settlement", TraceEventType.Information, "Scheduler", "Settlement Scheduler Information");

                    JobDataMap dataMap = context.JobDetail.JobDataMap;
                    var countryId = int.Parse(dataMap.GetString("countryId"));
                    var currencyId = int.Parse(dataMap.GetString("currencyId"));
                    var transactionTypeConfigurationCountryCurrencyId = int.Parse(dataMap.GetString("transactionTypeConfigurationCountryCurrencyId"));

                    this.settlementServicio.ProcesoSchedulerSettlementInit(countryId, currencyId, "Scheduler", transactionTypeConfigurationCountryCurrencyId);

                    this.InsertarCustomLogEntryScheduler(logWritter, "Fin Proceso Scheduler Settlement", TraceEventType.Information, "Scheduler", "Settlement Scheduler Information");
                }
                catch (JobExecutionException jEx)
                {
                    ProveedorExcepciones.ManejaExcepcion(jEx, "PoliticaSchedulerExecutionError");
                }
                catch (Exception ex)
                {
                    ProveedorExcepciones.ManejaExcepcion(ex, "PoliticaSchedulerExecutionError");
                }
            }
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="traceEventType"></param>
        /// <param name="customData"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        private void InsertarCustomLogEntryScheduler(LogWriter logWriter, string message, TraceEventType traceEventType, string customData, string title)
        {
            var customLogEntry = new CustomLogEntry()
            {
                Categories = new string[] { "Scheduler Execution Error" },
                Message = message,
                Severity = traceEventType,
                CustomData = customData,
                Title = title
            };

            logWriter.Write(customLogEntry);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// 
        /// </summary>
        ~SettlementTask() 
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
                SettlementDispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SettlementDispose() 
        {
            if (this.settlementServicio != null) 
            {
                if (this.settlementServicio is IDisposable) 
                {
                    ((IDisposable)this.settlementServicio).Dispose();
                }

                this.settlementServicio = null;
            }           
        }

        #endregion
    }
}