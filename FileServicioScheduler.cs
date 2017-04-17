using Aplicacion.Base;
using Datos.Persistencia.Core;
using Datos.Persistencia.Messaging;
using Dominio.Core;
using Dominio.Process;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Utilitarios.Base;
using Utilitarios.Excepciones;

namespace Aplicacion.Process
{
    public class FileServicioScheduler
    {
        #region Miembros Privados

        private IMessageQueueFactory messageQueueFactory;

        #endregion

        #region Constructor

        public FileServicioScheduler()
        {
            
        }

        #endregion

        #region Métodos Base

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>             
        public int ObtenerTransactionTypeConfigurationIdByFile(int fileId)
        {
            try
            {
                using (var contexto = new ContextoPrincipal())
                {

                    var fileOriginal = contexto.File.Where(p => p.Id == fileId).Include(new string[] { "TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry" }).FirstOrDefault();

                    return fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfigurationId;
                }
            }
            catch (DbUpdateConcurrencyException dbException)
            {
                throw ProveedorExcepciones.ManejaExcepcion(dbException, "PoliticaDependenciaDatos");
            }
            catch (Exception exception)
            {
                throw ProveedorExcepciones.ManejaExcepcion(exception, "PoliticaBase");
            }

        }

        /// <summary>
        /// Método que obtiene el transactionTypeConfiguration asociado a un archivo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TransactionTypeConfigurationCountryDTO ObtenerTransactionTypeConfigurationCountryFromFile(int? id, string identifier, int countryId)
        {
            Dominio.Core.File fileOriginal = null;
            TransactionTypeConfigurationCountry transactionTypeConfigurationCountry = null;
            IAdaptadorFile adaptadorFile = new AdaptadorFile();

            try
            {
                using (var contexto = new ContextoPrincipal())
                {
                    if (id.HasValue)
                    {
                        fileOriginal = contexto.File.FirstOrDefault(p => p.Id == id.Value);
                        transactionTypeConfigurationCountry = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;
                    }
                    else
                    {
                        transactionTypeConfigurationCountry = contexto.TransactionTypeConfigurationCountry.FirstOrDefault(p => p.CountryId == countryId && p.TransactionTypeConfiguration.TransactionType.Identifier == identifier);
                    }

                    return adaptadorFile.FabricarTransactionTypeConfigurationCountryFromFileDTO(transactionTypeConfigurationCountry);
                }
            }
            catch (DbUpdateConcurrencyException dbException)
            {
                throw ProveedorExcepciones.ManejaExcepcion(dbException, "PoliticaDependenciaDatos");
            }
            catch (Exception exception)
            {
                throw ProveedorExcepciones.ManejaExcepcion(exception, "PoliticaBase");
            }
        }

        #endregion

        #region IFileServicio Implementation

        #region Import

        /// <summary>
        /// Método que verifica todos los estados que debe cumplir para dejar el estado del archivo como Complete
        /// </summary>
        /// <param name="listaFiles"></param>
        /// <param name="paybankReturnFileConfiguration"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public void VerificarFinalStatusFromScheduler(List<int> idsFileList, int countryId, int currencyId)
        {
            using (var contexto = new ContextoPrincipal())
            {
                var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);

                var paybankReturnFile = countryOriginal.PaybankReturnFileConfigurations.FirstOrDefault(p => p.CountryId == countryId && p.CurrencyId == currencyId); ;

                var originalFiles = contexto.File.Where(a => idsFileList.Contains(a.Id)).Include("TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                    .Include("Batchs.Transactions.TransactionEventOnlineLogs.ErrorMessage")
                    .Include("Batchs.Transactions.TransactionTrandeFiles.TrandeFile");

                var listaIds = VerificarFinalStatus(originalFiles, paybankReturnFile, contexto, countryId);

                contexto.SaveChanges();
            }
        }

        /// <summary>
        /// Método que obtiene los archivos existentes dependiendo de la fecha de creación y de la fecha de filtro que se haya seleccionado
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="userSoeid"></param>
        /// <returns></returns>
        public IEnumerable<FileGridBodyDTO> ObtenerArchivosExistentesPorFecha(DateTime fecha, int countryId, int currencyId, int currentCulture, string userSoeid, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();

            using (traceManager.StartTrace("Post Accounting Import Error", newGuid))
            {
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);
                        var currencyOriginal = contexto.Currency.FirstOrDefault(p => p.Id == currencyId);

                        var paybankReturnFile = countryOriginal.PaybankReturnFileConfigurations.FirstOrDefault(p => p.CountryId == countryId && p.CurrencyId == currencyId);

                        var listaOriginal = new List<Dominio.Core.File>();
                        
                        listaOriginal = contexto.File.Where(p => p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryId && DbFunctions.TruncateTime(p.DateSelected) == DbFunctions.TruncateTime(fecha) && p.StateId != FileState.Complete && p.Batchs.Any(c => c.Transactions.Any(d => !d.SettlementSend.HasValue || (d.SettlementSend.HasValue && d.SettlementSend.Value == false)))).Include("TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                                            .Include("Batchs.Transactions.TransactionEventOnlineLogs.ErrorMessage")
                                            .Include("Batchs.Transactions.TransactionTrandeFiles.TrandeFile").ToList();

                        var listaIds = VerificarFinalStatus(listaOriginal, paybankReturnFile, contexto, countryId);

                        if (listaIds.Count() > 0)
                            listaOriginal.RemoveAll(p => listaIds.Contains(p.Id));

                        var originalPaybankConfiguration = contexto.PaybankConfiguration.Where(p => p.PaybankConfigurationCountries.Any(c => c.CountryId == countryId) && p.PaybankConfigurationCurrencies.Any(c => c.CurrencyId == currencyId));
                        var pathPaybankInclearing = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathInclearing;
                        var pathPaybankReturn = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathReturn;

                        var listPath = new List<string> { pathPaybankInclearing, pathPaybankReturn };

                        var iterator = 0;

                        foreach (var item in listPath)
                        {
                            //var rutaFinal = item.Replace("D:", "C:");

                            var pathPaybankProcessed = GetPaybankProcessedFiles(currencyId, iterator, originalPaybankConfiguration);

                            //pathPaybankProcessed = pathPaybankProcessed.Replace("D:", "C:");

                            var pathProcessedFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathPaybankProcessed);

                            var directoryInfoProcessedFile = new DirectoryInfo(pathProcessedFiles);
                            var directoryInfoPaybankFile = new DirectoryInfo(item);

                            var searchPattern = DominioImportLogica.GetPaybankSearchPattern(currencyId, originalPaybankConfiguration, iterator);

                            var listFilesProcessedFiles = directoryInfoProcessedFile.GetFiles("*.*", System.IO.SearchOption.TopDirectoryOnly);
                            var listFilesPaybankFiles = directoryInfoPaybankFile.GetFiles(searchPattern, System.IO.SearchOption.TopDirectoryOnly);

                            var onlyInPaybank = listFilesPaybankFiles.Except(listFilesProcessedFiles, new FileCompare()).Select(p => p);

                            if (onlyInPaybank != null && onlyInPaybank.Count() > 0)
                                ProcesarArchivoPaybankExistencia(countryOriginal, onlyInPaybank, listaOriginal, fecha, currencyId, currentCulture, userSoeid, newGuid, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail);

                            iterator++;
                        }

                        contexto.SaveChanges();

                        IAdaptadorFile adaptadorFile = new AdaptadorFile();

                        IEnumerable<FileGridBodyDTO> fileGrid = adaptadorFile.FabricarFileGridBodyDTO(listaOriginal, countryOriginal);

                        return fileGrid;
                    }
                }              
                catch (DirectoryNotFoundException dirEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dirEx, "PoliticaPostAccountingDirectoryNotFound");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Directory Not found - Search Files Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                catch (UnauthorizedAccessException unEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(unEx, "PoliticaPostAccountingUnauthorizedAccess");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Unauthorized Access - Search Files Process", newEx.Message, userSoeid);

                    throw newEx;
                }             
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseExistenciaImportError");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Generic Exception - Search Files Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                finally 
                {
                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }                
            }
        }

        /// <summary>
        /// Método que realiza el proceso Import de un archivo de pago en especifico
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stateId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ProcesoImportFileFromPaybank(string fileName, FileState stateId, int countryId, int currencyId, string userSoeid, int currentCulture, string identifier, DateTime dateSelected, ExecutionMode executionMode, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();

            using (traceManager.StartTrace("Post Accounting Import Error", newGuid))
            {
                try
                {
                    var createdFileId = 0;

                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);
                        var currencyOriginal = contexto.Currency.FirstOrDefault(p => p.Id == currencyId);

                        var originalPaybankConfiguration = contexto.PaybankConfiguration.Where(p => p.PaybankConfigurationCountries.Any(c => c.CountryId == countryId) && p.PaybankConfigurationCurrencies.Any(c => c.CurrencyId == currencyId));
                        var pathPaybankInclearing = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathInclearing;
                        var pathPaybankReturn = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathReturn;

                        string rutaFinal = null;
                        var iterator = 0;

                        if (System.IO.File.Exists(Path.Combine(pathPaybankInclearing, fileName)))
                        {
                            rutaFinal = pathPaybankInclearing;
                        }
                        else
                        {
                            rutaFinal = pathPaybankReturn;
                            iterator = 1;
                        }

                        var pathProcessed = (identifier == "incomingelectronic" || identifier == "inclearingcheck") ? GetPaybankProcessedFiles(currencyId, 0, originalPaybankConfiguration) : GetPaybankProcessedFiles(currencyId, 1, originalPaybankConfiguration);

                        var pathProcessedFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathProcessed);

                        var directoryInfoProcessedFile = new DirectoryInfo(pathProcessedFiles);
                        var directoryInfoPaybankFile = new DirectoryInfo(rutaFinal);

                        var searchPattern = DominioImportLogica.GetPaybankSearchPattern(currencyId, originalPaybankConfiguration, iterator);

                        var listFilesPaybankFiles = directoryInfoPaybankFile.GetFiles(searchPattern, System.IO.SearchOption.TopDirectoryOnly);

                        var fileSelected = listFilesPaybankFiles.Where(p => p.Name == fileName).FirstOrDefault();

                        if (stateId == FileState.New)
                        {
                            createdFileId = ProcesarArchivoNuevoPaybank(fileSelected, countryOriginal, currencyOriginal, userSoeid, currentCulture, pathProcessedFiles, newGuid, dateSelected, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail);
                        }

                        return createdFileId;
                    }
                }              
                catch (DirectoryNotFoundException dirEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dirEx, "PoliticaPostAccountingDirectoryNotFound");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Directory Not found - Import File Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                catch (UnauthorizedAccessException unEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(unEx, "PoliticaPostAccountingUnauthorizedAccess");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Unauthorized Access - Import File Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Update Concurrency - Import File Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseExistenciaImportError");

                    if (emailConfiguration != null)
                        emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process - Generic Exception - Import File Process", newEx.Message, userSoeid);

                    throw newEx;
                }
                finally 
                {
                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }               
            }
        }

        /// <summary>
        /// Método que obtiene la información del processing file log para los eventos search e import
        /// </summary>
        /// <param name="arrEvent"></param>
        /// <param name="listaArchivosErroneos"></param>
        /// <returns></returns>
        public IEnumerable<IGrouping<string, ProcessingFileLog>> ObtenerLogsOriginalesSearchImport(EventType[] arrEvent, List<string> listaArchivosErroneos)
        {
            using (var contexto = new ContextoPrincipal())
            {
                return contexto.ProcessingFileLog.Where(a => arrEvent.Contains(a.EventTypeId) && !a.FileId.HasValue && listaArchivosErroneos.Contains(a.FileName)).GroupBy(a => a.FileName).ToList();
            }
        }

        #endregion

        #region Inclearing True Transaction - Return

        /// <summary>
        /// Método que realiza el proceso de verificación GI - Citiscreening
        /// </summary>
        /// <param name="fileId"></param>
        public void ProcesoCitiscreeningValidation(int fileId, int countryId, int currencyId, string userSoeid)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);            
            var fileName = string.Empty;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            EmailConfiguration emailConfiguration = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Citiscreening Error", newGuid))
            {               
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId).Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry").Include("TransactionEventOnlineLogs.ErrorMessage").Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();                                                

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;                        

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.CitiscreeningValidation.HasValue && transactionTypeConfigurationCountry.CitiscreeningValidation.Value == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.CitiscreeningError || c.StatusId == TransactionState.Import))
                            {
                                flagProceso = true;

                                var queueConnectivity = contexto.CitiscreeningConfiguration.Where(p => p.CountryId == countryId).Include("CitiscreeningConfigurationCitiscreeningFields.CitiscreeningField").FirstOrDefault();

                                InitializeCitiscreeningConnection(queueConnectivity);

                                EnvioTransaccionesCitiscreening(listaTransaction, queueConnectivity, userSoeid, newGuid, contexto);
                            }
                        }
                    }                    

                    if (flagProceso)
                    {
                        if (listaTransaction.Any(p => p.StatusId == TransactionState.CitiscreeningPending))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.CitiscreeningManualValidationPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.CitiscreeningManualValidationPending, EventType.CitiscreeningValidation));
                        }
                        else if (listaTransaction.All(p => p.StatusId == TransactionState.CitiscreeningOk))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.NoHitsDuringCitiscreeningValidation.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NoHitsDuringCitiscreeningValidation, EventType.CitiscreeningValidation));
                        }
                        else if (listaTransaction.All(p => p.StatusId == TransactionState.CitiscreeningError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateInCitiscreening.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                        }
                        else if (listaTransaction.Any(p => p.StatusId == TransactionState.CitiscreeningError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.CitiscreeningAutomaticValidationPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.CitiscreeningAutomaticValidationPending, EventType.CitiscreeningValidation));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.CitiscreeningValidation);
                    }
                                            
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.CitiscreeningValidation, ProcessFlag.UnableToValidateInCitiscreening, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (CitiscreeningLargeFormatMessageException larForEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(larForEx, "PoliticaCitiscreeningLargeFormatMessageError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1033", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.CitiscreeningValidation, ProcessFlag.UnableToValidateInCitiscreening, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }      
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.CitiscreeningValidation, ProcessFlag.UnableToValidateInCitiscreening, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseCitiscreeningError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1014", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.CitiscreeningValidation, ProcessFlag.UnableToValidateInCitiscreening, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally 
                {
                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso de generacion archivo trande flexcube
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="currencyId"></param>
        /// <param name="culture"></param>
        public void ProcesoTrandeFlexcube(int fileId, int countryId, string userSoeid, int currencyId, int culture)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            var fileName = string.Empty;            
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            var postACHAccess = new PostACHDataAccessLayer();
            ContextoPrincipal contexto = null;
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;

            using (traceManager.StartTrace("Trande Flexcube File Error", newGuid))
            {
                var emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                try
                {
                    contexto = new ContextoPrincipal();
                    
                    var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);

                    listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                        .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.InterfaceSystem.SystemParameter")
                        .Include("TransactionTypeConfigurationCountry.InterfaceSystem.SystemParameter");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;  

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.InterfaceSystem.SystemParameter.Identifier == "flexcube")
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingFlexcubeConfiguration>().Where(p => p.CurrencyOriginalId == currencyId && p.ShippingMethod.Identifier == "trande").FirstOrDefault();

                            if (queueConnectivity == null)
                            {
                                var mensaje = culture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, fileName, fileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

                                return;
                            }

                            CreateTrandeFlexCubeFile(listaTransaction, queueConnectivity, currencyId, userSoeid, countryId, newGuid, culture, listProcessingLog, adaptadorFile, contexto);                            
                        }
                    }
                    
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.FlexcubeFile, ProcessFlag.UnableToGenerateTrandeFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseTrandeFlexcubeFileError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1012", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.FlexcubeFile, ProcessFlag.UnableToGenerateTrandeFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    listaTransaction.ToList().ForEach(p => contexto.Entry<Transaction>(p).State = EntityState.Modified);
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }       

        /// <summary>
        /// Método que realiza el proceso de Post Accounting Open batch
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingOpenBatch(int fileId, int countryId, string userSoeid, int batchId, int currentCulture, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);            
            Country countryOriginal = null;
            Dominio.Core.File fileOriginal = null;            
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            ContextoPrincipal contexto = null;

            using (traceManager.StartTrace("Post Accounting Open Batch Error", newGuid))
            {
                var emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                try
                {                    
                    contexto = new ContextoPrincipal();

                    countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();
                    fileOriginal = contexto.File.Where(p => p.Id == fileId).Include("Batchs.Transactions").Include("TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry").Include("TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations").FirstOrDefault();

                    var transactionTypeConfigurationCountry = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (fileOriginal.Batchs.SelectMany(p => p.Transactions).Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                        {
                            var batchConfiguration = countryOriginal.TransactionTypeConfigurationCountries.SelectMany(p => p.TransactionTypeConfigurationCountryCurrencies).SelectMany(p => p.TransactionTypeBatchConfigurations).FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && p.Id == batchId);

                            if (countryOriginal.TransactionTypeConfigurationCountries.Any(p => p.PostAccounting == true))
                            {
                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                IniciarProcesoOpenBatch(countryOriginal, queueConnectivity, userSoeid, batchConfiguration, currentCulture, newGuid, fileOriginal, currencyId, listProcessingLog, adaptadorFile, contexto);
                            }
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileOriginal.Name, fileId, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseOpenBatchError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1015", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    contexto.Entry<Country>(countryOriginal).State = EntityState.Modified;
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso de Post Accounting Upload
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        public void ProcesoPostAccountingUpload(int fileId, int countryId, string userSoeid, int batchId, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            var postACHAccess = new PostACHDataAccessLayer();
            EmailConfiguration emailConfiguration = null;
            IEnumerable<Transaction> listaTransaction = null;
            var emailServicio = new EmailServicio();
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Upload Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("Banks").Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesUpload(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToUploadToCosmos, EventType.Upload));
                            }
                        }
                    }

                    if (flagProceso)
                    {
                        if (listaTransaction.All(p => p.StatusId == TransactionState.UploadToCosmosButNotAuthorized || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SucessfullyUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SucessfullyUploadToCosmos, EventType.Upload));
                        }
                        else if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UploadToCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UploadToCosmosPending, EventType.Upload));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Upload);
                    }
                                            
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }                
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseUploadError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1017", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realizar el proceso de POst Accounting Authorize
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingAuthorize(int fileId, int countryId, string userSoeid, int batchId, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Authorize Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("Banks").Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesAuthorize(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);                                
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToAuthorizeInCosmos, EventType.Authorize));
                            }
                        }
                    }

                    if (flagProceso)
                    {
                        if (listaTransaction.All(p => p.StatusId == TransactionState.AuthorizeByCosmosOk || p.StatusId == TransactionState.AuthorizeByCosmosRejected || p.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyAuthorizeInCosmos, EventType.Authorize));
                        }
                        else if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.AuthorizationInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.AuthorizationInCosmosPending, EventType.Authorize));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Authorize);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;                    
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx; 
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseAuthorizeError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1018", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx; 
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso de Delete en Cosmos
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="currencyId"></param>
        /// <param name="executionMode"></param>
        public void ProcesoPostAccountingDelete(int fileId, int countryId, string userSoeid, int batchId, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Delete Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("Banks").Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesDelete(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);                                
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToDeleteInCosmos, EventType.Delete));
                            }
                        }
                    }

                    if (flagProceso)
                    {
                        if (listaTransaction.Any(c => c.TransactionEventOnlineLogs.Any(d => d.EventOnlineLog != null && d.EventOnlineLog.EventTypeId == EventType.Delete)))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyDeleteInCosmos, EventType.Delete));
                        }
                        else
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.DeleteInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.DeleteInCosmosPending, EventType.Delete));
                        }                           

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Delete);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseDeleteError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1019", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Metodo para crear el archivo retorno
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        public void ProcesoPostAccountingCreateFACPReturnFile(int fileId, int countryId, string userSoeid, int currencyId, int currentCulture)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            IEnumerable<Transaction> listaTransaction = null;
            var adaptadorFile = new AdaptadorFile();
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            ContextoPrincipal contexto = null; 

            using (traceManager.StartTrace("FACP File Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    contexto = new ContextoPrincipal();
                    
                    var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);
                    var currencyOriginal = contexto.Currency.FirstOrDefault(p => p.Id == currencyId);

                    listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId).Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                        .Include("TransactionEventOnlineLogs.ErrorMessage")
                        .Include("TransactionTrandeFiles.TrandeFile")
                        .Include("TransactionEventOnlineLogs.EventOnlineLog")
                        .Include("PendingChangeCitiscreenings.RejectCode").ToList();

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostPaylink.HasValue && transactionTypeConfigurationCountry.PostPaylink == true)
                    {
                        var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostPaylinkConfiguration>().Where(p => p.CurrencyId == currencyId).FirstOrDefault();

                        if (queueConnectivity == null)
                        {
                            var mensaje = currentCulture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, fileName, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                            return;
                        }

                        CreateFACPReturnFile(listaTransaction, countryOriginal, queueConnectivity, userSoeid, currencyOriginal, newGuid, contexto, listProcessingLog, adaptadorFile);
                    }                    
                }                
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.FACPFile, ProcessFlag.UnableToGenerateFACPFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaFACPFileError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1011", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.FACPFile, ProcessFlag.UnableToGenerateFACPFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    listaTransaction.ToList().ForEach(p => contexto.Entry<Transaction>(p).State = EntityState.Modified);
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Metodo para crear el archivo retorno
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        public void ProcesoPostAccountingCreatePaybankReturnFile(int fileId, int countryId, string userSoeid, int currentCulture, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            IEnumerable<Transaction> listaTransaction = null;
            string fileName = string.Empty;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            ContextoPrincipal contexto = null; 

            using (traceManager.StartTrace("Return File Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    contexto = new ContextoPrincipal();

                    var countryOriginal = contexto.Country.FirstOrDefault(p => p.Id == countryId);

                    listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                        .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                        .Include("TransactionEventOnlineLogs.ErrorMessage")
                        .Include("TransactionTrandeFiles.TrandeFile")
                        .Include("TransactionEventOnlineLogs.EventOnlineLog")
                        .Include("PendingChangeCitiscreenings.RejectCode").ToList();

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.ReturnFile.HasValue && transactionTypeConfigurationCountry.ReturnFile == true)
                    {
                        var paybankReturnFile = countryOriginal.PaybankReturnFileConfigurations.FirstOrDefault(p => p.CountryId == countryId && p.CurrencyId == currencyId);

                        if (paybankReturnFile == null)
                        {
                            var mensaje = currentCulture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, listaTransaction.FirstOrDefault().Batch.File.Name, listaTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                            return;
                        }

                        CreatePaybankReturnFile(listaTransaction, countryOriginal, paybankReturnFile, userSoeid, currencyId, newGuid, currentCulture, listProcessingLog, adaptadorFile);
                    }
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.PaybankReturnFile, ProcessFlag.UnableToGeneratePaybankReturnFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseReturnFileError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1020", fileName, fileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.PaybankReturnFile, ProcessFlag.UnableToGeneratePaybankReturnFile, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    listaTransaction.ToList().ForEach(p => contexto.Entry<Transaction>(p).State = EntityState.Modified);
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        #endregion

        #region Inclearing Check

        /// <summary>
        /// Método que realiza el proceso Open Batch de Post ACH para el tipo de proceso Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingOpenBatchInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currentCulture, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Dominio.Core.File fileOriginal = null;
            ContextoPrincipal contexto = null;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();

            using (traceManager.StartTrace("Post Accounting Open Batch Error", newGuid))
            {
                Country countryOriginal = null;                

                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    contexto = new ContextoPrincipal();                    

                    countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();
                    fileOriginal = contexto.File.Where(p => p.Id == fileId).Include("Batchs.Transactions.Bank.BankBatchBankEventOnlineLogs.BatchBankEventOnlineLog").FirstOrDefault();

                    if (fileOriginal.Batchs.SelectMany(p => p.Transactions).Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                    {
                        if (countryOriginal.TransactionTypeConfigurationCountries.Any(p => p.PostAccounting == true))
                        {
                            if (fileOriginal.StateId == FileState.Complete)
                                return;

                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            IniciarProcesoOpenBatchInclearingCheck(fileOriginal, countryOriginal, queueConnectivity, userSoeid, check, currentCulture, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);
                        }
                    }                    
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileOriginal.Name, fileId, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseOpenBatchError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1015", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileOriginal.Name, newGuid, emailServicio, emailConfiguration, EventType.OpenBatch, ProcessFlag.UnableToOpenBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    contexto.Entry<Country>(countryOriginal).State = EntityState.Modified;
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;                   
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Upload de Post ACH para el tipo de proceso Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingUploadInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Upload Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Bank")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesUploadInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);                                
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToUploadToCosmos, EventType.Upload));
                            }
                        }
                    }

                    if (flagProceso) 
                    {
                        if (listaTransaction.All(p => p.StatusId == TransactionState.UploadToCosmosButNotAuthorized || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SucessfullyUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SucessfullyUploadToCosmos, EventType.Upload));
                        }
                        else if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UploadToCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UploadToCosmosPending, EventType.Upload));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Upload);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseUploadError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1017", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Upload, ProcessFlag.UnableToUploadToCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Authorize de Post ACH para el tipo de proceso Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingAuthorizeInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Authorize Error", newGuid))
            {               
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Bank")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesAuthorizeInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);                                
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToAuthorizeInCosmos, EventType.Authorize));
                            }
                        }
                    }

                    if (flagProceso) 
                    {
                        if (listaTransaction.All(p => p.StatusId == TransactionState.AuthorizeByCosmosOk || p.StatusId == TransactionState.AuthorizeByCosmosRejected || p.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyAuthorizeInCosmos, EventType.Authorize));
                        }
                        else if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.AuthorizationInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.AuthorizationInCosmosPending, EventType.Authorize));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Authorize);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseAuthorizeError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1018", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Authorize, ProcessFlag.UnableToAuthorizeInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Delete de Post ACH para el tipo de proceso Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingDeleteInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            var emailServicio = new EmailServicio();
            IEnumerable<Transaction> listaTransaction = null;
            var flagProceso = false;

            using (traceManager.StartTrace("Post Accounting Delete Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    using (var contexto = new ContextoPrincipal())
                    {
                        countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();

                        listaTransaction = contexto.Transaction.Where(p => p.Batch.FileId == fileId)
                            .Include("Bank")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.Currency")
                            .Include("Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes")
                            .Include("TransactionEventOnlineLogs.ErrorMessage")
                            .Include("TransactionEventOnlineLogs.EventOnlineLog").ToList();

                        fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                        var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                        if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                        {
                            if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                flagProceso = true;

                                var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                                InitializePostAccountingConnection(queueConnectivity);

                                EnvioTransaccionesDeleteInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, contexto, listProcessingLog, adaptadorFile);                               
                            }
                            else
                            {
                                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToDeleteInCosmos, EventType.Delete));
                            }
                        }
                    }

                    if (flagProceso) 
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.DeleteInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.DeleteInCosmosPending, EventType.Delete));
                        }
                        else if (listaTransaction.Any(p => p.StatusId == TransactionState.DeletedInCosmosOk || p.StatusId == TransactionState.DeletedInCosmosClientAccountError || p.StatusId == TransactionState.DeletedInCosmosHoldingAccountError))
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyDeleteInCosmos, EventType.Delete));
                        }

                        postACHAccess.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Delete);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseDeleteError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1019", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));

                    ObtenerResumenEmailError(countryId, currencyId, userSoeid, fileId, fileName, newGuid, emailServicio, emailConfiguration, EventType.Delete, ProcessFlag.UnableToDeleteInCosmos, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                }
            }
        }

        #endregion

        #region Close Batch

        /// <summary>
        /// Método que ejecuta el proceso close batch para aquellos que se encuentren abiertos
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batdhIds"></param>
        public void ProcesoPostAccountingCloseBatch(int countryId, int currencyId, string userSoeid, int[] batchIds, int currentCulture)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;            
            var adaptadorFile = new AdaptadorFile();
            var listProcessingLog = new List<ProcessingFileLog>();
            EmailConfiguration emailConfiguration = null;
            var postACHAccess = new PostACHDataAccessLayer();
            ContextoPrincipal contexto = null;
            IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration = null;
            var emailServicio = new EmailServicio();
            IEnumerable<ErrorMessage> errorMessages = null;

            using (traceManager.StartTrace("Post Accounting Close Batch Error", newGuid))
            {                
                try
                {
                    emailConfiguration = this.ObtenerEmailConfiguration(countryId);

                    contexto = new ContextoPrincipal();

                    countryOriginal = countryOriginal = contexto.Country.Where(p => p.Id == countryId).Include("CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                    errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active).Include("System").Include("NachaCode");

                    batchConfiguration = contexto.TransactionTypeBatchConfiguration.Where(p => p.FieldStatusId == FieldStatus.Active && batchIds.Contains(p.Id)).Include("BatchConfigurationBatchEventOnlineLogs.BatchEventOnlineLog");                    

                    if (batchConfiguration.Any(p => p.IsOpen))
                    {
                        var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                        InitializePostAccountingConnection(queueConnectivity);

                        IniciarProcesoCloseBatch(countryOriginal, queueConnectivity, userSoeid, batchConfiguration, currentCulture, newGuid, currencyId, errorMessages, contexto, listProcessingLog, adaptadorFile);

                        if (userSoeid == "Scheduler")
                        {                            
                            if (emailConfiguration != null)
                            {
                                emailServicio.EnviarEmail(emailConfiguration, "Process ACH Scheduler Close Batch", "TemplateEmailCloseBatch", batchConfiguration, userSoeid);
                            }
                        }

                    }
                    else
                    {
                        if (userSoeid == "Scheduler")
                        {                           
                            if (emailConfiguration != null)
                            {
                                emailServicio.EnviarEmail(emailConfiguration, "Process ACH Scheduler Close Batch", "No data found.", userSoeid);
                            }
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));

                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, mqEx.ReasonCode, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    ObtenerCloseBatchResumenEmailError(countryId, currencyId, userSoeid, newGuid, batchConfiguration, emailServicio, emailConfiguration, EventType.CloseBatch, ProcessFlag.UnableToCloseBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }              
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));

                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, 990, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    ObtenerCloseBatchResumenEmailError(countryId, currencyId, userSoeid, newGuid, batchConfiguration, emailServicio, emailConfiguration, EventType.CloseBatch, ProcessFlag.UnableToCloseBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseCloseBatchError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1016", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));

                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, 1016, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    ObtenerCloseBatchResumenEmailError(countryId, currencyId, userSoeid, newGuid, batchConfiguration, emailServicio, emailConfiguration, EventType.CloseBatch, ProcessFlag.UnableToCloseBatch, newEx.Message, listProcessingLog, adaptadorFile);

                    throw newEx;
                }
                finally
                {
                    batchConfiguration.ToList().ForEach(p => contexto.Entry<TransactionTypeBatchConfiguration>(p).State = EntityState.Modified);                    
                    contexto.SaveChanges();

                    if (contexto != null)
                        contexto.Dispose();

                    if (listProcessingLog.Any())
                        postACHAccess.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);

                    if (this.messageQueueFactory != null)
                        this.messageQueueFactory.Dispose();

                    adaptadorFile = null;
                    listProcessingLog = null;
                    postACHAccess = null;
                    emailServicio = null;
                }
            }
        }

        #endregion

        #endregion

        #region Metodos Privados Inclearing Return

        #region Import

        /// <summary>
        /// Método que procesa el archivo desde paybank para su inserción en la base de datos
        /// </summary>
        /// <param name="listaArchivosPaybank"></param>
        /// <param name="listaOriginal"></param>
        private int ProcesarArchivoNuevoPaybank(FileInfo fileSelected, Country countryOriginal, Currency currencyOriginal, string userSoeid, int currentCulture, string pathProcessedFiles, Guid newGuid, DateTime dateSelected, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {            
            var fileId = 0;
            IAdaptadorFile adaptadorFile = new AdaptadorFile();
            IPostACHDataAccessLayer postDataAccess = new PostACHDataAccessLayer();

            try
            {                
                var returnCodeMappingConfigurationOriginal = countryOriginal.ReturnCodeMappingConfigurations.AsEnumerable();

                var filasArchivo = System.IO.File.ReadAllLines(Path.Combine(fileSelected.DirectoryName, fileSelected.Name));

                var headerLine = filasArchivo.FirstOrDefault();
                var primeraFila = filasArchivo[1];
                var bodyLines = filasArchivo.Skip(1).Take(filasArchivo.Count() - 1 - 1);
                var controlLine = filasArchivo.LastOrDefault();
                var sumSaldoGuatemalaCredito = Decimal.Zero;
                var sumSaldoGuatemalaDebito = Decimal.Zero;

                var file = DominioImportLogica.ProcesarArchivoNuevoPaybank(fileSelected.Name, fileSelected.CreationTime, headerLine, primeraFila, controlLine, bodyLines, countryOriginal, currentCulture, userSoeid, ref sumSaldoGuatemalaCredito, ref sumSaldoGuatemalaDebito);

                var connectionName = GetPaybankConnectionName(countryOriginal, currencyOriginal);

                var transactions = file.Batchs.SelectMany(a => a.Transactions);

                var totalDebit = transactions.Where(a => a.NatureTransactionId == NatureTransaction.Debit).Sum(a => a.Amount) + sumSaldoGuatemalaDebito;
                var totalCredit = transactions.Where(a => a.NatureTransactionId == NatureTransaction.Credit).Sum(a => a.Amount) + sumSaldoGuatemalaCredito;

                int? numberConsArchivo = default(int);

                if ((totalDebit != file.TotalDebit) || (totalCredit != file.TotalCredit))
                {
                    numberConsArchivo = null;
                }
                else
                {
                    IPaybankDataAccessLayer paybankDataAccess = new PaybankDataAccessLayer();

                    numberConsArchivo = paybankDataAccess.ValidateOuputFile(file.Name, file.CreationDate.Value, file.TotalDebit, file.TotalCredit, connectionName);
                }

                if (numberConsArchivo == null)
                    throw new ValidationPaybankException(currentCulture == 0 ? "The file can not be imported because it does not meet validation Paybank." : "El archivo no se puedo importar porque no cumple con la validación de Paybank.");

                file.NumConsArchivo = numberConsArchivo;

                var tipoTransaction = VerificarTipoProcesoArchivo(countryOriginal, file, currencyOriginal.Id, currentCulture, newGuid, userSoeid, file.DateSelected, EventType.Import, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail);

                if (tipoTransaction == null)
                    return fileId;

                SetAccountConfigurationByProcessConfiguration(tipoTransaction, file, countryOriginal.Id);

                file.Batchs.SelectMany(p => p.Transactions).ToList().ForEach(p =>
                {
                    if (tipoTransaction.TransactionType.Identifier != "inclearingcheck")
                    {
                        DominioImportLogica.SetFinancialInstitutionCodeDestination(p);
                    }

                    if (tipoTransaction.TransactionType.Identifier == "return")
                        DominioImportLogica.ValidateReturnCodeMappingConfiguration(returnCodeMappingConfigurationOriginal, tipoTransaction, p, countryOriginal);
                });

                file.StateId = FileState.Pending;

                var tipo = tipoTransaction.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).SelectMany(p => p.TransactionTypeConfigurationCountryCurrencies).Where(c => c.CurrencyId == currencyOriginal.Id).FirstOrDefault();

                file.TransactionTypeConfigurationCountryCurrencyId = tipo.Id;                

                fileId = postDataAccess.BulkInsertPostACH(file, true);

                fileSelected.CopyTo(Path.Combine(pathProcessedFiles, fileSelected.Name), true);
                fileSelected.Delete();

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.SucessfullyImported.GetDescription(), null, fileSelected.Name, fileId, newGuid, ProcessFlag.SucessfullyImported, EventType.Import));
            }
            catch (FormatException forEx)
            {
                errorFilesDetail.UnableToReadFileFormat = true;
                SaveCriticalFiles(fileSelected.Name, listaArchivosErroneos);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(forEx, "PoliticaPostAccountingFormatError");
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, newEx.Message, "1002", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.Import));
            }
            catch (ValidationPaybankException payEx)
            {
                errorFilesDetail.UnsucessfullyPaybankValidationFile = true;
                SaveCriticalFiles(fileSelected.Name, listaArchivosErroneos);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(payEx, "PoliticaValidationPaybankException");
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, newEx.Message, "1022", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToImport, EventType.Import));
            }
            catch (NachaReturnCodeMappingException nshEx)
            {
                errorFilesDetail.UnableToVerifyNachaReturnCodeMapping = true;
                SaveCriticalFiles(fileSelected.Name, listaArchivosErroneos);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(nshEx, "PoliticaNachaReturnCodeMapping");
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, newEx.Message, "1025", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyNachaReturnCodeMapping, EventType.Import));
            }
            catch (Exception ex)
            {
                errorFilesDetail.UnableToReadFileFormat = true;
                SaveCriticalFiles(fileSelected.Name, listaArchivosErroneos);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(ex, "PoliticaBaseImportError");
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, newEx.Message, "1003", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToImport, EventType.Import));
            }

            return fileId;
        }

        /// <summary>
        /// Método que verifica y obtiene la existencia de los archivos especificos para importar
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="listaArchivosPaybank"></param>
        /// <param name="listaOriginal"></param>
        /// <param name="fecha"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="contexto"></param>
        /// <param name="listProcessingLog"></param>
        /// <param name="listaArchivosErroneos"></param>
        /// <param name="errorFilesDetail"></param>
        private void ProcesarArchivoPaybankExistencia(Country countryOriginal, IEnumerable<FileInfo> listaArchivosPaybank, IList<Dominio.Core.File> listaOriginal, DateTime fecha, int currencyId, int currentCulture, string userSoeid, Guid newGuid, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {
            IAdaptadorFile adaptadorFile = new AdaptadorFile();

            var returnCodeMappingConfigurationOriginal = contexto.ReturnCodeMappingConfiguration.AsEnumerable();

            foreach (var fileInfo in listaArchivosPaybank)
            {
                try
                {
                    var dateShort = fecha.Date;
                    var creationFileDate = fileInfo.CreationTime.Date;

                    if (DateTime.Compare(dateShort, creationFileDate) == 0)
                    {
                        var filasArchivo = System.IO.File.ReadAllLines(Path.Combine(fileInfo.DirectoryName, fileInfo.Name));

                        var headerLine = filasArchivo.FirstOrDefault();
                        var primeraFila = filasArchivo[1];
                        var bodyLines = filasArchivo.Skip(1).Take(filasArchivo.Count() - 1 - 1);
                        var controlLine = filasArchivo.LastOrDefault();
                        var sumSaldoGuatemalaCredito = Decimal.Zero;
                        var sumSaldoGuatemalaDebito = Decimal.Zero;

                        var file = DominioImportLogica.ProcesarArchivoNuevoPaybank(fileInfo.Name, fileInfo.CreationTime, headerLine, primeraFila, controlLine, bodyLines, countryOriginal, currentCulture, userSoeid, ref sumSaldoGuatemalaCredito, ref sumSaldoGuatemalaDebito);

                        var tipoTransaction = VerificarTipoProcesoArchivo(countryOriginal, file, currencyId, currentCulture, newGuid, userSoeid, fecha, EventType.SearchFiles, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail);

                        if (tipoTransaction == null)
                            continue;

                        DominioImportLogica.SetAccountConfigurationByProcessConfiguration(tipoTransaction, file, countryOriginal.Id);

                        file.Batchs.SelectMany(p => p.Transactions).ToList().ForEach(p =>
                        {
                            if (tipoTransaction.TransactionType.Identifier != "inclearingcheck")
                            {
                                DominioImportLogica.SetFinancialInstitutionCodeDestination(p);
                            }

                            if (tipoTransaction.TransactionType.Identifier == "return")
                                DominioImportLogica.ValidateReturnCodeMappingConfiguration(returnCodeMappingConfigurationOriginal, tipoTransaction, p, countryOriginal);
                        });

                        if (tipoTransaction.TransactionType.Identifier == "inclearingcheck")
                        {
                            if (HttpContext.Current != null)
                            {
                                var dictionary = (Dictionary<string, List<int?>>)HttpContext.Current.Session["BankDictionary"] ?? new Dictionary<string, List<int?>>();

                                dictionary.Add(fileInfo.Name, file.Batchs.SelectMany(p => p.Transactions).Where(p => p.BankId.HasValue).Select(p => p.BankId).ToList());

                                HttpContext.Current.Session["BankDictionary"] = dictionary;
                            }
                        }

                        file.TransactionTypeConfigurationCountryCurrency = new TransactionTypeConfigurationCountryCurrency
                        {
                            TransactionTypeConfigurationCountry = new TransactionTypeConfigurationCountry
                            {
                                TransactionTypeConfiguration = new TransactionTypeConfiguration
                                {
                                    Id = tipoTransaction.Id,
                                    TransactionType = new Parameter
                                    {
                                        Identifier = tipoTransaction.TransactionType.Identifier
                                    }
                                }
                            }
                        };

                        listaOriginal.Add(file);
                    }
                }
                catch (FormatException forEx)
                {
                    errorFilesDetail.UnableToReadFileFormat = true;
                    SaveCriticalFiles(fileInfo.Name, listaArchivosErroneos);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(forEx, "PoliticaPostAccountingFormatError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1002", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.SearchFiles));                    
                }
                catch (NachaReturnCodeMappingException nshEx)
                {
                    errorFilesDetail.UnableToVerifyNachaReturnCodeMapping = true;
                    SaveCriticalFiles(fileInfo.Name, listaArchivosErroneos);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(nshEx, "PoliticaNachaReturnCodeMapping");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1025", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyNachaReturnCodeMapping, EventType.SearchFiles));
                }               
                catch (Exception ex)
                {
                    errorFilesDetail.UnableToReadFileFormat = true;
                    SaveCriticalFiles(fileInfo.Name, listaArchivosErroneos);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(ex, "PoliticaBaseExistenciaImportError");
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1013", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.SearchFiles));
                }
            }
        }

        /// <summary>
        /// Método que obtiene el nombre de la conexion a la BD de Paybank dependiendo del Pais
        /// </summary>
        /// <param name="country"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private string GetPaybankConnectionName(Country country, Currency currency)
        {
            string connectionName = null;

            switch (country.Name)
            {
                case "Trinidad Y Tobago":
                    connectionName = "TrinidadConnection";
                    break;
                case "Jamaica":
                    connectionName = "JamaicaConnection";
                    break;
                case "Honduras":
                    connectionName = currency.Code.Contains("USD") ? "HondurasDolarConnection" : "HondurasLocalConnection";
                    break;
                case "Guatemala":
                    connectionName = currency.Code.Contains("USD") ? "GuatemalaDolarConnection" : "GuatemalaLocalConnection";
                    break;
                case "Dominican Republic":
                    connectionName = "DominicanaConnection";
                    break;
            }

            return connectionName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processConfiguration"></param>
        private void SetAccountConfigurationByProcessConfiguration(TransactionTypeConfiguration processConfiguration, Dominio.Core.File fileOriginal, int countryId)
        {
            var configurationCountry = processConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryId).FirstOrDefault();

            fileOriginal.Batchs.SelectMany(p => p.Transactions).ToList().ForEach((p) =>
            {
                int? largo = null;

                if (p.AccountNumber.Length > configurationCountry.AccountLarge)
                    largo = configurationCountry.AccountLarge;

                var fillCharacter = string.IsNullOrEmpty(configurationCountry.FillCharacter) ? "" : configurationCountry.FillCharacter.Replace("\0", "");

                p.AccountNumber = configurationCountry.AccountFillAlignmentId == AccountFillAlignment.Left ? (string.IsNullOrEmpty(fillCharacter) ? p.AccountNumber : p.AccountNumber.PadLeft(configurationCountry.AccountLarge, fillCharacter[0])) : (string.IsNullOrEmpty(fillCharacter) ? p.AccountNumber : p.AccountNumber.PadRight(configurationCountry.AccountLarge, fillCharacter[0]));

                if (largo.HasValue)
                    p.AccountNumber = p.AccountNumber.Substring(p.AccountNumber.Length - largo.Value);
            });
        }        

        /// <summary>
        /// Método que verifica el tipo de proceso al cual pertenece el archivo
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="file"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="fecha"></param>
        /// <param name="eventType"></param>
        /// <param name="contexto"></param>
        /// <param name="listProcessingLog"></param>
        /// <param name="listaArchivosErroneos"></param>
        /// <param name="errorFilesDetail"></param>
        /// <returns></returns>
        private TransactionTypeConfiguration VerificarTipoProcesoArchivo(Country countryOriginal, Dominio.Core.File file, int currencyId, int currentCulture, Guid newGuid, string userSoeid, DateTime fecha, EventType eventType, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {
            var mensaje = string.Empty;
            IAdaptadorFile adaptadorFile = new AdaptadorFile();

            var paybankConfiguration = countryOriginal.PaybankConfigurationCountries.FirstOrDefault().PaybankConfiguration;

            var codDestinoInmediato = file.Batchs.SelectMany(p => p.Transactions).FirstOrDefault().ImmediateDestinationCodeTrade;

            var transactionCodes = file.Batchs.SelectMany(p => p.Transactions).Select(c => int.Parse(c.TransactionCode)).ToArray();

            if (codDestinoInmediato == paybankConfiguration.RFICity || codDestinoInmediato == paybankConfiguration.Ofi1)//Destino de archivos propios OnUs
            {
                var answer = false;

                var transactionTypeConfiguration = contexto.TransactionTypeConfiguration.FirstOrDefault(p => p.TransactionType.Identifier == "inclearingcheck" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));

                if (transactionTypeConfiguration != null)
                {
                    if (file.Batchs.Any(c => c.SECCode == "TRC"))
                    {                        
                        answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Any(p => p.CountryId == countryOriginal.Id && p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                        if (answer)
                        {
                            if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail))
                                return null;

                            var arrSecCodes = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Select(p => p.SEC).FirstOrDefault().Split('-');

                            if (file.Batchs.Any(p => arrSecCodes.Contains(p.SECCode)))
                            {
                                file.Batchs.SelectMany(p => p.Transactions).ToList().ForEach(p => DominioImportLogica.ValidateFinancialInstitutionCodeDestination(countryOriginal, transactionTypeConfiguration, p, currentCulture));

                                var varOut = file.Batchs.SelectMany(p => p.Transactions).FirstOrDefault().FinancialInstitutionCodeDestination.PadLeft(15, '0');
                                var codBancoStringOut = varOut.Substring(varOut.Length - 3, 3);

                                var varIn = file.Batchs.SelectMany(p => p.Transactions).FirstOrDefault().TrackingNumber.PadLeft(20, '0');
                                var codBancoStringIn = varIn.Substring(0, 13);

                                if (int.Parse(codBancoStringOut) != int.Parse(paybankConfiguration.RFICity) || int.Parse(codBancoStringIn) != int.Parse(paybankConfiguration.RFICity)) //Inclearing Check On Us
                                {
                                    answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Any(p => p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                                    if (answer)
                                        return transactionTypeConfiguration;
                                }
                            }
                        }
                        else
                        {
                            var exceptArrayInclearingCheck = GetExceptArrayTransactionCodes(countryOriginal, currencyId, transactionCodes, contexto);

                            return UnableVerifyProcessType(countryOriginal, file, currencyId, currentCulture, newGuid, eventType, ref mensaje, exceptArrayInclearingCheck, listProcessingLog, listaArchivosErroneos, errorFilesDetail, adaptadorFile);
                        }
                    }
                }

                transactionTypeConfiguration = contexto.TransactionTypeConfiguration.FirstOrDefault(p => p.TransactionType.Identifier == "return" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));                

                if (transactionTypeConfiguration != null)
                {
                    answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Any(p => p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                    if (answer)
                    {
                        if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail))
                            return null;

                        var arrSecCodes = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Select(p => p.SEC).FirstOrDefault().Split('-');

                        answer = file.Batchs.Any(p => arrSecCodes.Contains(p.SECCode));

                        if (answer)
                            return transactionTypeConfiguration;
                    }
                }

                transactionTypeConfiguration = contexto.TransactionTypeConfiguration.FirstOrDefault(p => p.TransactionType.Identifier == "incomingelectronic" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));                

                if (transactionTypeConfiguration != null)
                {
                    answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Any(p => p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                    if (answer)
                    {
                        if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid, contexto, listProcessingLog, listaArchivosErroneos, errorFilesDetail))
                            return null;

                        var arrSecCodes = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Select(p => p.SEC).FirstOrDefault().Split('-');

                        answer = file.Batchs.Any(p => arrSecCodes.Contains(p.SECCode));

                        if (answer)
                            return transactionTypeConfiguration;
                    }
                }

                var exceptArrayReturnInclearing = GetExceptArrayTransactionCodes(countryOriginal, currencyId, transactionCodes, contexto);

                return UnableVerifyProcessType(countryOriginal, file, currencyId, currentCulture, newGuid, eventType, ref mensaje, exceptArrayReturnInclearing, listProcessingLog, listaArchivosErroneos, errorFilesDetail, adaptadorFile);
            }

            mensaje = currentCulture == 0 ? "The imported file does not correspond to a destination ONUs own files." : "El archivo importado no corresponde a un destino de archivos propios OnUs.";

            SaveCriticalFiles(file.Name, listaArchivosErroneos);

            ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaOnusOwnFileValidationException");

            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1024", file.Name, (int?)null, newGuid, ProcessFlag.NotONUsOwnFileDestinationFile, eventType));

            errorFilesDetail.NotONUsOwnFileDestinationFile = true;

            return null;
        }

        /// <summary>
        /// Método que maneja la excepcion en el caso de que no se pueda identificar el tipo de proceso asociado al archivo
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="file"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="eventType"></param>
        /// <param name="mensaje"></param>
        /// <param name="exceptArray"></param>
        /// <returns></returns>
        private TransactionTypeConfiguration UnableVerifyProcessType(Country countryOriginal, Dominio.Core.File file, int currencyId, int currentCulture, Guid newGuid, EventType eventType, ref string mensaje, IEnumerable<int> exceptArray, List<ProcessingFileLog> listProcessingLog, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail, IAdaptadorFile adaptadorFile)
        {
            mensaje = currentCulture == 0 ? string.Format("There is no existing transaction code to verify the transaction type associated with the file. Transaction Codes: {0}", exceptArray.Select(p => p.ToString()).Aggregate((a, b) => a + "," + b)) : string.Format("No existe un código de transaccion existente para verificar el tipo de transacción asociado al archivo. Códigos Transacciones: {0}", exceptArray.Select(p => p.ToString()).Aggregate((a, b) => a + "," + b));

            SaveCriticalFiles(file.Name, listaArchivosErroneos);

            ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaProcessTypeVerificationException");

            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1023", file.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyProcessType, eventType));

            errorFilesDetail.UnableToVerifyProcessType = true;

            return null;
        }

        /// <summary>
        /// Método que obtiene el array de datos de los transaction codes por pais y moneda
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="transactionCodes"></param>
        /// <returns></returns>
        private IEnumerable<int> GetExceptArrayTransactionCodes(Country countryOriginal, int currencyId, IEnumerable<int> transactionCodes, ContextoPrincipal contexto)
        {
            var systemTransactionCodes = contexto.TransactionCode.Where(p => p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryOriginal.Id);

            var paybankCodeArrayInt = systemTransactionCodes.Select(p => int.Parse(p.PaybankCode));

            var exceptArray = transactionCodes.Except(paybankCodeArrayInt);

            return exceptArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private bool ValidateSecCodesApplication(TransactionTypeConfiguration transactionTypeConfiguration, Dominio.Core.File file, int currentCulture, Country countryOriginal, int currencyId, EventType eventType, Guid newGuid, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, List<string> listaArchivosErroneos, ErrorFilesSchedulerDetail errorFilesDetail)
        {
            var answer = true;

            if (transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Any(p => p.CountryId == countryOriginal.Id && p.SEC == null))
            {
                var mensaje = currentCulture == 0 ? string.Format("{0} : {1}", "No existe un codigo SEC configurado en la aplicación.", transactionTypeConfiguration.TransactionType.SpanishGloss) : string.Format("{0} : {1}", "There is no SEC configured in the application code.", transactionTypeConfiguration.TransactionType.Gloss);

                SaveCriticalFiles(file.Name, listaArchivosErroneos);

                ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaProcessTypeVerificationException");

                IAdaptadorFile adaptadorFile = new AdaptadorFile();

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1023", file.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyProcessType, eventType));                
                
                errorFilesDetail.UnableToVerifyProcessType = true;

                answer = false;
            }

            return answer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveCriticalFiles(string fileName, List<string> listaArchivosErroneos)
        {
            listaArchivosErroneos.Add(fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="country"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private string GetPaybankProcessedFiles(int currencyId, int iterator, IEnumerable<PaybankConfiguration> originalPaybankConfiguration)
        {
            string pathProcessedFiles = null;

            pathProcessedFiles = iterator == 0 ? originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathBackInclearing : originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathBackReturn;            

            return pathProcessedFiles;
        }   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileOriginal"></param>
        private List<int> VerificarFinalStatus(IEnumerable<Dominio.Core.File> listaFiles, PaybankReturnFileConfiguration paybankReturnFileConfiguration, ContextoPrincipal contexto, int countryId)
        {
            var removeArrids = new List<int>();

            var schedulerConfiguration = contexto.SchedulerPostACHConfiguration.FirstOrDefault(p => p.CountryId == countryId);

            foreach (var file in listaFiles)
            {              
                if (file.Batchs.SelectMany(p => p.Transactions).Any(p => p.StatusId == TransactionState.TrandeGenerationError || p.StatusId == TransactionState.PaybankReturnFileGenerationError || p.StatusId == TransactionState.FACPFileGenerationError))
                {
                    continue;
                }

                var answer = DominioImportLogica.ValidateFinalStatus(file, paybankReturnFileConfiguration, schedulerConfiguration);

                if (answer)
                {
                    removeArrids.Add(file.Id);

                    if (file.StateId != FileState.Complete)
                        file.StateId = FileState.Complete;

                    contexto.Entry<Dominio.Core.File>(file).State = EntityState.Modified;

                    continue;
                }
            }

            return removeArrids;
        }                

        #endregion

        #region Citiscreening

        /// <summary>
        /// Método que inicializa la conexion a Citiscreening
        /// </summary>
        /// <param name="queueConnectivity"></param>
        private void InitializeCitiscreeningConnection(CitiscreeningConfiguration queueConnectivity)
        {
            this.messageQueueFactory = new MessageQueueFactory(new WebsphereMessageQueue());

            this.messageQueueFactory.Initialization(new MessageQueueSpecs()
            {
                Channel = queueConnectivity.Channel,
                QueueManager = queueConnectivity.QueueManager,
                ListenQueue = queueConnectivity.ListenQueue,
                RemoteQueue = queueConnectivity.RemoteQueue,
                ReplyQueue = queueConnectivity.ReplyQueue,
                ReplyQueueManager = queueConnectivity.ReplyQueueManager,
                HostName = queueConnectivity.DNS,
                Attempts = 5,
                Intervals = Convert.ToInt32(queueConnectivity.WaitingAnswerTime.TotalSeconds),
                MessagePattern = MessagePattern.RequestResponse,
                AccessTypeGet = AccessType.MQOO_INPUT_SHARED,
                AccessTypePut = AccessType.MQOO_OUTPUT
            });            
        }

        /// <summary>
        /// Método que envia las transacciones segun su estado a GI
        /// </summary>
        /// <param name="listaTransaction"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        private void EnvioTransaccionesCitiscreening(IEnumerable<Transaction> listaTransaction, CitiscreeningConfiguration configuration, string userSoeid, Guid newGuid, ContextoPrincipal contexto)
        {          
            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "citiscreening" && p.FieldStatusId == FieldStatus.Active);

            var transactionFilter = listaTransaction.Where(c => c.StatusId == TransactionState.CitiscreeningError || c.StatusId == TransactionState.Import).Select((value, index) => new { Value = value, Index = index });

            foreach (var item in transactionFilter) 
            {
                ValidarTransaccionCitiscreening(item.Index, newGuid, item.Value, configuration, errorMessages, userSoeid);
            }      
        }

        /// <summary>
        /// Método que realiza las validaciones correspondientes a citiscreening
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionCitiscreening(int index, Guid newGuid, Transaction transaction, CitiscreeningConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;
            var txn = string.Empty;

            var txnId = DominioCitiscreeningLogica.ObtenerTxnId(index.ToString());

            txn = DominioCitiscreeningLogica.ObtenerStringTxnCitiscreeningConfiguration(configuration, transaction).Select(p => p).Aggregate((a, b) => a + "," + b);

            txn = string.IsNullOrEmpty(txn) ? "," : txn;

            var sendMessage = DominioCitiscreeningLogica.FormatLargoMensajeComposicionCitiscreening(txnId, txn, configuration);

            this.messageQueueFactory.Send(new Message() { Body = sendMessage, EventTypeId = EventType.CitiscreeningValidation }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            if (codeReturn == 0)
            {
                int resto = 0;

                int result = Math.DivRem(replyMessage.Length - 32 + 1, 3845, out resto);

                if (replyMessage.Length == 32 || resto == 0)
                {
                    var numberOfMatches = int.Parse(replyMessage.Substring(30, 2).Trim());

                    transaction.StatusId = numberOfMatches > 0 ? TransactionState.CitiscreeningPending : TransactionState.CitiscreeningOk;

                    if (numberOfMatches > 0)
                        IngresarEventoTransaccionCitiscreeningOK(transaction, codeReturn, newGuid, sendMessage, replyMessage, userSoeid, configuration, txnId);
                    else
                        IngresarEventoTransaccionCitiscreeningOkNoMatches(transaction, codeReturn, newGuid, sendMessage, replyMessage, userSoeid, configuration, txnId);
                    
                }
                else
                {
                    transaction.StatusId = TransactionState.CitiscreeningError;

                    IngresarEventoTransaccionCitiscreeningError(transaction, errorMessages, codeReturn, newGuid, sendMessage, replyMessage, userSoeid);
                }
            }
            else
            {
                transaction.StatusId = TransactionState.CitiscreeningError;

                IngresarEventoTransaccionCitiscreeningError(transaction, errorMessages, codeReturn, newGuid, sendMessage, replyMessage, userSoeid);
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de una respuesta correcta desde GI
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="codeReturn"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        private void IngresarEventoTransaccionCitiscreeningOK(Transaction transaction, int codeReturn, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, CitiscreeningConfiguration configuration, string txnId)
        {
            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            var trxEventLog = new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    Date = DateTime.Now,
                    EventTypeId = EventType.CitiscreeningValidation,
                    EventStateId = EventState.Complete,
                    MessageSend = sendMessage,
                    MessageReceived = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            };

            trxEventLog.CitiscreeningPendingCompositions = SetCitiscreeningPendingCompositionsWithMatches(receiveMessage, transaction, configuration);

            transaction.TransactionEventOnlineLogs.Add(trxEventLog);
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de una respuesta incorrecta desde GI
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        private void IngresarEventoTransaccionCitiscreeningError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    Date = DateTime.Now,
                    EventTypeId = EventType.CitiscreeningValidation,
                    EventStateId = EventState.Incomplete,
                    MessageSend = sendMessage,
                    MessageReceived = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Transaction Id : {0}, Return Code: {1}", transaction.Id, codeReturn)), "PoliticaCitiscreeningError");
            }
        }        

        #endregion

        #region Release 2

        /// <summary>
        /// Método que registra un nuevo evento en el caso de una respuesta correcta desde GI sin hits 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="codeReturn"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="configuration"></param>
        /// <param name="txnId"></param>
        private void IngresarEventoTransaccionCitiscreeningOkNoMatches(Transaction transaction, int codeReturn, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, CitiscreeningConfiguration configuration, string txnId)
        {
            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            var trxEventLog = new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    Date = DateTime.Now,
                    EventTypeId = EventType.CitiscreeningValidation,
                    EventStateId = EventState.Complete,
                    MessageSend = sendMessage,
                    MessageReceived = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            };

            trxEventLog.CitiscreeningPendingCompositions = SetCitiscreeningPendingCompositionsNoMatches(transaction, configuration, txnId);

            transaction.TransactionEventOnlineLogs.Add(trxEventLog);
        }

        /// <summary>
        /// Metodo que crear un registro de la composicion de la respuesta recibida desde Citiscreening sin hits asociados
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="txnId"></param>
        /// <returns></returns>
        private HashSet<CitiscreeningPendingComposition> SetCitiscreeningPendingCompositionsNoMatches(Transaction transaction, CitiscreeningConfiguration configuration, string txnId)
        {
            string sendFieldValue = string.Empty, sendField = string.Empty;
            sendFieldValue = DominioCitiscreeningLogica.ObtenerStringTxnCitiscreeningConfiguration(configuration, transaction).Select(p => p).Aggregate((a, b) => a + "," + b);
            sendFieldValue = string.IsNullOrEmpty(sendFieldValue) ? "," : sendFieldValue;
            sendField = DominioCitiscreeningLogica.ObtenerStringSendFieldCitiscreeningConfiguration(configuration, transaction).Select(p => p).Aggregate((a, b) => a + "," + b);
            sendField = string.IsNullOrEmpty(sendField) ? "," : sendField;
            var ruleSetName = DominioCitiscreeningLogica.GetRuleSetCitiscreeningConfiguration(configuration);

            return new HashSet<CitiscreeningPendingComposition>(){
                    new CitiscreeningPendingComposition{                    
                        TxnId = txnId,
                        HitCount = (short)0,  
                        CitiscreeningSendField = sendField,
                        CitiscreeningSendFieldValue = sendFieldValue,
                        RuleSetName = ruleSetName
                    }
            };
        }

        /// <summary>
        /// Metodo que crear un registro de la composicion de la respuesta recibida desde Citiscreening con hits asociados
        /// </summary>
        /// <param name="giAnswer"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private HashSet<CitiscreeningPendingComposition> SetCitiscreeningPendingCompositionsWithMatches(string giAnswer, Transaction transaction, CitiscreeningConfiguration configuration)
        {
            HashSet<CitiscreeningPendingComposition> citiscreeningPendingCompositions = new HashSet<CitiscreeningPendingComposition>();
            string sendFieldValue = string.Empty, sendField = string.Empty;
            sendFieldValue = DominioCitiscreeningLogica.ObtenerStringTxnCitiscreeningConfiguration(configuration, transaction).Select(p => p).Aggregate((a, b) => a + "," + b);
            sendFieldValue = string.IsNullOrEmpty(sendFieldValue) ? "," : sendFieldValue;
            sendField = DominioCitiscreeningLogica.ObtenerStringSendFieldCitiscreeningConfiguration(configuration, transaction).Select(p => p).Aggregate((a, b) => a + "," + b);
            sendField = string.IsNullOrEmpty(sendField) ? "," : sendField;

            var numberOfMatches = int.Parse(giAnswer.Substring(30, 2).Trim());
            var ruleSetName = DominioCitiscreeningLogica.GetRuleSetCitiscreeningConfiguration(configuration);

            var arrGiAnswer = giAnswer.Split(new char[] { '*' });
            var firstRow = arrGiAnswer.FirstOrDefault();
            var bodyLines = arrGiAnswer.Skip(1).Take(arrGiAnswer.Count() - 1).ToArray();

            citiscreeningPendingCompositions.Add(
                    new CitiscreeningPendingComposition
                    {
                        TxnId = firstRow.Substring(0, 30),
                        RuleSetName = ruleSetName,
                        HitCount = (short)numberOfMatches,
                        PositionOfMatch = short.Parse(firstRow.Substring(32, 4)),
                        LengthOfMatch = short.Parse(firstRow.Substring(36, 4)),
                        AccuracyOfMatch = short.Parse(firstRow.Substring(40, 3)),
                        MatchablePattern = firstRow.Substring(43, 700),
                        MiDesc = firstRow.Substring(743, 700),
                        Category = firstRow.Substring(1443, 400),
                        MiNotes = firstRow.Substring(1843, 2000),
                        BusinessUnit = firstRow.Substring(3843, 20),
                        BusinessUnitType = firstRow.Substring(3863, 3),
                        LastUpdateDatePattern = firstRow.Substring(3866, 10),
                        CitiscreeningSendField = sendField,
                        CitiscreeningSendFieldValue = sendFieldValue
                    });

            if (bodyLines.Any())
                for (int i = 0; i < bodyLines.Count(); i++)
                {
                    citiscreeningPendingCompositions.Add(
                    new CitiscreeningPendingComposition
                    {
                        TxnId = firstRow.Substring(0, 30),
                        RuleSetName = ruleSetName,
                        HitCount = (short)numberOfMatches,
                        PositionOfMatch = short.Parse(bodyLines[i].Substring(0, 4)),
                        LengthOfMatch = short.Parse(bodyLines[i].Substring(4, 4)),
                        AccuracyOfMatch = short.Parse(bodyLines[i].Substring(8, 3)),
                        MatchablePattern = bodyLines[i].Substring(11, 700),
                        MiDesc = bodyLines[i].Substring(711, 700),
                        Category = bodyLines[i].Substring(1411, 400),
                        MiNotes = bodyLines[i].Substring(1811, 2000),
                        BusinessUnit = bodyLines[i].Substring(3811, 20),
                        BusinessUnitType = bodyLines[i].Substring(3831, 3),
                        LastUpdateDatePattern = bodyLines[i].Substring(3834, 10),
                        CitiscreeningSendField = sendField,
                        CitiscreeningSendFieldValue = sendFieldValue,
                    });
                }

            return citiscreeningPendingCompositions;
        }

        #endregion

        #region TrandeFlexCube

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        private void CreateTrandeFlexCubeFile(IEnumerable<Transaction> listaTransactions, PostAccountingFlexcubeConfiguration configuration, int currencyId, string userSoeid, int countryId, Guid newGuid, int currentCulture, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile, ContextoPrincipal contexto)
        {
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;

            try
            {
                bool localCurrency = false;

                if (listaTransactions.All(p => p.StatusId == TransactionState.TrandeGenerationOK))
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.NothingToWriteInATrandeFile.GetDescription(), null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInATrandeFile, EventType.FlexcubeFile));
                    return;
                }

                var currenyOriginal = contexto.Currency.FirstOrDefault(p => p.Id == currencyId);

                if (currenyOriginal.Code.Trim() != "USD")
                {
                    localCurrency = true;
                }

                nombreArchivo = string.Format("TrandeFlexcube{0:yyMMdd_hhmmss}.txt", DateTime.Now);

                if (!Directory.Exists(configuration.OutputFilePath))
                {
                    throw new AplicacionExcepcion(currentCulture == 0 ? string.Format("Can not create the file because the path: {0} not exist", configuration.OutputFilePath) : string.Format("No se puede crear el archivo ya que no existe el directorio: {0}", configuration.OutputFilePath));
                }

                rutaArchivo = Path.Combine(configuration.OutputFilePath, nombreArchivo);

                string header = GetHeaderTrandeFlexCubeFile(configuration, listaTransactions);
                string sumatory = GetSumatoryTrandeFlexCubeFile(listaTransactions, configuration, localCurrency);

                using (StreamWriter TrandeFlexCubeFile = new StreamWriter(rutaArchivo, true))
                {
                    TrandeFlexCubeFile.WriteLine(header);

                    foreach (var transaction in listaTransactions)
                    {
                        string line = GetDetailTrandeFlexCubeFile(transaction, configuration, localCurrency);
                        TrandeFlexCubeFile.WriteLine(line);
                    }

                    TrandeFlexCubeFile.WriteLine(sumatory);
                }

                byte[] bytes = System.IO.File.ReadAllBytes(rutaArchivo);

                IngresarEventoTrandeFlexCubeFileOK(listaTransactions, nombreArchivo, bytes, userSoeid);

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.TrandeFileSucessfullyGenerated.GetDescription(), null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.TrandeFileSucessfullyGenerated, EventType.FlexcubeFile));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ex.Message, null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

                IngresarEventoTrandeFlexCubeFileError(listaTransactions, nombreArchivo, null, ex.Message, userSoeid);

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        private void IngresarEventoTrandeFlexCubeFileOK(IEnumerable<Transaction> transactions, string fileName, byte[] file, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.TrandeGenerationError;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {                    
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        File = file,
                        Date = DateTime.Now,
                        EventTypeId = EventType.FlexcubeFile,
                        EventStateId = EventState.Complete,
                        StatusId = TransactionState.TrandeGenerationOK,
                        UserSoeid = userSoeid
                    }
                });


            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        private void IngresarEventoTrandeFlexCubeFileError(IEnumerable<Transaction> transactions, string fileName, byte[] file, string message, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.TrandeGenerationError;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {                    
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        File = file,
                        Date = DateTime.Now,
                        EventTypeId = EventType.FlexcubeFile,
                        EventStateId = EventState.Incomplete,
                        StatusId = TransactionState.TrandeGenerationError,
                        MessageReceived = message,
                        UserSoeid = userSoeid
                    }
                });
            }
        }

        /// <summary>
        /// Da formato a la linea generada, si el string enviado exede el numChar, este sera cortado dependiendo de la variable align
        /// </summary>
        /// <param name="align">alineacion de la cadena de retorno</param>
        /// <param name="str">string que se desea evaluar</param>
        /// <param name="numChar">cantidad de caracteres que debe tener la cadena de retorno</param>
        /// <param name="escapeChar">caracter especial con cual debe ser llenado el espacio sobrante en la cadena</param>
        /// <returns>linea formateada</returns>
        private string SetFieldFormat(Align align, string str, int numChar, char escapeChar)
        {
            if (!string.IsNullOrEmpty(str) || str.Length != numChar)
            {
                if (str.Length < numChar)
                {
                    if (align == Align.Left)
                    {
                        str = str.PadLeft(numChar, escapeChar);
                    }
                    else if (align == Align.Right)
                    {
                        str = str.PadRight(numChar, escapeChar);
                    }
                }
                else
                {
                    str = str.Length < numChar ? str.Trim() : str;

                    if (align == Align.Left)
                    {
                        str = str.Remove(numChar - 1, str.Length - numChar);
                    }
                    else if (align == Align.Right)
                    {
                        str = str == "" ? string.Empty : str.Remove(0, str.Length - numChar);
                    }
                }
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileOriginal"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private string GetHeaderTrandeFlexCubeFile(PostAccountingFlexcubeConfiguration configuration, IEnumerable<Transaction> transactions)
        {
            try
            {
                string qREC = transactions.ToList().Count.ToString();
                decimal totalCredito = transactions.Where(t => t.NatureTransactionId == NatureTransaction.Credit).Sum(p => p.Amount);
                decimal totalDebito = transactions.Where(t => t.NatureTransactionId == NatureTransaction.Debit).Sum(p => p.Amount);
                string TotalAmountDebitTransactions = string.Format("{0:000000000000.00;(000000000000.00);zero}", (totalDebito + totalCredito).ToString(CultureInfo.GetCultureInfo("en-US")));
                string TotalAmountCreditTransactions = string.Format("{0:000000000000.00;(000000000000.00);zero}", (totalDebito + totalCredito).ToString(CultureInfo.GetCultureInfo("en-US")));

                string numberOfTransactions = (transactions.ToList().Count + 1).ToString();
                string result = string.Empty;

                if (configuration.ProductCode.Length < 3)
                    result += configuration.ProductCode + new string(' ', 3 - configuration.ProductCode.Length);
                else
                    result += SetFieldFormat(Align.Left, configuration.ProductCode, 3, ' ');

                result += DateTime.Today.ToString("yyMMdd");

                if (numberOfTransactions.Length < 4)
                    result += new string(' ', 4 - numberOfTransactions.Length) + numberOfTransactions;
                else
                    result += SetFieldFormat(Align.Left, numberOfTransactions, 4, ' ');

                if (TotalAmountDebitTransactions.Length < 15)
                    result += new string('0', 15 - TotalAmountDebitTransactions.Length) + TotalAmountDebitTransactions;
                else
                    result += SetFieldFormat(Align.Right, TotalAmountDebitTransactions, 15, '0');

                if (TotalAmountCreditTransactions.Length < 15)
                    result += new string('0', 15 - TotalAmountCreditTransactions.Length) + TotalAmountCreditTransactions;
                else
                    result += SetFieldFormat(Align.Right, TotalAmountCreditTransactions, 15, '0');

                if (configuration.TrandeUserName.Length < 7)
                    result += configuration.TrandeUserName.PadRight(7, ' ');
                else
                    result += SetFieldFormat(Align.Left, configuration.TrandeUserName.Substring(0, 7), 7, ' ');

                if (transactions.FirstOrDefault().Batch.File.ReferenceCode.Length < 8)
                    result += transactions.FirstOrDefault().Batch.File.ReferenceCode + new string(' ', 8 - transactions.FirstOrDefault().Batch.File.ReferenceCode.Length);
                else
                    result += SetFieldFormat(Align.Right, transactions.FirstOrDefault().Batch.File.ReferenceCode, 8, ' ');

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private string GetSumatoryTrandeFlexCubeFile(IEnumerable<Transaction> transactions, PostAccountingFlexcubeConfiguration configuration, bool localCurrency)
        {
            try
            {
                string AccountNumber;
                string currency;
                string AmountLocalCurrency;
                string AmountForeignCurrency;
                string DRorCR;
                string FecSegTranOriginal;
                string Rate;
                string transactionCode;
                string ReferenceNumber;
                string APA;
                string MisCurrency;
                string MisSpread;
                string NumeroRastreo;
                string Constant1;
                string Constant2;
                string Constant3;
                string NomOriginador;
                string Constant4;
                string DesPropTransaccion;
                string Constant5;
                string InfRelPago;

                decimal totalCredito = transactions.Where(t => t.NatureTransactionId == NatureTransaction.Credit).Sum(p => p.Amount);
                decimal totalDebito = transactions.Where(t => t.NatureTransactionId == NatureTransaction.Debit).Sum(p => p.Amount);
                string TotalAmountDebitTransactions = string.Format("{0:000000000000.00;(000000000000.00);zero}", totalDebito.ToString(CultureInfo.GetCultureInfo("en-US")));
                string TotalAmountCreditTransactions = string.Format("{0:000000000000.00;(000000000000.00);zero}", totalCredito.ToString(CultureInfo.GetCultureInfo("en-US")));
                decimal totalSumatoria = 0;

                if (totalDebito == 0)
                {
                    DRorCR = "0";
                    transactionCode = "862";
                    totalSumatoria = totalCredito;
                }
                else if (totalCredito == 0)
                {
                    DRorCR = "1";
                    transactionCode = "868";
                    totalSumatoria = totalDebito;
                }
                else
                {
                    totalSumatoria = totalCredito - totalDebito;

                    if (totalSumatoria < 0)
                    {
                        DRorCR = "1";
                        transactionCode = "868";
                    }
                    else
                    {
                        DRorCR = "0";
                        transactionCode = "862";
                    }
                }

                AccountNumber = "165250128";
                currency = "000";

                if (localCurrency)
                {
                    AmountLocalCurrency = string.Format("{0:000000000000.00;(000000000000.00);zero}", totalSumatoria.ToString(CultureInfo.GetCultureInfo("en-US")));
                    AmountForeignCurrency = "000000000000.00";
                }
                else
                {
                    AmountLocalCurrency = "000000000000.00";
                    AmountForeignCurrency = string.Format("{0:000000000000.00;(000000000000.00);zero}", totalSumatoria.ToString(CultureInfo.GetCultureInfo("en-US")));
                }

                DateTime fecha = (DateTime)transactions.LastOrDefault().OriginalTransactionDate;
                FecSegTranOriginal = fecha.ToString("yyMMdd");
                //AccountNumber = "165250128";
                //currency = "000";
                //Rate = "0000.0000";
                //ReferenceNumber = "0";
                //APA = "8110";
                //MisCurrency = "000";
                //MisSpread = "0000.0000";
                //NumeroRastreo = "101010600000000";
                //Constant1 = string.Empty;
                //Constant2 = "~";
                //Constant3 = "P/C de";
                //Constant4 = string.Empty;
                //Constant5 = string.Empty;
                AccountNumber = configuration.AccountNumber;
                currency = configuration.Currency;
                Rate = configuration.Rate.ToString("0000.0000", CultureInfo.GetCultureInfo("en-US"));
                ReferenceNumber = "0";
                APA = configuration.APA;
                MisCurrency = configuration.MisCurrency;
                MisSpread = configuration.MisSpread.ToString("0000.0000", CultureInfo.GetCultureInfo("en-US"));
                NumeroRastreo = "101010600000000";
                Constant1 = configuration.Constant1 == null ? string.Empty : configuration.Constant1;
                Constant2 = configuration.Constant2 == null ? string.Empty : configuration.Constant2;
                Constant3 = configuration.Constant3 == null ? string.Empty : configuration.Constant3;
                Constant4 = configuration.Constant4 == null ? string.Empty : configuration.Constant4;
                Constant5 = configuration.Constant5 == null ? string.Empty : configuration.Constant5;
                InfRelPago = string.Empty;
                NomOriginador = string.Empty;
                DesPropTransaccion = string.Empty;


                System.Text.StringBuilder result = new System.Text.StringBuilder();

                result.Append(SetFieldFormat(Align.Right, AccountNumber, 10, ' '));
                result.Append(SetFieldFormat(Align.Left, currency, 3, '0'));
                result.Append(SetFieldFormat(Align.Left, AmountLocalCurrency, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, AmountForeignCurrency, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, DRorCR, 1, ' '));
                result.Append(FecSegTranOriginal);
                result.Append(SetFieldFormat(Align.Left, Rate, 9, ' '));
                result.Append(SetFieldFormat(Align.Left, transactionCode, 6, '0'));
                result.Append(SetFieldFormat(Align.Left, ReferenceNumber, 10, ' '));
                result.Append(SetFieldFormat(Align.Left, APA, 6, ' '));
                result.Append(SetFieldFormat(Align.Left, MisCurrency, 3, '0'));
                result.Append(SetFieldFormat(Align.Left, MisSpread, 9, ' '));
                result.Append(SetFieldFormat(Align.Left, NumeroRastreo, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, Constant1, 21, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant2, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, Constant3, 7, ' '));
                result.Append(SetFieldFormat(Align.Right, NomOriginador, 16, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant4, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, DesPropTransaccion, 14, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant5, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, InfRelPago, 80, ' '));

                return result.ToString();
            }
            catch
            {
                throw;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private string GetDetailTrandeFlexCubeFile(Transaction transaction, PostAccountingFlexcubeConfiguration configuration, bool localCurrency)
        {
            try
            {

                string AccountNumber;
                string currency;
                string AmountLocalCurrency;
                string AmountForeignCurrency;
                string DRorCR;
                string FecSegTranOriginal;
                string Rate;
                string transactionCode;
                string ReferenceNumber;
                string APA;
                string MisCurrency;
                string MisSpread;
                string NumeroRastreo;
                string Constant1;
                string Constant2;
                string Constant3;
                string NomOriginador;
                string Constant4;
                string DesPropTransaccion;
                string Constant5;
                string InfRelPago;

                AccountNumber = transaction.AccountNumber;
                //currency = "000";
                currency = configuration.Currency;

                if (localCurrency)
                {
                    AmountLocalCurrency = string.Format("{0:000000000000.00;(000000000000.00);zero}", transaction.Amount.ToString(CultureInfo.GetCultureInfo("en-US")));
                    AmountForeignCurrency = "000000000000.00";
                }
                else
                {
                    AmountLocalCurrency = "000000000000.00";
                    AmountForeignCurrency = string.Format("{0:000000000000.00;(000000000000.00);zero}", transaction.Amount.ToString(CultureInfo.GetCultureInfo("en-US")));
                }

                if (transaction.NatureTransactionId == NatureTransaction.Credit)
                {
                    DRorCR = "1";
                }
                else
                {
                    DRorCR = "0";
                }

                var transactionCodeNacha = int.Parse(transaction.TransactionCode);

                FecSegTranOriginal = (transaction.OriginalTransactionDate.HasValue) ? ((DateTime)transaction.OriginalTransactionDate).ToString("yyMMdd") : "";
                //Rate = "0000.0000";
                Rate = configuration.Rate.ToString("0000.0000", CultureInfo.GetCultureInfo("en-US"));

                transactionCode = transaction.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCodeNacha).MappingCode;
                ReferenceNumber = transaction.PepBankTransactionId.ToString();
                //APA = "8110";
                //MisCurrency = "000";
                MisCurrency = configuration.MisCurrency;
                APA = configuration.APA;
                //MisSpread = "0000.0000";
                MisSpread = configuration.MisSpread.ToString("0000.0000", CultureInfo.GetCultureInfo("en-US"));
                NumeroRastreo = transaction.TrackingNumberId;
                //Constant1 = string.Empty;
                //Constant2 = "~";
                //Constant3 = "P/C de";
                //Constant4 = string.Empty;
                //Constant5 = string.Empty;
                Constant1 = configuration.Constant1 == null ? string.Empty : configuration.Constant1;

                Constant2 = configuration.Constant2 == null ? string.Empty : configuration.Constant2.FirstOrDefault().ToString();

                Constant3 = configuration.Constant3 == null ? string.Empty : configuration.Constant3;
                Constant4 = configuration.Constant4 == null ? string.Empty : configuration.Constant4;
                Constant5 = configuration.Constant5 == null ? string.Empty : configuration.Constant5;

                NomOriginador = transaction.Batch.Originator;
                DesPropTransaccion = transaction.Batch.DescriptonPurposeTransaction;
                InfRelPago = transaction.Addenda;


                System.Text.StringBuilder result = new System.Text.StringBuilder();

                result.Append(SetFieldFormat(Align.Right, AccountNumber, 10, ' '));
                result.Append(SetFieldFormat(Align.Left, currency, 3, '0'));
                result.Append(SetFieldFormat(Align.Left, AmountLocalCurrency, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, AmountForeignCurrency, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, DRorCR, 1, ' '));
                result.Append(FecSegTranOriginal);
                result.Append(SetFieldFormat(Align.Left, Rate, 9, ' '));
                result.Append(SetFieldFormat(Align.Left, transactionCode, 6, '0'));
                result.Append(SetFieldFormat(Align.Left, ReferenceNumber, 10, ' '));
                result.Append(SetFieldFormat(Align.Left, APA, 6, ' '));
                result.Append(SetFieldFormat(Align.Left, MisCurrency, 3, '0'));
                result.Append(SetFieldFormat(Align.Left, MisSpread, 9, ' '));
                result.Append(SetFieldFormat(Align.Left, NumeroRastreo, 15, '0'));
                result.Append(SetFieldFormat(Align.Left, Constant1, 21, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant2, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, Constant3, 7, ' '));
                result.Append(SetFieldFormat(Align.Right, NomOriginador, 16, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant4, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, DesPropTransaccion, 14, ' '));
                result.Append(SetFieldFormat(Align.Left, Constant5, 1, ' '));
                result.Append(SetFieldFormat(Align.Right, InfRelPago, 80, ' '));

                return result.ToString();
            }
            catch
            {
                throw;
            }
        }


        #endregion

        #region Post Accounting Connection

        /// <summary>
        /// Método que inicializa la conexion Open Batch a Cosmos
        /// </summary>
        /// <param name="queueConnectivity"></param>
        private void InitializePostAccountingConnection(PostAccountingConfiguration queueConnectivity)
        {
            this.messageQueueFactory = new MessageQueueFactory(new WebsphereMessageQueue());

            this.messageQueueFactory.Initialization(new MessageQueueSpecs()
            {
                Channel = queueConnectivity.Channel,
                QueueManager = queueConnectivity.QueueManager,
                ListenQueue = queueConnectivity.ListenQueue,
                RemoteQueue = queueConnectivity.RemoteQueue,
                ReplyQueue = queueConnectivity.ReplyQueue,
                ReplyQueueManager = queueConnectivity.ReplyQueueManager,
                HostName = queueConnectivity.DNS,
                Attempts = queueConnectivity.Attempts.Value,
                Intervals = Convert.ToInt32(queueConnectivity.Intervals.Value.TotalSeconds),
                MessagePattern = MessagePattern.RequestResponse,
                AccessTypeGet = AccessType.MQOO_INPUT_SHARED,
                AccessTypePut = AccessType.MQOO_OUTPUT
            });
        }

        #endregion

        #region Open Batch

        /// <summary>
        /// Método que inicia el proceso Open Batch y obtiene el xml del mensaje
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        private void IniciarProcesoOpenBatch(Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, TransactionTypeBatchConfiguration batchConfiguration, int currentCulture, Guid newGuid, Dominio.Core.File fileOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile, ContextoPrincipal contexto)
        {          
            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active).ToList();

            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xmlString = DominioOpenBatchLogica.GetOpenBatchXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, batchConfiguration);

            EnvioTransaccionOpenBatch(xmlString, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal, currencyId, countryOriginal, listProcessingLog, adaptadorFile);            
        }

        /// <summary>
        /// Método que envia y valida el envio del Open Batch
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="countryOriginal"></param>
        private void EnvioTransaccionOpenBatch(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, Dominio.Core.File fileOriginal, int currencyId, Country countryOriginal, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                if (codeReturn == 0)
                {
                    var xdoc = XDocument.Parse(replyMessage);

                    if (xdoc.Descendants("RE_BATCH").Any(p => p.Element("ERROR") != null))
                    {
                        var error = xdoc.Descendants("RE_BATCH").Select(p => p.Element("ERROR")).FirstOrDefault();

                        var codigoError = error.Element("ECODE").Value;
                        var descripcion = error.Element("EDESC").Value;

                        if (codigoError == "50")
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchAlreadyOpened.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchAlreadyOpened, EventType.OpenBatch));

                            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id, currencyId);

                            batchConfiguration.IsOpen = true;
                            batchConfiguration.UserOpenedBatch = userSoeid;
                            batchConfiguration.OpenDate = DateTime.Now;
                        }
                        else
                        {
                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                            InsertErrorMessageOpenBatchCodeReturn0(xmlMessage, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal.Id, currencyId, replyMessage, codigoError, descripcion);
                        }
                    }
                    else
                    {
                        batchConfiguration.IsOpen = true;
                        batchConfiguration.UserOpenedBatch = userSoeid;
                        batchConfiguration.OpenDate = DateTime.Now;

                        listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchSucessfully.GetDescription(), "0", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchSucessfully, EventType.OpenBatch));
                        IngresarEventoTransaccionOpenBatchOK(batchConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);                        
                    }
                }
                else
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codeReturn.ToString(), fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    InsertErrorMessageOpenBatchCodeReturnNo0(xmlMessage, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal.Id, currencyId, replyMessage, codeReturn);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="replyMessage"></param>
        /// <param name="codeReturn"></param>
        private void InsertErrorMessageOpenBatchCodeReturnNo0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, int fileId, int currencyId, string replyMessage, int codeReturn)
        {
            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, codeReturn, null, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId, currencyId);

            var internErrorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (internErrorMessage != null)
            {
                var mensaje = currentCulture == 0 ? String.Format("There was an error when try to open the batch: {0}", internErrorMessage.EnglishText) : String.Format("Hubo un error cuando se trato de abrir el batch: {0}", internErrorMessage.SpanishText);                

                throw new AplicacionExcepcion(mensaje);
            }
            else
            {
                var mensaje = currentCulture == 0 ? string.Format("There was an error when try to open the batch. Contact with your administrator. Error Code : {0}", codeReturn) : string.Format("Hubo un error cuando se trato de abrir el batch. Contactese con su administrador. Código Error : {0}", codeReturn);

                throw new AplicacionExcepcion(mensaje);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="replyMessage"></param>
        /// <param name="codigoError"></param>
        /// <param name="descripcion"></param>
        private void InsertErrorMessageOpenBatchCodeReturn0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, int fileId, int currencyId, string replyMessage, string codigoError, string descripcion)
        {
            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId, currencyId);

            var internErrorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == int.Parse(codigoError)).FirstOrDefault();

            if (internErrorMessage != null)
            {
                var mensaje = currentCulture == 0 ? String.Format("There was an error when try to open the batch: {0}", internErrorMessage.EnglishText) : String.Format("Hubo un error cuando se trato de abrir el batch: {0}", internErrorMessage.SpanishText);

                throw new AplicacionExcepcion(mensaje);
            }
            else
            {
                var mensaje = currentCulture == 0 ? string.Format("There was an error when try to open the batch. Contact with your administrator. Error Code : {0}", codigoError) : string.Format("Hubo un error cuando se trato de abrir el batch. Contactese con su administrador. Código Error : {0}", codigoError);

                throw new AplicacionExcepcion(mensaje);
            }
        }

        /// <summary>
        /// Agrega el codigo del batch que tuvo problemas
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveCriticalBatch(int batchId)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["BatchCodeProblem"] != null)
                {
                    var listaBatchCodeProblem = (List<string>)HttpContext.Current.Session["BatchCodeProblem"];

                    listaBatchCodeProblem.Add(batchId.ToString());
                }
                else
                {
                    var newListFileName = new List<string>() { batchId.ToString() };

                    HttpContext.Current.Session["BatchCodeProblem"] = newListFileName;
                }
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Open Batch) desde Cosmos
        /// </summary>
        /// <param name="batchConfiguration"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="fileId"></param>
        private void IngresarEventoTransaccionOpenBatchOK(TransactionTypeBatchConfiguration batchConfiguration, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, int fileId)
        {
            if (batchConfiguration.BatchConfigurationBatchEventOnlineLogs == null)
                batchConfiguration.BatchConfigurationBatchEventOnlineLogs = new HashSet<BatchConfigurationBatchEventOnlineLog>();

            batchConfiguration.BatchConfigurationBatchEventOnlineLogs.Add(new BatchConfigurationBatchEventOnlineLog
            {
                FileId = fileId,
                BatchEventOnlineLog = new BatchEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.OpenBatch,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid
                }
            });
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Open Batch) desde Cosmos
        /// </summary>
        /// <param name="batchConfiguration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionOpenBatchError(TransactionTypeBatchConfiguration batchConfiguration, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, int fileId, int currencyId)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (batchConfiguration.BatchConfigurationBatchEventOnlineLogs == null)
                batchConfiguration.BatchConfigurationBatchEventOnlineLogs = new HashSet<BatchConfigurationBatchEventOnlineLog>();

            batchConfiguration.BatchConfigurationBatchEventOnlineLogs.Add(new BatchConfigurationBatchEventOnlineLog
            {
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                FileId = fileId,
                BatchEventOnlineLog = new BatchEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.OpenBatch,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid,
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Open Batch Process. Batch Id : {0} - Return Code: {1}", batchConfiguration.Id, codeReturn)), "PoliticaPostAccountingOpenBatchError");
            }

        }

        /// <summary>
        /// Método que valida en el caso de que existan excepciones en el proceso Open Batch
        /// </summary>
        /// <param name="listaExcepcion"></param>    
        private void ValidarExcepcionesOpenBatch(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Count > 0)
            {
                var groupList = (from c in listaExcepcion
                                 orderby c.GetType()
                                 select c).GroupBy(g => g.GetType()).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingOpenBatchError");
                });

                throw groupList.FirstOrDefault();
            }
        }

        #endregion               

        #region Upload

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="listaTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesUpload(IEnumerable<Transaction> listaTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var postAccountingMode = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var transactionFilterStateNoUploadToCosmos = listaTransaction.Where(c => c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.Import);
            var transactionFilterStateUploadError = listaTransaction.Where(c => c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

            var statusId = transactionFilterStateNoUploadToCosmos.Any() ? TransactionState.Import : (transactionFilterStateUploadError.Any() ? TransactionState.UploadToCosmosRejected : TransactionState.Import);

            var listaSplitedStateNoUploadToCosmos = transactionFilterStateNoUploadToCosmos.Any() ? transactionFilterStateNoUploadToCosmos.Split(maxTransactionNumber) : null;
            var listaSplitedStateErrorUploadToCosmos = transactionFilterStateUploadError.Any() ? transactionFilterStateUploadError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : null;

            switch (statusId)
            {
                case TransactionState.Import:
                    foreach (var item in listaSplitedStateNoUploadToCosmos ?? new List<List<Transaction>>())
                    {
                        if (item.Any())
                            ValidarTransaccionUploadStateNoUploadToCosmos(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                    }
                    break;
                case TransactionState.UploadToCosmosRejected:
                    foreach (var item in listaSplitedStateErrorUploadToCosmos ?? new List<List<Transaction>>())
                    {
                        if (item.Any())
                            ValidarTransaccionUploadStateUploadToCosmos(item, listaTransaction, newGuid, countryOriginal, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                    }
                    break;
            }         
        }

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que sea la primera vez que se envian a cosmos
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionUploadStateNoUploadToCosmos(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioUploadLogica.GetXmlDeclaration(uniqueKey);

            var identificadorTransactionType = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType.Identifier;
            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;
            var transactionTypeBatchConfiguration = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && p.Id == batchId);

            foreach (var transaction in splitList)
            {
                transaction.StatusId = TransactionState.UploadToCosmos;

                DominioUploadLogica.GetUploadMessageByTransactionType(countryOriginal, configuration, xDeclaration, transaction, identificadorTransactionType, postAccountingMode, cosmosFunctionalUser, transactionTypeBatchConfiguration);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionUploadResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
        }

        /// <summary>
        /// Método que obtiene los xml agrupados por el Unique key que ya fue enviado la primera vez
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionUploadStateUploadToCosmos(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Upload && (p.EventOnlineLog.StatusId == TransactionState.UploadToCosmos || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosRejected || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosHoldingAccountError))).OrderByDescending(p => p.EventOnlineLog.Date).Select(p => p.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            foreach (var eventOnline in eventOnlineGroupByUniqueKey)
            {
                var listTransaction = eventOnline.ListEventOnlineLog.SelectMany(p => p.TransactionEventOnlineLogs).Select(p => p.Transaction);

                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionUploadResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
            }
        }

        /// <summary>
        /// Método que valida dependiendo del tipo de posteo realizado, el tipo de confirmación de respuesta que se debe realizar
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void ValidarTransactionUploadResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            try
            {
                if (codeReturn == 0)
                {
                    switch (postAccountingMode)
                    {
                        case PostAccountMode.ClientAccount:
                            ValidarTransactionUploadResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                            break;
                        case PostAccountMode.HoldingAccount:
                            ValidarTransactionUploadResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                            //SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                            break;
                        case PostAccountMode.Both:
                            ValidarTransactionUploadResponseClientBoth(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                            break;
                    }
                }
                else
                {
                    listTransaction.ForEach(p => IngresarEventoTransaccionUploadError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId));
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Método que valida la respuesta para cuando el xml de envio se realizo con Cuenta Cliente y Cuenta Puente
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void ValidarTransactionUploadResponseClientBoth(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {          
            var xmlReply = XDocument.Parse(replyMessage);

            var xmlAnswersCount = xmlReply.Descendants("REPLY_ACCTXN").Count() / 2;

            var listCount = listTransaction.Count();
            var transactionCounter = 0;

            if (listCount == xmlAnswersCount)
            {
                var xmlSplit = SplitXmlFile.SplitXml(replyMessage, "FCC_AC_SERVICE", "REPLY_ACCTXN", 2);

                foreach (var item in xmlSplit)
                {
                    var transaction = listTransaction[transactionCounter];
                    var errorTrxOriginal = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Element("TXNFLAG").Value == "F" ? true : false;
                    var errorTrxHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Element("TXNFLAG").Value == "F" ? true : false;

                    if (!errorTrxOriginal && !errorTrxHoldingAccount)
                    {
                        if (transaction.StatusId == TransactionState.UploadToCosmos || transaction.StatusId == TransactionState.UploadToCosmosClientAccountError || transaction.StatusId == TransactionState.UploadToCosmosHoldingAccountError)
                        {
                            transaction.StatusId = TransactionState.UploadToCosmosButNotAuthorized;
                            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
                            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

                            IngresarEventoTransaccionUploadOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                        }
                    }
                    else
                    {
                        if (errorTrxOriginal && !errorTrxHoldingAccount)
                        {
                            ProcessUploadErrorOriginalTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                        else if (!errorTrxOriginal && errorTrxHoldingAccount)
                        {
                            ProcessUploadErrorHoldingAccountTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                        else
                        {
                            ProcessUploadErrorRejectedByCosmos(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Upload));
            }            
        }

        /// <summary>
        /// Método que valida los errores RejectedByCosmos del proceso Upload
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessUploadErrorRejectedByCosmos(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.UploadToCosmosRejected;

            int codeRejected = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejected = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, codeRejected, descriptionRejected, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);

            int codeRejectedHoldingAccount = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejectedHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, codeRejectedHoldingAccount, descriptionRejectedHoldingAccount, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que valida los errores asociados al holding account del proceso Upload
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessUploadErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.UploadToCosmosHoldingAccountError;

            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que valida los errores asociados al client account del proceso Upload
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessUploadErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.UploadToCosmosClientAccountError;

            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que valida la respuesta para cuando el xml de envio se realizo solamente con Cuenta Cliente 
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void ValidarTransactionUploadResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {         
            var xmlReply = XDocument.Parse(replyMessage);

            var xmlAnswersCount = xmlReply.Descendants("REPLY_ACCTXN").Count();

            var listCount = listTransaction.Count();
            var transactionCounter = 0;

            if (listCount == xmlAnswersCount)
            {
                var xmlSplit = SplitXmlFile.SplitXml(replyMessage, "FCC_AC_SERVICE", "REPLY_ACCTXN", 1);

                foreach (var item in xmlSplit)
                {
                    var transaction = listTransaction[transactionCounter];

                    var errorNode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").Any();

                    if (errorNode)
                    {
                        int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
                        var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

                        transaction.StatusId = TransactionState.UploadToCosmosRejected;

                        IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
                    }
                    else
                    {
                        if (transaction.StatusId == TransactionState.UploadToCosmos || transaction.StatusId == TransactionState.UploadToCosmosClientAccountError || transaction.StatusId == TransactionState.UploadToCosmosHoldingAccountError)
                        {
                            transaction.StatusId = TransactionState.UploadToCosmosButNotAuthorized;

                            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

                            IngresarEventoTransaccionUploadOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Upload));
            }            
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Upload) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionUploadOK(Transaction transaction, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Upload,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Upload) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionUploadError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, Country countryOriginal, int currencyId)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Upload,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Upload Process. TransactionId: {0} - Return Code: {1} ", transaction.Id, codeReturn)), "PoliticaPostAccountingUploadError");
            }
        }
        
        #endregion

        #region Authorize

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesAuthorize(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var postAccountingMode = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var transactionFilterStateNoAuthorize = listTransaction.Where(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized);
            var transactionFilterStateAuthorizeError = listTransaction.Where(c => c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));            

            var statusId = transactionFilterStateNoAuthorize.Any() ? TransactionState.UploadToCosmosButNotAuthorized : (transactionFilterStateAuthorizeError.Any() ? TransactionState.AuthorizeByCosmosRejected : TransactionState.UploadToCosmosButNotAuthorized);

            var listaSplitedStateAuthorizeToCosmosNotAuthorized = transactionFilterStateNoAuthorize.Count() > 0 ? transactionFilterStateNoAuthorize.Split(maxTransactionNumber) : null;
            var listaSplitedStateErrorAuthorizeToCosmos = transactionFilterStateAuthorizeError.Count() > 0 ? transactionFilterStateAuthorizeError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : null;

            switch (statusId)
            {
                case TransactionState.UploadToCosmosButNotAuthorized:
                    foreach (var item in listaSplitedStateAuthorizeToCosmosNotAuthorized ?? new List<List<Transaction>>())
                    {
                        if (item.Any())
                            ValidarTransaccionAuthorizeStateNotAuthorized(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                    }
                    break;
                case TransactionState.AuthorizeByCosmosRejected:
                    foreach (var item in listaSplitedStateErrorAuthorizeToCosmos ?? new List<List<Transaction>>())
                    {
                        if (item.Any())
                            ValidarTransaccionAuthorizeStateAuthorize(item, listTransaction, newGuid, countryOriginal, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                    }
                    break;
            }           
        }

        /// <summary>
        /// Método que obtiene los xml agrupados por el Unique Key que ya fue enviado la primera vez
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionAuthorizeStateAuthorize(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Authorize && (p.EventOnlineLog.StatusId == TransactionState.Authorize || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))).OrderByDescending(p => p.EventOnlineLog.Date).Select(c => c.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            foreach (var eventOnline in eventOnlineGroupByUniqueKey)
            {                
                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionAuthorizeResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
            }
        }

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que sea la primera vez que se envian a cosmos
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionAuthorizeStateNotAuthorized(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioAuthorizeLogica.GetXmlDeclaration(uniqueKey);

            var identificadorTransactionType = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType.Identifier;
            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;
            var transactionTypeBatchConfiguration = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.Id == batchId);

            foreach (var transaction in splitList)
            {
                transaction.StatusId = TransactionState.Authorize;

                DominioAuthorizeLogica.GetAuthorizeMessageByTransactionType(countryOriginal, configuration, xDeclaration, transaction, identificadorTransactionType, postAccountingMode, cosmosFunctionalUser, transactionTypeBatchConfiguration);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionAuthorizeResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);

        }

        /// <summary>
        /// Método que valida dependiendo del tipo de posteo realizado, el tipo de confirmación de respuesta que se debe realizar
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void ValidarTransactionAuthorizeResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            if (codeReturn == 0)
            {
                switch (postAccountingMode)
                {
                    case PostAccountMode.ClientAccount:
                        ValidarTransactionAuthorizeResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                        break;
                    case PostAccountMode.HoldingAccount:
                        ValidarTransactionAuthorizeResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                        //SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                        break;
                    case PostAccountMode.Both:
                        ValidarTransactionAuthorizeResponseClientBoth(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                        break;
                }
            }
            else
            {
                listTransaction.ForEach(p => IngresarEventoTransaccionAuthorizeError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId));
            }
        }

        /// <summary>
        /// Método que valida la respuesta para cuando el xml de envio se realizo solamente con Cuenta Cliente 
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingFileLog"></param>
        private void ValidarTransactionAuthorizeResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {           
            var xmlReply = XDocument.Parse(replyMessage);

            var xmlAnswersCount = xmlReply.Descendants("REPLY_ACCTXN").Count();

            var listCount = listTransaction.Count();
            var transactionCounter = 0;

            if (listCount == xmlAnswersCount)
            {
                var xmlSplit = SplitXmlFile.SplitXml(replyMessage, "FCC_AC_SERVICE", "REPLY_ACCTXN", 1);

                foreach (var item in xmlSplit)
                {
                    var transaction = listTransaction[transactionCounter];

                    var errorNode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").Any();

                    if (errorNode)
                    {
                        int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
                        var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

                        transaction.StatusId = TransactionState.AuthorizeByCosmosRejected;

                        IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
                    }
                    else
                    {
                        if (transaction.StatusId == TransactionState.Authorize || transaction.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || transaction.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError)
                        {
                            transaction.StatusId = TransactionState.AuthorizeByCosmosOk;

                            IngresarEventoTransaccionAuthorizeOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Authorize));
            }           
        }

        /// <summary>
        /// Método que valida la respuesta para cuando el xml de envio se realizo con Cuenta Cliente y Cuenta Puente
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        private void ValidarTransactionAuthorizeResponseClientBoth(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {         
            var xmlReply = XDocument.Parse(replyMessage);

            var xmlAnswersCount = xmlReply.Descendants("REPLY_ACCTXN").Count() / 2;

            var listCount = listTransaction.Count();
            var transactionCounter = 0;

            if (listCount == xmlAnswersCount)
            {
                var xmlSplit = SplitXmlFile.SplitXml(replyMessage, "FCC_AC_SERVICE", "REPLY_ACCTXN", 2);

                foreach (var item in xmlSplit)
                {
                    var transaction = listTransaction[transactionCounter];
                    var errorTrxOriginal = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Element("TXNFLAG").Value == "F" ? true : false;
                    var errorTrxHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Element("TXNFLAG").Value == "F" ? true : false;

                    if (!errorTrxOriginal && !errorTrxHoldingAccount)
                    {
                        if (transaction.StatusId == TransactionState.Authorize || transaction.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || transaction.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError)
                        {
                            transaction.StatusId = TransactionState.AuthorizeByCosmosOk;
                            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
                            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

                            IngresarEventoTransaccionAuthorizeOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                        }
                    }
                    else
                    {
                        if (errorTrxOriginal && !errorTrxHoldingAccount)
                        {
                            ProcessAuthorizeErrorOriginalTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                        else if (!errorTrxOriginal && errorTrxHoldingAccount)
                        {
                            ProcessAuthorizeErrorHoldingAccountTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                        else
                        {
                            ProcessAuthorizeErrorRejectedByCosmos(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction, countryOriginal, currencyId);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Authorize));
            }            
        }

        /// <summary>
        /// Método que valida los errores asociados al client account del proceso Authorize
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessAuthorizeErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosClientAccountError;
            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Authorize) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionAuthorizeError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, Country countryOriginal, int currencyId)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Authorize,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Authorize Process. TransactionId: {0} - Return Code: {1} ", transaction.Id, codeReturn)), "PoliticaPostAccountingUploadError");
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Authorize) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionAuthorizeOK(Transaction transaction, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Authorize,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });
        }

        /// <summary>
        /// Método que valida los errores asociados al holding account del proceso Authorize
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessAuthorizeErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosHoldingAccountError;
            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que valida los errores RejectedByCosmos del proceso Authorize
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessAuthorizeErrorRejectedByCosmos(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosRejected;

            int codeRejected = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejected = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, codeRejected, descriptionRejected, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);

            int codeRejectedHoldingAccount = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejectedHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, codeRejectedHoldingAccount, descriptionRejectedHoldingAccount, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesDelete(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {         
            var postAccountingMode = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);
            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);
            var processConfiguration = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

            if (processConfiguration.PostAccountModeId.Value == PostAccountMode.ClientAccount)
            {
                DeleteProcessClientAccount(listTransaction, countryOriginal, configuration, userSoeid, batchId, newGuid, currencyId, listProcessingLog, adaptadorFile, maxTransactionNumber, errorMessages);
            }

            if (processConfiguration.PostAccountModeId.Value == PostAccountMode.Both)
            {
                DeleteProcessBoth(listTransaction, countryOriginal, configuration, userSoeid, batchId, newGuid, currencyId, listProcessingLog, adaptadorFile, maxTransactionNumber, errorMessages);
            }            
        }

        /// <summary>
        /// Método que realiza el proceso de eliminación en el caso de que el posteo a cosmos se realizo con Cuenta Cliente y Cuenta Puente
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        /// <param name="exceptions"></param>
        /// <param name="maxTransactionNumber"></param>
        /// <param name="errorMessages"></param>
        private void DeleteProcessBoth(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile, int maxTransactionNumber, IQueryable<ErrorMessage> errorMessages)
        {
            var transactionAuthorizeRejected = listTransaction.Where(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected);
            var transactionClientAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.UploadToCosmosClientAccountError));
            var transactionHoldingAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.UploadToCosmosHoldingAccountError));
            var transactionClientHoldingAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError));           

            var listaSplitedAuthorizeRejected = transactionAuthorizeRejected.Count() > 0 ? transactionAuthorizeRejected.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientAccountError = transactionClientAccountError.Count() > 0 ? transactionClientAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedHoldingAccountError = transactionHoldingAccountError.Count() > 0 ? transactionHoldingAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientHoldingAccountError = transactionClientHoldingAccountError.Count() > 0 ? transactionClientHoldingAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();            

            foreach (var item in listaSplitedAuthorizeRejected)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);

                    if (item.Any())
                        ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }

            foreach (var item in listaSplitedClientAccountError)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }

            foreach (var item in listaSplitedHoldingAccountError)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }

            foreach (var item in listaSplitedClientHoldingAccountError)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);

                    if (item.Any())
                        ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }         
        }

        /// <summary>
        /// Método que realiza el proceso de eliminación en el caso de que el posteo a cosmos se realizo solamente con Cuenta Cliente
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        /// <param name="exceptions"></param>
        /// <param name="maxTransactionNumber"></param>
        /// <param name="errorMessages"></param>
        private void DeleteProcessClientAccount(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile, int maxTransactionNumber, IQueryable<ErrorMessage> errorMessages)
        {
            var transactionAuthorizeRejected = listTransaction.Where(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected);
            var transactionClientAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError));           

            var listaSplitedAuthorizeRejected = transactionAuthorizeRejected.Count() > 0 ? transactionAuthorizeRejected.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientAccountError = transactionClientAccountError.Count() > 0 ? transactionClientAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();           

            foreach (var item in listaSplitedClientAccountError)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }

            foreach (var item in listaSplitedAuthorizeRejected)
            {
                if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                {
                    if (item.Any())
                        ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog, adaptadorFile);
                }
            }
        }       

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que haya ocurrido un error a nivel de Holding Account       
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionDeleteStateClientAccountError(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioDeleteLogica.GetXmlDeclaration(uniqueKey);

            var identificadorTransactionType = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType.Identifier;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;
            var transactionTypeBatchConfiguration = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.Id == batchId);

            foreach (var transaction in splitList)
            {
                DominioDeleteLogica.GetDeleteMessageClientHoldingAccountErrorByTransactionType(countryOriginal, configuration, xDeclaration, transaction, identificadorTransactionType, PostAccountMode.ClientAccount, cosmosFunctionalUser, transactionTypeBatchConfiguration);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosClientAccountError, countryOriginal, currencyId, listProcessingLog, adaptadorFile);

        }

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que haya ocurrido un error a nivel de Client Account
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionDeleteStateHoldingAccountError(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioDeleteLogica.GetXmlDeclaration(uniqueKey);

            var identificadorTransactionType = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType.Identifier;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;
            var transactionTypeBatchConfiguration = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.Id == batchId);

            foreach (var transaction in splitList)
            {
                DominioDeleteLogica.GetDeleteMessageClientHoldingAccountErrorByTransactionType(countryOriginal, configuration, xDeclaration, transaction, identificadorTransactionType, PostAccountMode.HoldingAccount, cosmosFunctionalUser, transactionTypeBatchConfiguration);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosHoldingAccountError, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
        }

        /// <summary>
        /// Método que realiza la validación de la respuesta del xml de Delete dependieno del tipo de envio que se realizo
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void ValidarTransactionDeleteResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, TransactionState deletedTransactionState, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            if (codeReturn == 0)
            {
                switch (postAccountingMode)
                {
                    case PostAccountMode.ClientAccount:
                        ValidarTransactionDeleteResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, deletedTransactionState, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
                        break;
                    case PostAccountMode.HoldingAccount:
                        //var xelementHoldingAccount = GetUploadXmlRequestIncomingHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                        //SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                        break;
                }
            }
            else
            {
                listTransaction.ForEach(p => IngresarEventoTransaccionDeleteError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId));
            }
        }

        /// <summary>
        /// Método que valida la respuesta para cuando el xml de envio se realizo solamente con Cuenta Cliente 
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="listTransaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="postAccountingMode"></param>
        private void ValidarTransactionDeleteResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, TransactionState deletionState, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {          
            var xmlReply = XDocument.Parse(replyMessage);

            var xmlAnswersCount = xmlReply.Descendants("REPLY_ACCTXN").Count();

            var listCount = listTransaction.Count();
            var transactionCounter = 0;

            if (listCount == xmlAnswersCount)
            {
                var xmlSplit = SplitXmlFile.SplitXml(replyMessage, "FCC_AC_SERVICE", "REPLY_ACCTXN", 1);

                foreach (var item in xmlSplit)
                {
                    var transaction = listTransaction[transactionCounter];

                    var errorNode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").Any();

                    if (errorNode)
                    {
                        int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
                        var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

                        transaction.StatusId = deletionState;

                        IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
                    }
                    else
                    {
                        transaction.StatusId = TransactionState.DeletedInCosmosOk;

                        IngresarEventoTransaccionDeleteOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Delete));
            }          
        }

        /// <summary>
        /// Método que valida los errores asociados al client account del proceso Delete
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessDeleteErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        /// <summary>
        /// Método que valida los errores asociados al holding account del proceso Upload
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessDeleteErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction, Country countryOriginal, int currencyId)
        {
            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey, countryOriginal, currencyId);
        }

        // <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Delete) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionDeleteError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, Country countryOriginal, int currencyId)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Delete,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Delete Process. TransactionId: {0} - Return Code: {1} ", transaction.Id, codeReturn)), "PoliticaPostAccoutingDeleteError");
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Delete) desde Cosmos
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionDeleteOK(Transaction transaction, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            if (transaction.TransactionEventOnlineLogs == null)
                transaction.TransactionEventOnlineLogs = new HashSet<TransactionEventOnlineLog>();

            transaction.TransactionEventOnlineLogs.Add(new TransactionEventOnlineLog
            {
                TransactionId = transaction.Id,
                EventOnlineLog = new EventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.Delete,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid,
                    StatusId = transaction.StatusId
                }
            });
        }

        #endregion

        #region Resumen Email Error

        /// <summary>
        /// Método que genera el objeto de proceso email error y lo envia por email avisando de error dentro del proceso Post ACH del Scheduler
        /// </summary>
        /// <param name="userSoeid"></param>
        /// <param name="fileName"></param>
        /// <param name="emailServicio"></param>
        /// <param name="emailConfiguration"></param>
        /// <param name="eventType"></param>
        /// <param name="processFlag"></param>
        /// <param name="exceptionMessage"></param>
        private static void ObtenerResumenEmailError(int countryId, int currencyId, string userSoeid, int fileId, string fileName, Guid newGuid, EmailServicio emailServicio, EmailConfiguration emailConfiguration, EventType eventType, ProcessFlag processFlag, string exceptionMessage, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            try
            {
                var resumen = new HashSet<ProcessingFileLogEmailDTO>() { 
                        new ProcessingFileLogEmailDTO
                        {                            
                            FileName = fileName,
                            EventTypeId = eventType.GetDescription(),
                            Message = exceptionMessage,
                            ProcessFlagId = processFlag.GetDescription()
                        }
                    };

                if(emailConfiguration == null)
                    throw ProveedorExcepciones.ManejaExcepcion(new ConfigurationExcepcion("Email Configuration does not exist"), "PoliticaConfigurationError");

                emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler ACH Process Error Files", "TemplateEmailFilesError", resumen, userSoeid);
            }
            catch (ConfigurationExcepcion confEx)
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, confEx.Message, "1032", fileName, fileId, newGuid, processFlag, eventType));
            }
            catch (Exception ex) 
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ex.Message, "1100", fileName, fileId, newGuid, processFlag, eventType));
            }
        }

        /// <summary>
        /// Método que genera el objeto de proceso email error y lo envia por email avisando de error dentro del proceso del Close Batch del Scheduler
        /// </summary>
        /// <param name="userSoeid"></param>
        /// <param name="fileName"></param>
        /// <param name="emailServicio"></param>
        /// <param name="emailConfiguration"></param>
        /// <param name="eventType"></param>
        /// <param name="processFlag"></param>
        /// <param name="exceptionMessage"></param>
        private static void ObtenerCloseBatchResumenEmailError(int countryId, int currencyId, string userSoeid, Guid newGuid, IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration, EmailServicio emailServicio, EmailConfiguration emailConfiguration, EventType eventType, ProcessFlag processFlag, string exceptionMessage, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var resumen = new HashSet<ProcessingFileLogEmailDTO>();

            try
            {
                batchConfiguration.ToList().ForEach(p => resumen.Add(new ProcessingFileLogEmailDTO
                {
                    FileName = p.BatchNumber.ToString(),
                    EventTypeId = eventType.GetDescription(),
                    Message = exceptionMessage,
                    ProcessFlagId = processFlag.GetDescription(),
                    Status = p.FieldStatusId.GetDescription()
                }));

                if (emailConfiguration == null)
                    throw ProveedorExcepciones.ManejaExcepcion(new ConfigurationExcepcion("Email Configuration does not exist"), "PoliticaConfigurationError");

                emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler Close Batch Process Error", "TemplateEmailCloseBatchError", resumen, userSoeid);
            }
            catch (ConfigurationExcepcion confEx)
            {
                batchConfiguration.ToList().ForEach(p =>
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, confEx.Message, "1032", p.BatchNumber.ToString(), null, newGuid, processFlag, eventType))
                );
            }
            catch (Exception ex) 
            {
                batchConfiguration.ToList().ForEach(p =>
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ex.Message, "1016", p.BatchNumber.ToString(), null, newGuid, processFlag, eventType))
                );
            }
        }        

        #endregion

        #region FACPFile

        /// <summary>
        /// Método  que obtiene una lista de transacciones, dependiendo PepbankTransactionId, para generar el archivo FACP
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="currencyOriginal"></param>
        /// <param name="newGuid"></param>
        /// <param name="contexto"></param>
        /// <param name="listProcessingLog"></param>
        /// <param name="adaptadorFile"></param>
        private void CreateFACPReturnFile(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostPaylinkConfiguration configuration, string userSoeid, Currency currencyOriginal, Guid newGuid, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;            

            try
            {
                var paybankDataAccess = new PaybankDataAccessLayer();

                var nombreArchivoArr = listTransaction.FirstOrDefault().Batch.File.Name.Split('.');

                var connectionName = GetPaybankConnectionName(countryOriginal, currencyOriginal);

                var paybankOriginalTransaction = paybankDataAccess.ObtenerOriginalPaybankTransaction(nombreArchivoArr[1], connectionName);

                if (paybankOriginalTransaction != null)
                {
                    if (!paybankOriginalTransaction.Any())
                    {
                        listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
                        return;
                    }
                    else
                    {
                        foreach (var paybankOriginator in paybankOriginalTransaction)
                        {
                            if (listTransaction.Any(p => p.PepBankTransactionId == paybankOriginator.PaybankTransactionId))
                            {
                                var transactionOriginal = listTransaction.Where(p => p.PepBankTransactionId == paybankOriginator.PaybankTransactionId).FirstOrDefault();

                                transactionOriginal.OriginalTransactionPepBankTransactionId = paybankOriginator.OriginalPaybankTransactionId.ToString();
                            }
                        }
                    }
                }
                else
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
                    return;
                }

                if (listTransaction.Any(p => p.TransactionTrandeFiles.Any(c => c.TrandeFile.EventStateId == EventState.Complete)))
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
                    return;
                }

                nombreArchivo = string.Format("FACP{0:yyMMddhhmmss}.txt", DateTime.Now);
                rutaArchivo = Path.Combine(configuration.OutputFolderPath, nombreArchivo);

                string header = GetHeaderFactFile(listTransaction.FirstOrDefault().Batch.File, configuration);
                string footer = GetFooterFactFile(listTransaction);

                using (StreamWriter FACTReturnFile = new StreamWriter(rutaArchivo, true))
                {
                    FACTReturnFile.WriteLine(header);

                    foreach (var transaction in listTransaction)
                    {
                        string line = GetDetailFactFile(transaction, configuration);
                        FACTReturnFile.WriteLine(line);
                    }

                    FACTReturnFile.WriteLine(footer);
                }

                byte[] bytes = System.IO.File.ReadAllBytes(rutaArchivo);

                var logTransactions = listTransaction.Where(p => !((p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue) || (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue && (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.Code != "9999");

                IngresarEventoFACPFileReturnOK(logTransactions, nombreArchivo, bytes, userSoeid);                

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.FACPFileSuccessfullyGenerated.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.FACPFileSuccessfullyGenerated, EventType.FACPFile));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ex.Message, null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                var logTransactions = listTransaction.Where(p => !((p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue) || (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue && (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.Code != "9999");

                IngresarEventoFACPFileReturnError(listTransaction, nombreArchivo, null, ex.Message, userSoeid);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessage"></param>
        private void IngresarEventoFACPFileReturnOK(IEnumerable<Transaction> transactions, string fileName, byte[] file, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.FACPFileGenerationOk;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        File = file,
                        Date = DateTime.Now,
                        EventTypeId = EventType.FACPFile,
                        EventStateId = EventState.Complete,
                        StatusId = transaction.StatusId,
                        UserSoeid = userSoeid
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessage"></param>
        private void IngresarEventoFACPFileReturnError(IEnumerable<Transaction> transactions, string fileName, byte[] file, string message, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.FACPFileGenerationError;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        File = file,
                        Date = DateTime.Now,
                        EventTypeId = EventType.FACPFile,
                        EventStateId = EventState.Incomplete,
                        MessageReceived = message,
                        StatusId = transaction.StatusId,
                        UserSoeid = userSoeid
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private string GetHeaderFactFile(Dominio.Core.File fileOriginal, PostPaylinkConfiguration configuration)
        {
            string result = string.Empty;

            result += '0';
            if (configuration.CountryCode.ToString().Length < 4)
            {
                var branch = new string('0', 4 - configuration.CountryCode.ToString().Length);
                result += string.Format("{0}{1}", branch, configuration.CountryCode);
            }
            else
            {
                result += configuration.CountryCode.ToString();
            }

            result += fileOriginal.DateSelected.ToString("yyMMdd");
            result += new string(' ', 49);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private string GetFooterFactFile(IEnumerable<Transaction> transactions)
        {
            string result = string.Empty;
            string qREC = transactions.ToList().Count.ToString();
            decimal total = transactions.Where(t => t.NatureTransactionId == NatureTransaction.Credit).Sum(p => p.Amount) + transactions.Where(t => t.NatureTransactionId == NatureTransaction.Debit).Sum(p => p.Amount);
            Int64 totalFooter = (Int64)(total * 100);

            result += '9';
            if (qREC.Length < 7)
            {
                var cl_qrec = new string('0', 7 - qREC.Length);
                result += string.Format("{0}{1}", cl_qrec, qREC);
            }
            else
            {
                result += qREC;
            }

            if (totalFooter.ToString().Length < 18)
            {
                var tAmount = new string('0', 18 - totalFooter.ToString().Length);
                result += string.Format("{0}{1}", tAmount, totalFooter.ToString());
            }
            else
            {
                result += totalFooter.ToString();
            }

            result += new string(' ', 34);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private string GetDetailFactFile(Transaction transaction, PostPaylinkConfiguration configuration)
        {
            string result = string.Empty;
            string amount = ((Int64)transaction.Amount * 100).ToString();
            string clRef = string.Empty;
            string pyType = (transaction.NatureTransactionId == NatureTransaction.Credit) ? "071" : "171";
            string pyrCode = transaction.ReturnCodeMappingConfiguration.PaylinkReturnCode;

            if (configuration.CBIIReferenceNumberId == CBIIReferenceNumber.TraceNumber)
            {
                if (transaction.TrackingNumber.Trim().Length >= 7)
                {
                    if (transaction.OriginalTransactionPepBankTransactionId != null)
                    {
                        clRef = "0990" + transaction.TrackingNumber.Trim().Substring(transaction.TrackingNumber.Trim().Length - 7, 7);
                    }
                }
                else
                {
                    //throw new Exception(new ECText(109, "The reference number for the transaction id: ").ToString() + tran.NumConsTransaccion.ToString() + new ECText(110, " does not have the required length.").ToString());
                    throw new AplicacionExcepcion("The reference number for the transaction id: " + transaction.TransactionCode.ToString() + " does not have the required length.");
                }
            }
            else if (configuration.CBIIReferenceNumberId == CBIIReferenceNumber.IDNumber)
            {
                if (transaction.BeneficiaryId.Length > 11)
                {
                    int value = transaction.BeneficiaryId.Length - 11;
                    clRef = transaction.BeneficiaryId.Substring(value, 11);
                }
                else
                {
                    clRef = transaction.BeneficiaryId;
                }

            }

            result += "1";

            if (transaction.OriginatorAccount.Length < 11)
                result += new string('0', 11 - transaction.OriginatorAccount.Length) + transaction.OriginatorAccount;
            else
            {
                if (transaction.OriginatorAccount.Length > 11)
                {
                    int value = transaction.OriginatorAccount.Length - 11;
                    result += transaction.OriginatorAccount.Substring(value, 11);
                }
                else
                {
                    result += transaction.OriginatorAccount;
                }
            }


            if (amount.Length < 17)
                result += new string('0', 17 - amount.Length) + amount;
            else
                result += amount;

            if (clRef.Length < 11)
                result += new string('0', 11 - clRef.Length) + clRef;
            else
                result += clRef;

            if (pyType.Length < 3)
                result += new string('0', 3 - pyType.Length) + pyType;
            else
                result += pyType;

            result += "R";

            result += new string(' ', 1);

            if (pyrCode.Length < 2)
                result += new string('0', 2 - pyrCode.Length) + pyrCode;
            else
                result += pyrCode;

            result += new string(' ', 13);

            return result;

        }

        #endregion

        #region ReturnFile

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileOriginal"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        private void CreatePaybankReturnFile(IEnumerable<Transaction> listTransaction, Country countryOriginal, PaybankReturnFileConfiguration configuration, string userSoeid, int currencyId, Guid newGuid, int currentCulture, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var eventLogFilterResult = new List<EventOnlineLog>();
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;

            try
            {
                var arrTrandeFileId = listTransaction.SelectMany(p => p.TransactionTrandeFiles).Where(p => p.TrandeFile.EventStateId == EventState.Complete).Select(p => p.TransactionId).ToArray();

                if (listTransaction.All(p => arrTrandeFileId.Contains(p.Id) && p.TransactionTrandeFiles.All(d => d.TrandeFile.EventStateId == EventState.Complete)))
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToWriteInAPaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
                    return;
                }

                nombreArchivo = (configuration.CreateFileOptionId == CreateFileOption.OnePerDay) ? string.Format("PayBankFile{0:yyMMdd}", DateTime.Now) : string.Format("PayBankFile{0:yyMMddhhmmss}", DateTime.Now);
                rutaArchivo = Path.Combine(configuration.OutputFolderPath, nombreArchivo);

                if (configuration.IncludeTrxRejectedByAccounting)
                {
                    var arrStatesAccounting = new[] { TransactionState.UploadToCosmosRejected, TransactionState.AuthorizeByCosmosRejected, TransactionState.AuthorizeByCosmosClientAccountError, TransactionState.UploadToCosmosClientAccountError, TransactionState.UploadToCosmosHoldingAccountError, TransactionState.AuthorizeByCosmosHoldingAccountError };

                    eventLogFilterResult.AddRange(listTransaction.Where(p => !arrTrandeFileId.Contains(p.Id)).SelectMany(p => p.TransactionEventOnlineLogs).Where(p => arrStatesAccounting.Contains(p.EventOnlineLog.StatusId) && p.ErrorMessageId.HasValue).Select(p => p.EventOnlineLog));
                }

                if (configuration.IncludeTrxRejectedByECS)
                {
                    //transactionFilter = transactionFilter.Where(p => p.StatusId != TransactionState.CitiscreeningRejected);
                }

                if (configuration.IncludeTrxRejectedSanctionScreening)
                {
                    var arrStatesCitiscreening = new[] { TransactionState.CitiscreeningRejected, TransactionState.CitiscreeningError };

                    eventLogFilterResult.AddRange(listTransaction.Where(p => !arrTrandeFileId.Contains(p.Id)).SelectMany(p => p.TransactionEventOnlineLogs).Where(p => arrStatesCitiscreening.Contains(p.EventOnlineLog.StatusId) && p.ErrorMessageId.HasValue).Select(p => p.EventOnlineLog));
                }

                if (!eventLogFilterResult.SelectMany(p => p.TransactionEventOnlineLogs).Any(p => p.ErrorMessageId.HasValue))
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToGeneratePaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
                    return;
                }

                if (eventLogFilterResult.Exists(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessage.CategoryErrorId == CategoryError.Business)))
                {
                    using (StreamWriter paybankReturnFile = new StreamWriter(rutaArchivo, true))
                    {
                        string correctedData = string.Empty.PadLeft(44, ' ');

                        foreach (var eventOnline in eventLogFilterResult.Where(p => (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.CategoryErrorId == CategoryError.Business))
                        {
                            string returnCode = null;

                            try
                            {
                                switch (eventOnline.EventTypeId)
                                {
                                    case EventType.CitiscreeningValidation:
                                        if (eventOnline.StatusId == TransactionState.CitiscreeningRejected)
                                        {
                                            if (configuration.IncludeTrxRejectedSanctionScreening)
                                            {
                                                returnCode = configuration.SanctionScreeningRejectionCode.Trim();
                                            }
                                            else
                                            {
                                                if (eventOnline.TransactionEventOnlineLogs.Select(p => p.Transaction).Any(p => p.PendingChangeCitiscreenings.Any()))
                                                {
                                                    if (eventOnline.TransactionEventOnlineLogs.Select(p => p.Transaction).SelectMany(d => d.PendingChangeCitiscreenings).Any(c => c.RejectCodeId.HasValue))
                                                    {
                                                        returnCode = eventOnline.TransactionEventOnlineLogs.Select(p => p.Transaction).SelectMany(d => d.PendingChangeCitiscreenings).Where(c => c.RejectCodeId.HasValue).FirstOrDefault().RejectCode.Value;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        returnCode = eventOnline.TransactionEventOnlineLogs.FirstOrDefault().ErrorMessage.NachaCode.Identifier.ToUpper();
                                        break;
                                }
                            }
                            catch
                            {
                                throw new AplicacionExcepcion(currentCulture == 0 ? "There is no Nacha code associated with the Error" : "No existe código Nacha asociado al Error");
                            }

                            string line = string.Format("{0}{1}{2}", eventOnline.TransactionEventOnlineLogs.FirstOrDefault().Transaction.PepBankTransactionId, returnCode, correctedData);
                            paybankReturnFile.WriteLine(line);
                        }
                    }

                    byte[] bytes = System.IO.File.ReadAllBytes(rutaArchivo);

                    IngresarEventoFileReturnOK(eventLogFilterResult.Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessage.CategoryErrorId == CategoryError.Business)).SelectMany(p => p.TransactionEventOnlineLogs).Select(c => c.Transaction), nombreArchivo, bytes, userSoeid);
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.PaybankReturnFileSucessfullyGenerated.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.PaybankReturnFileSucessfullyGenerated, EventType.PaybankReturnFile));
                }
                else
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToWriteInAPaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
                }
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ex.Message, null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                var transactions = listTransaction.Where(p => (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue && ((p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().EventOnlineLog.EventTypeId == EventType.Upload || (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().EventOnlineLog.EventTypeId == EventType.Authorize) && (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.CategoryErrorId == CategoryError.Business);

                IngresarEventoFileReturnError(transactions.Any() ? transactions : new List<Transaction>(), nombreArchivo, null, ex.Message, userSoeid);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessage"></param>
        private void IngresarEventoFileReturnOK(IEnumerable<Transaction> transactions, string fileName, byte[] file, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.PaybankReturnFileGenerationOk;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        File = file,
                        Date = DateTime.Now,
                        EventTypeId = EventType.PaybankReturnFile,
                        EventStateId = EventState.Complete,
                        StatusId = transaction.StatusId,
                        UserSoeid = userSoeid
                    }
                });
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="errorMessage"></param>
        private void IngresarEventoFileReturnError(IEnumerable<Transaction> transactions, string fileName, byte[] file, string message, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.PaybankReturnFileGenerationError;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TrandeFile = new TrandeFile
                    {
                        FileName = fileName,
                        Date = DateTime.Now,
                        EventTypeId = EventType.PaybankReturnFile,
                        EventStateId = EventState.Incomplete,
                        StatusId = transaction.StatusId,
                        MessageReceived = message,
                        UserSoeid = userSoeid
                    }
                });
            }

        }


        #endregion

        #endregion

        #region Métodos Privados Inclearing Check

        #region Open Batch

        /// <summary>
        /// Método que inicia el proceso Open Batch y obtiene el xml del mensaje
        /// </summary>
        /// <param name="fileOriginal"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        private void IniciarProcesoOpenBatchInclearingCheck(Dominio.Core.File fileOriginal, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, int currentCulture, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            var transactions = fileOriginal.Batchs.SelectMany(p => p.Transactions);

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            foreach (var item in groupingBank)
            {
                var xmlString = DominioOpenBatchLogica.GetOpenBatchInclearingCheckXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, item.Bank, check);

                EnvioTransaccionOpenBatchInclearingCheck(xmlString, userSoeid, newGuid, errorMessages, item.Bank, check, uniqueKey, currentCulture, fileOriginal, currencyId, countryOriginal, listProcessingLog, adaptadorFile);
            }           
        }

        /// <summary>
        /// Método que envia y valida el envio del Open Batch
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="check"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="countryOriginal"></param>
        private void EnvioTransaccionOpenBatchInclearingCheck(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, Bank bankConfiguration, DailyBrand check, string uniqueKey, int currentCulture, Dominio.Core.File fileOriginal, int currencyId, Country countryOriginal, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (codeReturn == 0)
            {
                var xdoc = XDocument.Parse(replyMessage);

                if (xdoc.Descendants("RE_BATCH").Any(p => p.Element("ERROR") != null))
                {
                    var error = xdoc.Descendants("RE_BATCH").Select(p => p.Element("ERROR")).FirstOrDefault();

                    var codigoError = error.Element("ECODE").Value;
                    var descripcion = error.Element("EDESC").Value;

                    if (codigoError == "50")
                    {
                        listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchAlreadyOpened.GetDescription(), null, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchAlreadyOpened, EventType.OpenBatch));
                        IngresarEventoTransaccionOpenBatchErrorInclearingCheck(bankConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);
                        SetBankBatchOpen(userSoeid, bankConfiguration, check);                        
                    }
                    else
                    {
                        listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                        InsertErrorMessageOpenBatchInclearingCheckCodeReturn0(xmlMessage, userSoeid, newGuid, errorMessages, bankConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, errorMessage, codigoError, descripcion);                        
                    }
                }
                else
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchSucessfully.GetDescription(), null, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchSucessfully, EventType.OpenBatch));
                    IngresarEventoTransaccionOpenBatchOKInclearingCheck(bankConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);                    
                    SetBankBatchOpen(userSoeid, bankConfiguration, check);                   
                }
            }
            else
            {
                listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codeReturn.ToString(), fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                InsertErrorMessageOpenBatchInclearingCheckCodeReturnNo0(xmlMessage, userSoeid, newGuid, errorMessages, bankConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, codeReturn, errorMessage);
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Open Batch) desde Cosmos
        /// </summary>
        /// <param name="bankConfiguration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionOpenBatchErrorInclearingCheck(Bank bankConfiguration, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, int fileId)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (bankConfiguration.BankBatchBankEventOnlineLogs == null)
                bankConfiguration.BankBatchBankEventOnlineLogs = new HashSet<BankBatchBankEventOnlineLog>();

            bankConfiguration.BankBatchBankEventOnlineLogs.Add(new BankBatchBankEventOnlineLog
            {
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                FileId = fileId,
                BatchBankEventOnlineLog = new BatchBankEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.OpenBatch,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid
                }
            });

            if (errorMessage == null)
            {
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Open Batch Process. Bank Id : {0} - Return Code: {1}", bankConfiguration.Id, codeReturn)), "PoliticaPostAccountingOpenBatchError");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="replyMessage"></param>
        /// <param name="errorMessage"></param>
        /// <param name="codigoError"></param>
        /// <param name="descripcion"></param>
        private void InsertErrorMessageOpenBatchInclearingCheckCodeReturn0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, Bank bankConfiguration, string uniqueKey, int currentCulture, int fileId, string replyMessage, ErrorMessage errorMessage, string codigoError, string descripcion)
        {
            IngresarEventoTransaccionOpenBatchErrorInclearingCheck(bankConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId);

            var internErrorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == int.Parse(codigoError)).FirstOrDefault();

            if (errorMessage != null)
            {
                var mensaje = currentCulture == 0 ? String.Format("There was an error when try to open the batch: {0}", internErrorMessage.EnglishText) : String.Format("Hubo un error cuando se trato de abrir el batch: {0}", internErrorMessage.SpanishText);

                throw new AplicacionExcepcion(mensaje);
            }
            else
            {
                var mensaje = currentCulture == 0 ? string.Format("There was an error when try to open the batch. Contact with your administrator. Error Code: {0}", codigoError) : string.Format("Hubo un error cuando se trato de abrir el batch. Contactese con su administrador. Código Error: {0}", codigoError);

                throw new AplicacionExcepcion(mensaje);
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Open Batch) desde Cosmos
        /// </summary>
        /// <param name="bankConfiguration"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="fileId"></param>
        private void IngresarEventoTransaccionOpenBatchOKInclearingCheck(Bank bankConfiguration, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, int fileId)
        {
            if (bankConfiguration.BankBatchBankEventOnlineLogs == null)
                bankConfiguration.BankBatchBankEventOnlineLogs = new HashSet<BankBatchBankEventOnlineLog>();

            bankConfiguration.BankBatchBankEventOnlineLogs.Add(new BankBatchBankEventOnlineLog
            {
                FileId = fileId,
                BatchBankEventOnlineLog = new BatchBankEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.OpenBatch,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSoeid"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="check"></param>
        private static void SetBankBatchOpen(string userSoeid, Bank bankConfiguration, DailyBrand check)
        {
            if (check == DailyBrand.AM)
            {
                bankConfiguration.MorningBatchIsOpen = true;
                bankConfiguration.UserOpenedMorningBatch = userSoeid;
            }

            if (check == DailyBrand.PM)
            {
                bankConfiguration.AfternoonBatchIsOpen = true;
                bankConfiguration.UserOpenedAfternoonBatch = userSoeid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="fileOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="replyMessage"></param>
        /// <param name="codeReturn"></param>
        /// <param name="errorMessage"></param>
        private void InsertErrorMessageOpenBatchInclearingCheckCodeReturnNo0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, Bank bankConfiguration, string uniqueKey, int currentCulture, int fileId, string replyMessage, int codeReturn, ErrorMessage errorMessage)
        {
            IngresarEventoTransaccionOpenBatchErrorInclearingCheck(bankConfiguration, errorMessages, codeReturn, null, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId); 

            var internErrorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage != null)
            {
                var mensaje = currentCulture == 0 ? String.Format("There was an error when try to open the batch: {0}", internErrorMessage.EnglishText) : String.Format("Hubo un error cuando se trato de abrir el batch: {0}", internErrorMessage.SpanishText);                

                throw new AplicacionExcepcion(mensaje);
            }
            else
            {
                var mensaje = currentCulture == 0 ? string.Format("There was an error when try to open the batch. Contact with your administrator. Error Code: {0}", codeReturn) : string.Format("Hubo un error cuando se trato de abrir el batch. Contactese con su administrador. Código Error : {0}", codeReturn);

                throw new AplicacionExcepcion(currentCulture == 0 ? "There was an error when try to open the batch. Contact with your administrator." : "Hubo un error cuando se trato de abrir el batch. Contactese con su administrador.");
            }
        }

        #endregion

        #region Upload

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesUploadInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {           
            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            foreach (var bankItem in groupingBank)
            {
                var transactionFilterStateNoUploadToCosmos = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk));
                var transactionFilterStateUploadError = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError)).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

                var statusId = transactionFilterStateNoUploadToCosmos.Any() ? TransactionState.Import : (transactionFilterStateUploadError.Any() ? TransactionState.UploadToCosmosRejected : TransactionState.Import);

                var listaSplitedStateNoUploadToCosmos = transactionFilterStateNoUploadToCosmos.Any() ? transactionFilterStateNoUploadToCosmos.Split(maxTransactionNumber) : null;
                var listaSplitedStateErrorUploadToCosmos = transactionFilterStateUploadError.Any() ? transactionFilterStateUploadError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : null;

                switch (statusId)
                {
                    case TransactionState.Import:
                        foreach (var itemNoUpload in listaSplitedStateNoUploadToCosmos ?? new List<List<Transaction>>())
                        {
                            if (itemNoUpload.Any())
                                ValidarTransaccionUploadStateNoUploadToCosmosInclearingCheck(itemNoUpload, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog, adaptadorFile);
                        }
                        break;
                    case TransactionState.UploadToCosmosRejected:
                        foreach (var itemError in listaSplitedStateErrorUploadToCosmos ?? new List<List<Transaction>>())
                        {
                            if (itemError.Any())
                                ValidarTransaccionUploadStateUploadToCosmosInclearingCheck(itemError, transactions, newGuid, countryOriginal, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog, adaptadorFile);
                        }
                        break;
                }
            }           
        }

        /// <summary>
        /// Método que obtiene los xml agrupados por el Unique key que ya fue enviado la primera vez
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionUploadStateUploadToCosmosInclearingCheck(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bank, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Upload && p.EventOnlineLog.StatusId == TransactionState.UploadToCosmos || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosRejected || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosHoldingAccountError)).OrderByDescending(p => p.EventOnlineLog.Date).Select(p => p.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            foreach (var eventOnline in eventOnlineGroupByUniqueKey)
            {                
                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionUploadResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
            }
        }

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que sea la primera vez que se envian a cosmos
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionUploadStateNoUploadToCosmosInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bank, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioUploadLogica.GetXmlDeclaration(uniqueKey);

            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            foreach (var transaction in splitList)
            {
                transaction.StatusId = TransactionState.UploadToCosmos;

                DominioUploadLogica.GetUploadMessageByTransactionTypeInclearingCheck(countryOriginal, configuration, xDeclaration, transaction, postAccountingMode, cosmosFunctionalUser, bank, check);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionUploadResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
        }       

        #endregion        

        #region Authorize

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesAuthorizeInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {         
            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            foreach (var bankItem in groupingBank)
            {
                var transactionFilterStateNoAuthorize = transactions.Where(c => c.BankId == bankItem.Key && c.StatusId == TransactionState.UploadToCosmosButNotAuthorized);
                var transactionFilterStateAuthorizeError = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError)).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

                var statusId = transactionFilterStateNoAuthorize.Any() ? TransactionState.UploadToCosmosButNotAuthorized : (transactionFilterStateAuthorizeError.Any() ? TransactionState.AuthorizeByCosmosRejected : TransactionState.UploadToCosmosButNotAuthorized);

                var listaSplitedStateNotAuthorized = transactionFilterStateNoAuthorize.Any() ? transactionFilterStateNoAuthorize.Split(maxTransactionNumber) : null;
                var listaSplitedStateErrorAuthorizeToCosmos = transactionFilterStateAuthorizeError.Any() ? transactionFilterStateAuthorizeError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : null;

                switch (statusId)
                {
                    case TransactionState.UploadToCosmosButNotAuthorized:
                        foreach (var itemNotAuthrorize in listaSplitedStateNotAuthorized ?? new List<List<Transaction>>())
                        {
                            if (itemNotAuthrorize.Any())
                                ValidarTransaccionAuthorizeStateNotAuthorizeInclearingCheck(itemNotAuthrorize, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog, adaptadorFile);
                        }
                        break;
                    case TransactionState.AuthorizeByCosmosRejected:
                        foreach (var itemAuthorize in listaSplitedStateErrorAuthorizeToCosmos ?? new List<List<Transaction>>())
                        {
                            if (itemAuthorize.Any())
                                ValidarTransaccionAuthorizeStateAuthorizeInclearingCheck(itemAuthorize, transactions, newGuid, countryOriginal, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog, adaptadorFile);
                        }
                        break;
                }
            }          
        }

        /// <summary>
        /// Método que obtiene los xml agrupados por el Unique key que ya fue enviado la primera vez
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionAuthorizeStateAuthorizeInclearingCheck(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Authorize && p.EventOnlineLog.StatusId == TransactionState.Authorize || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError)).OrderByDescending(p => p.EventOnlineLog.Date).Select(c => c.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            foreach (var eventOnline in eventOnlineGroupByUniqueKey)
            {
                var listTransaction = eventOnline.ListEventOnlineLog.SelectMany(p => p.TransactionEventOnlineLogs).Select(p => p.Transaction);

                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionAuthorizeResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);
            }
        }

        /// <summary>
        /// Método que realiza la generación de los xml en el caso de que sea la primera vez que se envian a cosmos
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="transaction"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        private void ValidarTransaccionAuthorizeStateNotAuthorizeInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioAuthorizeLogica.GetXmlDeclaration(uniqueKey);

            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            foreach (var transaction in splitList)
            {
                transaction.StatusId = TransactionState.Authorize;

                DominioAuthorizeLogica.GetAuthorizeMessageByTransactionTypeInclearingCheck(countryOriginal, configuration, xDeclaration, transaction, postAccountingMode, cosmosFunctionalUser, bankConfiguration, check);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionAuthorizeResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal, currencyId, listProcessingLog, adaptadorFile);

        }       

        #endregion

        #region Delete

        /// <summary>
        /// Método que separa la cantidad de transacciones en tamaños definidos por configuración y dependiendo del estado realiza su proceso definido
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void EnvioTransaccionesDeleteInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {          
            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = contexto.ErrorMessage.Where(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active);

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            foreach (var item in groupingBank)
            {
                var transactionClientAccountError = transactions.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError));

                var listaSplitedClientAccountError = transactionClientAccountError.Count() > 0 ? transactionClientAccountError.SelectMany(p => p.TransactionEventOnlineLogs).Select(p => p.Transaction).Split(maxTransactionNumber) : null;

                foreach (var itemClientAccount in listaSplitedClientAccountError ?? new List<List<Transaction>>())
                {
                    if (itemClientAccount.Any())
                        ValidarTransaccionDeleteStateClientAccountErrorInclearingCheck(itemClientAccount, newGuid, countryOriginal, configuration, errorMessages, userSoeid, item.Bank, check, currencyId, listProcessingLog, adaptadorFile);
                }
            }         
        }

        /// <summary>
        /// Método que obtiene los xml agrupados por el Unique key que ya fue enviado la primera vez
        /// </summary>
        /// <param name="splitList"></param>
        /// <param name="newGuid"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="userSoeid"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        /// <param name="listProcessingLog"></param>
        private void ValidarTransaccionDeleteStateClientAccountErrorInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioDeleteLogica.GetXmlDeclaration(uniqueKey);

            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            foreach (var transaction in splitList)
            {
                DominioDeleteLogica.GetDeleteMessageByTransactionTypeInclearingCheck(countryOriginal, configuration, xDeclaration, transaction, postAccountingMode, cosmosFunctionalUser, bankConfiguration, check);
            }

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosClientAccountError, countryOriginal, currencyId, listProcessingLog, adaptadorFile);

        }        

        #endregion

        #endregion

        #region Métodos Utilitarios Scheduler

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public EmailConfiguration ObtenerEmailConfiguration(int countryId) 
        {
            using (var contexto = new ContextoPrincipal()) 
            {
                var emailConfiguration = contexto.EmailConfiguration.Where(p => p.CountryId == countryId).Include("NotificationEmails").FirstOrDefault();

                return emailConfiguration;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public TransactionTypeBatchConfiguration ObtenerBatchConfigurationFromFile(int countryId, int fileId, int currencyId)
        {
            using (var contexto = new ContextoPrincipal())
            {
                var fileOriginal = contexto.File.Where(p => p.Id == fileId).Include("TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry").FirstOrDefault();

                var transactionTypeConfigurationId = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfigurationId;

                var listaOriginalTransactionTypeBatchConfiguration = contexto.TransactionTypeBatchConfiguration.Where(p => p.FieldStatusId == FieldStatus.Active && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.Id == transactionTypeConfigurationId && p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId);

                var batchConfiguration = listaOriginalTransactionTypeBatchConfiguration.Any(p => p.IsDefault) ? listaOriginalTransactionTypeBatchConfiguration.Where(p => p.IsDefault).FirstOrDefault() : listaOriginalTransactionTypeBatchConfiguration.FirstOrDefault();

                return batchConfiguration;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        public List<int> GetTypeConfigurationBatchIdsByCountry(int countryId, int currencyId)
        {
            List<int> transactionTypeConfigurationBatchIds = new List<int>();

            using (var contexto = new ContextoPrincipal())
            {                
                var transactionTypeBatchOriginal = contexto.TransactionTypeBatchConfiguration.Where(p => p.FieldStatusId == FieldStatus.Active && p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryId && p.IsOpen == true);

                transactionTypeBatchOriginal.ToList().ForEach(p => transactionTypeConfigurationBatchIds.Add(p.Id));

                return transactionTypeConfigurationBatchIds;
            }
        }

        #endregion

        #region Close Batch

        /// <summary>
        /// Método que inicia el proceso Close Batch y obtiene el xml del mensaje
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="currencyId"></param>
        /// <param name="errorMessages"></param>
        private void IniciarProcesoCloseBatch(Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration, int currentCulture, Guid newGuid, int currencyId, IEnumerable<ErrorMessage> errorMessages, ContextoPrincipal contexto, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {            
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            foreach (var batch in batchConfiguration)
            {
                try
                {
                    var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

                    var xmlString = DominioCloseBatchLogica.GetCloseBatchXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, batch);

                    EnvioTransaccionCloseBatch(xmlString, userSoeid, newGuid, errorMessages, batch, uniqueKey, currentCulture, countryOriginal, currencyId, listProcessingLog, adaptadorFile);                    
                }
                catch (Exception ex)
                {
                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ex.Message, "1021", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                }
            }
        }

        /// <summary>
        /// Método que envia y valida el envio del Close Batch
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        /// <param name="errorMessages"></param>
        /// <param name="batchConfiguration"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="currentCulture"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="currencyId"></param>
        private void EnvioTransaccionCloseBatch(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, Country countryOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog, IAdaptadorFile adaptadorFile)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.CloseBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.CloseBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (codeReturn == 0)
            {
                var xdoc = XDocument.Parse(replyMessage);

                if (xdoc.Descendants("RE_BATCH").Any(p => p.Element("ERROR") != null))
                {
                    var error = xdoc.Descendants("RE_BATCH").Select(p => p.Element("ERROR")).FirstOrDefault();

                    var codigoError = error.Element("ECODE").Value;
                    var descripcion = error.Element("EDESC").Value;

                    if (codigoError == "59")
                    {
                        IngresarEventoTransaccionCloseBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                        batchConfiguration.IsOpen = false;
                        batchConfiguration.CloseDate = DateTime.Now;

                        listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, descripcion, codigoError, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.CloseBatchAlreadyClosed, EventType.CloseBatch));

                        if (HttpContext.Current != null)
                            HttpContext.Current.Session["CloseBatchAlreadyClosed"] = true;
                        SaveCriticalBatch(batchConfiguration.Id);
                    }
                    else
                    {
                        IngresarEventoTransaccionCloseBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                        errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == int.Parse(codigoError)).FirstOrDefault();

                        if (HttpContext.Current != null)
                            HttpContext.Current.Session["UnableToCloseBatch"] = true;

                        SaveCriticalBatch(batchConfiguration.Id);

                        if (errorMessage != null)
                        {
                            var mensaje = currentCulture == 0 ? string.Format("There was an error when try to close the batch: {0}", errorMessage.EnglishText) : string.Format("Hubo un error cuando se trato de cerrar el batch: {0}", errorMessage.SpanishText);

                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, errorMessage.Code, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                        }
                        else
                        {
                            var mensaje = currentCulture == 0 ? "There was an error when try to close the batch. Contact with your administrator." : "Hubo un error cuando se trato de cerrar el batch. Contactese con su administrador.";

                            listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, codigoError, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                        }
                    }
                }
                else
                {
                    batchConfiguration.IsOpen = false;
                    batchConfiguration.CloseDate = DateTime.Now;

                    IngresarEventoTransaccionCloseBatchOK(batchConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.CloseBatchSucessfully.GetDescription(), "0", batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.CloseBatchSucessfully, EventType.CloseBatch));

                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["CloseBatchSucessfully"] = true;

                    SaveCriticalBatch(batchConfiguration.Id);
                }
            }
            else
            {
                IngresarEventoTransaccionCloseBatchError(batchConfiguration, errorMessages, codeReturn, null, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

                if (HttpContext.Current != null)
                    HttpContext.Current.Session["UnableToCloseBatch"] = true;

                SaveCriticalBatch(batchConfiguration.Id);

                if (errorMessage != null)
                {
                    var mensaje = currentCulture == 0 ? string.Format("There was an error when try to close the batch: {0}", errorMessage.EnglishText) : string.Format("Hubo un error cuando se trato de cerrar el batch: {0}", errorMessage.SpanishText);

                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, errorMessage.Code, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                }
                else
                {
                    var mensaje = currentCulture == 0 ? "There was an error when try to close the batch. Contact with your administrator." : "Hubo un error cuando se trato de cerrar el batch. Contactese con su administrador.";

                    listProcessingLog.Add(adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, codeReturn.ToString(), batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                }
            }
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta correcta (Close Batch) desde Cosmos
        /// </summary>
        /// <param name="batchConfiguration"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionCloseBatchOK(TransactionTypeBatchConfiguration batchConfiguration, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            if (batchConfiguration.BatchConfigurationBatchEventOnlineLogs == null)
                batchConfiguration.BatchConfigurationBatchEventOnlineLogs = new HashSet<BatchConfigurationBatchEventOnlineLog>();

            batchConfiguration.BatchConfigurationBatchEventOnlineLogs.Add(new BatchConfigurationBatchEventOnlineLog
            {
                BatchEventOnlineLog = new BatchEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.CloseBatch,
                    EventStateId = EventState.Complete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    UserSoeid = userSoeid
                }
            });
        }

        /// <summary>
        /// Método que registra un nuevo evento en el caso de que sea una respuesta incorrecta (Close Batch) desde Cosmos
        /// </summary>
        /// <param name="batchConfiguration"></param>
        /// <param name="errorMessages"></param>
        /// <param name="codeReturn"></param>
        /// <param name="description"></param>
        /// <param name="newGuid"></param>
        /// <param name="sendMessage"></param>
        /// <param name="receiveMessage"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        private void IngresarEventoTransaccionCloseBatchError(TransactionTypeBatchConfiguration batchConfiguration, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            var errorMessage = errorMessages.Where(p => Int16.Parse(p.Code) == codeReturn).FirstOrDefault();

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.Where(p => p.Code == codigoGenerico).FirstOrDefault();
            }

            if (batchConfiguration.BatchConfigurationBatchEventOnlineLogs == null)
                batchConfiguration.BatchConfigurationBatchEventOnlineLogs = new HashSet<BatchConfigurationBatchEventOnlineLog>();

            batchConfiguration.BatchConfigurationBatchEventOnlineLogs.Add(new BatchConfigurationBatchEventOnlineLog
            {
                ErrorMessageId = errorMessage != null ? errorMessage.Id : (int?)null,
                BatchEventOnlineLog = new BatchEventOnlineLog
                {
                    UniqueKey = newGuid,
                    UniqueKeyCosmos = uniqueKey,
                    Date = DateTime.Now,
                    EventTypeId = EventType.CloseBatch,
                    EventStateId = EventState.Incomplete,
                    XmlRequest = sendMessage,
                    XmlResponse = receiveMessage,
                    MessageReceived = string.IsNullOrEmpty(description) ? null : description,
                    UserSoeid = userSoeid
                }
            });

            if (errorMessage == null)
                ProveedorExcepciones.ManejaExcepcion(new AplicacionExcepcion(string.Format("Unexistent return code in ACH_ErrorMessage Table. Open Batch Process. Batch Id : {0} - Return Code: {1}", batchConfiguration.Id, codeReturn)), "PoliticaPostAccountingCloseBatchError");

        }

        /// <summary>
        /// Método que valida en el caso de que hayan ocurrido excepciones durante el proceso de Close Batch
        /// </summary>
        /// <param name="exception"></param>
        private void ValidarExcepcionesCloseBatch(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Count > 0)
            {
                var groupList = (from c in listaExcepcion
                                 orderby c.GetType()
                                 select c).GroupBy(g => g.GetType()).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingCloseBatchError");
                });

                throw groupList.FirstOrDefault();
            }
        }     

        #endregion

        #region Summary Grids

        /// <summary>
        /// Método que retorna el resumen del proceso del archivo
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userSoeid"></param>
        /// <returns></returns>
        public IEnumerable<FileResumenGridBodyDTO> ProcesoPostAccountingFilesResume(List<int> fileIdsList)
        {
            IAdaptadorFile adaptadorFile = null;

            using (ContextoPrincipal contexto = new ContextoPrincipal())
            {
                adaptadorFile = new AdaptadorFile();

                var listaOriginal = contexto.File.Where(p => fileIdsList.Contains(p.Id));
                var fileResumenList = adaptadorFile.FabricarFileResumenGridBodyDTO(listaOriginal);

                return fileResumenList;
            }          
        }

        #endregion
    }
}
