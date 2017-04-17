using Aplicacion.Process;
using Aplicacion.Process.Servicios;
using System;
using System.Linq;
using System.Web.Http;
using WindowsService_HostAPI.Contract;
using WindowsService_HostAPI.HttpAction;

namespace WindowsService_HostAPI
{
    public class SchedulerController : ApiController
    {
        #region Miembros

        /// <summary>
        /// 
        /// </summary>
        private ISchedulerConfigurationServicio schedulerConfigurationServicio;
        private ISchedulerService schedulerService;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fileService"></param>
        public SchedulerController(ISchedulerService _schedulerService)
        {
            this.schedulerConfigurationServicio = new SchedulerConfigurationServicio();
            this.schedulerService = _schedulerService;            
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        [HttpGet]        
        public IHttpActionResult Update(int countryId, int currencyId, string schedulerModule)
        {
            try
            {                            
                switch (schedulerModule)
                {
                    case "generalsettings":
                        UpdateGeneralSettingsScheduler(countryId, currencyId);
                        break;
                    case "postach":
                        UpdatePostACHScheduler(countryId, currencyId);
                        break;
                    case "settlement":
                        UpdateSettlementScheduler(countryId, currencyId);
                        break;
                    case "closingprocesses":
                        UpdateClosingProcessesScheduler(countryId, currencyId);
                        break;
                }                

                return Ok<string>(Boolean.TrueString);
                
            }
            catch(Exception ex)
            {
                return new CustomErrorIHttpActionResult(ex.Message, Request);
            }
        }

        #region Métodos Privados

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        private void UpdateGeneralSettingsScheduler(int countryId, int currencyId)
        {
            var schedulerConfiguration = this.schedulerConfigurationServicio.GetGeneralSettingsConfigurationScheduler(countryId, currencyId);

            this.schedulerService.GeneralSettingsStart(schedulerConfiguration, countryId, currencyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        private void UpdatePostACHScheduler(int countryId, int currencyId)
        {
            var schedulerConfiguration = this.schedulerConfigurationServicio.GetPostACHConfigurationScheduler(countryId, currencyId);

            if (schedulerConfiguration.PostACHConfiguration != null)
                this.schedulerService.PostACHStart(schedulerConfiguration, countryId, currencyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        private void UpdateSettlementScheduler(int countryId, int currencyId)
        {
            var schedulerConfiguration = this.schedulerConfigurationServicio.GetSettlementConfigurationScheduler(countryId, currencyId);

            if (schedulerConfiguration.SettlementConfiguration.Any())
                this.schedulerService.SettlementStart(schedulerConfiguration, countryId, currencyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        private void UpdateClosingProcessesScheduler(int countryId, int currencyId)
        {
            var schedulerConfiguration = this.schedulerConfigurationServicio.GetClosingProcessesConfigurationScheduler(countryId, currencyId);

            if (schedulerConfiguration.ClosingProcessesConfiguration != null)
                this.schedulerService.ClosingProcessesStart(schedulerConfiguration, countryId, currencyId);
        }

        #endregion
    }
}