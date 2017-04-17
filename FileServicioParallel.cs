using Aplicacion.Base;
using AutoMapper;
using Datos.Persistencia.Core;
using Datos.Persistencia.Messaging;
using Dominio.Core;
using Dominio.Mantenedores;
using Dominio.Process;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Utilitarios.Base;
using Utilitarios.CustomDatabaseTraceListener;
using Utilitarios.Excepciones;

namespace Aplicacion.Process
{
    public class FileServicioParallel : IFileServicioParallel
    {
        #region Miembros

        private IFileRepositorio fileRepositorio;
        private ICountryRepositorio countryRepositorio;
        private IPaybankConfigurationRepositorio paybankConfigurationRepositorio;
        private ITransactionTypeConfigurationRepositorio transactionTypeConfigurationRepositorio;
        private IMessageQueueFactory messageQueueFactory;
        private IErrorMessageRepositorio errorMessageRepositorio;
        private IReturnCodeMappingConfiguratonRepositorio returnCodeMappingConfigurationRepositorio;
        private ITransactionTypeBatchConfigurationRepositorio transactionTypeBatchConfigurationRepositorio;
        private IBankRepositorio bankRepositorio;
        private IEventOnlineLogRepositorio eventOnlineLogRepositorio;
        private ITransactionTypeConfigurationCountryRepositorio transactionTypeConfigurationCountryRepositorio;
        private ICurrencyRepositorio currencyRepositorio;
        private IPaybankRepositorio paybankRepositorio;
        private IAdaptadorFile adaptadorFile;
        private IProcessingFileLogRepositorio processingFileLogRepositorio;
        private ITransactionRepositorio transactionRepositorio;
        private IMapper mapper;
        private IEmailServicio emailServicio;
        private ISchedulerPostACHConfigurationRepositorio schedulerPostACHConfigurationRepositorio;
        private ICitiscreeningConfigurationRepositorio citiscreeningConfigurationRepositorio;
        private IPostACHRepositorio postACHRepositorio;
        private IPostAccountingConfigurationRepositorio postAccountingConfigurationRepositorio;
        private ITransactionCodeRepositorio transactionCodeRepositorio;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_iInterfaceSystemRepositorio"></param>
        /// <param name="_mapper"></param>
        public FileServicioParallel(IFileRepositorio _fileRepositorio,
                            IPaybankConfigurationRepositorio _paybankConfigurationRepositorio,
                            ICountryRepositorio _countryRepositorio,
                            ITransactionTypeConfigurationRepositorio _transactionTypeConfigurationRepositorio,
                            IMessageQueueFactory _messageQueueFactory,
                            IAdaptadorFile _adaptadorFile,
                            IErrorMessageRepositorio _errorMessageRepositorio,
                            IReturnCodeMappingConfiguratonRepositorio _returnCodeMappingConfigurationRepositorio,
                            IMapper _mapper,
                            ITransactionTypeBatchConfigurationRepositorio _transactionTypeBatchConfigurationRepositorio,
                            IBankRepositorio _bankRepositorio,
                            IEventOnlineLogRepositorio _eventOnlineLogRepositorio,
                            ITransactionTypeConfigurationCountryRepositorio _transactionTypeConfigurationCountryRepositorio,
                            ICurrencyRepositorio _currencyRepositorio,
                            IPaybankRepositorio _paybankRepositorio,
                            IProcessingFileLogRepositorio _processingFileLogRepositorio,
                            IEmailServicio _emailServicio,
                            ITransactionRepositorio _transactionRepositorio,
                            ISchedulerPostACHConfigurationRepositorio _schedulerPostACHConfigurationRepositorio,
                            ICitiscreeningConfigurationRepositorio _citiscreeningConfigurationRepositorio,
                            IPostACHRepositorio _postACHRepositorio,
                            IPostAccountingConfigurationRepositorio _postAccountingConfigurationRepositorio,
                            ITransactionCodeRepositorio _transactionCodeRepositorio)
        {
            this.fileRepositorio = _fileRepositorio;
            this.paybankConfigurationRepositorio = _paybankConfigurationRepositorio;
            this.countryRepositorio = _countryRepositorio;
            this.transactionTypeConfigurationRepositorio = _transactionTypeConfigurationRepositorio;
            this.messageQueueFactory = _messageQueueFactory;
            this.adaptadorFile = _adaptadorFile;
            this.errorMessageRepositorio = _errorMessageRepositorio;
            this.returnCodeMappingConfigurationRepositorio = _returnCodeMappingConfigurationRepositorio;
            this.mapper = _mapper;
            this.transactionTypeBatchConfigurationRepositorio = _transactionTypeBatchConfigurationRepositorio;
            this.bankRepositorio = _bankRepositorio;
            this.eventOnlineLogRepositorio = _eventOnlineLogRepositorio;
            this.transactionTypeConfigurationCountryRepositorio = _transactionTypeConfigurationCountryRepositorio;
            this.currencyRepositorio = _currencyRepositorio;
            this.paybankRepositorio = _paybankRepositorio;
            this.processingFileLogRepositorio = _processingFileLogRepositorio;
            this.emailServicio = _emailServicio;
            this.transactionRepositorio = _transactionRepositorio;
            this.schedulerPostACHConfigurationRepositorio = _schedulerPostACHConfigurationRepositorio;
            this.citiscreeningConfigurationRepositorio = _citiscreeningConfigurationRepositorio;
            this.postACHRepositorio = _postACHRepositorio;
            this.postAccountingConfigurationRepositorio = _postAccountingConfigurationRepositorio;
            this.transactionCodeRepositorio = _transactionCodeRepositorio;
        }

        #endregion

        #region Métodos Base

        public Base.FileDTO Crear(Base.FileDTO entidadACrear, string userSoeid)
        {
            throw new NotImplementedException();
        }

        public void Editar(Base.FileDTO entidadAEditar, string userSoeid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Base.FileDTO> Listar()
        {
            throw new NotImplementedException();
        }

        public void Eliminar(Base.FileDTO entidadAEliminar, string userSoeid)
        {
            throw new NotImplementedException();
        }

        public void Eliminar(int id, string userSoeid)
        {
            throw new NotImplementedException();
        }

        public Base.FileDTO Encontrar(int id)
        {
            var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == id, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType,  Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.Country.QueueConnectivities, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

            var postAccountingConfiguration = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.Country.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

            return null;
        }

        public IEnumerable<Base.FileDTO> Buscar(string consultaSQL, params object[] parametros)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>             
        public int ObtenerTransactionTypeConfigurationIdByFile(int fileId)
        {
            try
            {
                var fileOriginal = this.fileRepositorio.Obtener(p => p.Id == fileId, null, "TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry").FirstOrDefault();

                return fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfigurationId;
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
        /// Método que obtiene los archivos existentes dependiendo de la fecha de creación y de la fecha de filtro que se haya seleccionado
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="countryId"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="userSoeid"></param>
        /// <returns></returns>
        public IEnumerable<FileGridBodyDTO> ObtenerArchivosExistentesPorFecha(DateTime fecha, int countryId, int currencyId, int currentCulture, string userSoeid)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);

            using (traceManager.StartTrace("Post Accounting Import Error", newGuid))
            {
                try
                {
                    var countryOriginal = this.countryRepositorio.Encontrar(countryId);
                    var currencyOriginal = this.currencyRepositorio.Encontrar(currencyId);

                    var paybankReturnFile = countryOriginal.PaybankReturnFileConfigurations.FirstOrDefault(p => p.CountryId == countryId && p.CurrencyId == currencyId);

                    var listaOriginal = this.fileRepositorio.Obtener(p => p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryId && DbFunctions.TruncateTime(p.DateSelected) == DbFunctions.TruncateTime(fecha) && p.StateId != FileState.Complete && p.Batchs.Any(c => c.Transactions.Any(d => !d.SettlementSend.HasValue || (d.SettlementSend.HasValue && d.SettlementSend.Value == false))), null, "TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batchs.Transactions.TransactionEventOnlineLogs.ErrorMessage, Batchs.Transactions.TransactionTrandeFiles.TrandeFile").ToList();

                    var listaIds = VerificarFinalStatus(listaOriginal, paybankReturnFile, 0, countryId);

                    if (listaIds.Count() > 0)
                        listaOriginal.RemoveAll(p => listaIds.Contains(p.Id));

                    var originalPaybankConfiguration = this.paybankConfigurationRepositorio.Obtener(p => p.PaybankConfigurationCountries.Any(c => c.CountryId == countryId) && p.PaybankConfigurationCurrencies.Any(c => c.CurrencyId == currencyId));
                    var pathPaybankInclearing = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathInclearing;
                    var pathPaybankReturn = originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currencyId).FirstOrDefault().FolderPathReturn;

                    var listPath = new List<string> { pathPaybankInclearing, pathPaybankReturn };

                    var iterator = 0;

                    foreach (var item in listPath)
                    {
                        //var rutaFinal = item.Replace("D:", "C:");

                        var pathPaybankProcessed = GetPaybankProcessedFiles(countryOriginal, currencyOriginal, iterator, originalPaybankConfiguration);

                        //pathPaybankProcessed = pathPaybankProcessed.Replace("D:", "C:");

                        var pathProcessedFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathPaybankProcessed);

                        var directoryInfoProcessedFile = new DirectoryInfo(pathProcessedFiles);
                        var directoryInfoPaybankFile = new DirectoryInfo(item);

                        var searchPattern = DominioImportLogica.GetPaybankSearchPattern(currencyId, originalPaybankConfiguration, iterator);

                        var listFilesProcessedFiles = directoryInfoProcessedFile.GetFiles("*.*", System.IO.SearchOption.TopDirectoryOnly);
                        var listFilesPaybankFiles = directoryInfoPaybankFile.GetFiles(searchPattern, System.IO.SearchOption.TopDirectoryOnly);

                        var onlyInPaybank = listFilesPaybankFiles.Except(listFilesProcessedFiles, new FileCompare()).Select(p => p);

                        if (onlyInPaybank != null && onlyInPaybank.Count() > 0)
                            ProcesarArchivoPaybankExistencia(countryOriginal, onlyInPaybank, listaOriginal, fecha, currencyId, currentCulture, userSoeid, newGuid);

                        iterator++;
                    }

                    IEnumerable<FileGridBodyDTO> tarea = this.adaptadorFile.FabricarFileGridBodyDTO(listaOriginal, countryOriginal);

                    return tarea;
                }            
                catch (DirectoryNotFoundException dirEx)
                {                    
                    throw ProveedorExcepciones.ManejaExcepcion(dirEx, "PoliticaPostAccountingDirectoryNotFound");
                }
                catch (UnauthorizedAccessException unEx)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(unEx, "PoliticaPostAccountingUnauthorizedAccess");
                }              
                catch (Exception exception)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(exception, "PoliticaBaseExistenciaImportError");
                }
                finally
                {
                    this.fileRepositorio.UnidadDeTrabajo.Confirmar();
                    this.processingFileLogRepositorio.UnidadDeTrabajo.Confirmar();
                }
            }
        }

        /// <summary>
        /// étodo 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BatchBankGridBodyDTO>> ObtenerBatchPorBankIdFileName(int? id, string fileName)
        {
            try
            {
                var listaBatchBankList = new HashSet<BatchBankGridBodyDTO>();

                if (!id.HasValue)
                {
                    var dictionary = (Dictionary<string, List<int?>>)HttpContext.Current.Session["BankDictionary"];
                    var arrIdbank = dictionary[fileName];

                    var bankList = this.bankRepositorio.Obtener(p => p.FieldStatusId == FieldStatus.Active && arrIdbank.Contains(p.Id));

                    return await this.adaptadorFile.FabricarBatchBankGridBodyDTO(bankList);
                }
                else
                {
                    var fileOriginal = this.fileRepositorio.Obtener(p => p.Id == id, null, "Batchs.Transactions.Bank").FirstOrDefault();
                    var bankList = fileOriginal.Batchs.SelectMany(p => p.Transactions).Select(p => p.Bank).GroupBy(p => p.Id).Select(c => new { Key = c.Key, ListBank = c.FirstOrDefault() });

                    var intArr = bankList.Select(p => p.ListBank).ToList();

                    return await this.adaptadorFile.FabricarBatchBankGridBodyDTO(intArr);
                }
            }
            catch (AggregateException ae)
            {
                throw ProveedorExcepciones.ManejaExcepcion(ae.InnerException, "PoliticaBase");
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
        /// Método que realiza el proceso Import de un archivo de pago en especifico
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stateId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ProcesoImportFileFromPaybank(string fileName, FileState stateId, int countryId, int currencyId, string userSoeid, int currentCulture, string identifier, DateTime dateSelected, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();

            var logWritter = Logger.Writer;

            var traceManager = new TraceManager(logWritter);

            using (traceManager.StartTrace("Post Accounting Import Error", newGuid))
            {
                try
                {
                    var createdFileId = 0;

                    var countryOriginal = this.countryRepositorio.Encontrar(countryId);
                    var currencyOriginal = this.currencyRepositorio.Encontrar(currencyId);

                    var originalPaybankConfiguration = this.paybankConfigurationRepositorio.Obtener(p => p.PaybankConfigurationCountries.Any(c => c.CountryId == countryId) && p.PaybankConfigurationCurrencies.Any(c => c.CurrencyId == currencyId));
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

                    var pathProcessed = (identifier == "incomingelectronic" || identifier == "inclearingcheck") ? GetPaybankProcessedFiles(countryOriginal, currencyOriginal, 0, originalPaybankConfiguration) : GetPaybankProcessedFiles(countryOriginal, currencyOriginal, 1, originalPaybankConfiguration);

                    var pathProcessedFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathProcessed);

                    var directoryInfoProcessedFile = new DirectoryInfo(pathProcessedFiles);
                    var directoryInfoPaybankFile = new DirectoryInfo(rutaFinal);

                    var searchPattern = DominioImportLogica.GetPaybankSearchPattern(currencyId, originalPaybankConfiguration, iterator);

                    var listFilesPaybankFiles = directoryInfoPaybankFile.GetFiles(searchPattern, System.IO.SearchOption.TopDirectoryOnly);

                    var fileSelected = listFilesPaybankFiles.Where(p => p.Name == fileName).FirstOrDefault();

                    if (stateId == FileState.New)
                    {
                        createdFileId = ProcesarArchivoNuevoPaybank(fileSelected, countryId, currencyId, userSoeid, currentCulture, pathProcessedFiles, newGuid, dateSelected, executionMode);
                    }

                    return createdFileId;
                }               
                catch (DirectoryNotFoundException dirEx)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(dirEx, "PoliticaPostAccountingDirectoryNotFound");
                }
                catch (UnauthorizedAccessException unEx)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(unEx, "PoliticaPostAccountingUnauthorizedAccess");
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(dbException, "PoliticaDependenciaDatos");
                }
                catch (Exception exception)
                {
                    throw ProveedorExcepciones.ManejaExcepcion(exception, "PoliticaBaseImportError");
                }
                finally
                {
                    this.processingFileLogRepositorio.UnidadDeTrabajo.Confirmar();
                }
            }
        }

        #endregion

        #region Inclearing True Transaction - Return

        /// <summary>
        /// Método que realiza el proceso de verificación GI - Citiscreening
        /// </summary>
        /// <param name="fileId"></param>
        public void ProcesoCitiscreeningValidation(int fileId, int countryId, int currencyId, string userSoeid, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);            
            var fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Citiscreening Error", newGuid))
            {
                try
                {
                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.CitiscreeningValidation.HasValue && transactionTypeConfigurationCountry.CitiscreeningValidation.Value == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.CitiscreeningError || c.StatusId == TransactionState.Import))
                        {
                            var queueConnectivity = this.citiscreeningConfigurationRepositorio.Obtener(p => p.CountryId == countryId, null, "CitiscreeningConfigurationCitiscreeningFields.CitiscreeningField").FirstOrDefault();

                            InitializeCitiscreeningConnection(queueConnectivity);

                            EnvioTransaccionesCitiscreening(listaTransaction, queueConnectivity, userSoeid, newGuid);                            

                            if (listaTransaction.Any(p => p.StatusId == TransactionState.CitiscreeningPending))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.CitiscreeningManualValidationPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.CitiscreeningManualValidationPending, EventType.CitiscreeningValidation));
                            }
                            else if (listaTransaction.All(p => p.StatusId == TransactionState.CitiscreeningOk))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.NoHitsDuringCitiscreeningValidation.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NoHitsDuringCitiscreeningValidation, EventType.CitiscreeningValidation));
                            }
                            else if (listaTransaction.All(p => p.StatusId == TransactionState.CitiscreeningError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateInCitiscreening.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                            }
                            else if (listaTransaction.Any(p => p.StatusId == TransactionState.CitiscreeningError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.CitiscreeningAutomaticValidationPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.CitiscreeningAutomaticValidationPending, EventType.CitiscreeningValidation));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.CitiscreeningValidation);
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                    throw newEx;
                }
                catch (CitiscreeningLargeFormatMessageException larForEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(larForEx, "PoliticaCitiscreeningLargeFormatMessageError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1033", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseCitiscreeningError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1014", fileName, fileId, newGuid, ProcessFlag.UnableToValidateInCitiscreening, EventType.CitiscreeningValidation));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
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
            var listProcessingLog = new List<ProcessingFileLog>();
            Country countryOriginal = null;
            Dominio.Core.File fileOriginal = null;    

            using (traceManager.StartTrace("Post Accounting Open Batch Error", newGuid))
            {                
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();
                    fileOriginal = this.fileRepositorio.Obtener(p => p.Id == fileId, null, "Batchs.Transactions, TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry, TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations").FirstOrDefault();

                    var transactionTypeConfigurationCountry = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (fileOriginal.Batchs.SelectMany(p => p.Transactions).Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                        {
                            var transactionTypeBatchConfiguration = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && p.Id == batchId);                            
                           
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            IniciarProcesoOpenBatch(countryOriginal, queueConnectivity, userSoeid, transactionTypeBatchConfiguration, currentCulture, newGuid, fileOriginal, currencyId, listProcessingLog);                            
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileOriginal.Name, fileId, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseOpenBatchError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1015", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }
                finally
                {
                    this.countryRepositorio.Actualizar(countryOriginal);
                    this.countryRepositorio.UnidadDeTrabajo.Confirmar();

                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
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

            var transactionTypeBatchOriginal = this.transactionTypeBatchConfigurationRepositorio.Obtener(p => p.FieldStatusId == FieldStatus.Active && p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryId && p.IsOpen == true);

            transactionTypeBatchOriginal.ToList().ForEach(p => transactionTypeConfigurationBatchIds.Add(p.Id));

            return transactionTypeConfigurationBatchIds;
        }

        /// <summary>
        /// Método que realiza el proceso de Post Accounting Upload
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="batchId"></param>
        public void ProcesoPostAccountingUpload(int fileId, int countryId, string userSoeid, int batchId, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Upload Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "Banks, CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations ,Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {                            
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();                            

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesUpload(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, listProcessingLog);
                           
                            if (listaTransaction.All(p => p.StatusId == TransactionState.UploadToCosmosButNotAuthorized || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SucessfullyUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SucessfullyUploadToCosmos, EventType.Upload));
                            }
                            else if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UploadToCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UploadToCosmosPending, EventType.Upload));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Upload);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToUploadToCosmos, EventType.Upload));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }                
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseUploadError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1017", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realizar el proceso de Post Accounting Authorize
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        public void ProcesoPostAccountingAuthorize(int fileId, int countryId, string userSoeid, int batchId, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Authorize Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "Banks, CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesAuthorize(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, listProcessingLog);

                            if (listaTransaction.All(p => p.StatusId == TransactionState.AuthorizeByCosmosOk || p.StatusId == TransactionState.AuthorizeByCosmosRejected || p.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyAuthorizeInCosmos, EventType.Authorize));
                            }
                            else if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.AuthorizationInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.AuthorizationInCosmosPending, EventType.Authorize));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Authorize);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToAuthorizeInCosmos, EventType.Authorize));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseAuthorizeError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1018", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
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
        public void ProcesoPostAccountingDelete(int fileId, int countryId, string userSoeid, int batchId, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Country countryOriginal = null;
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Delete Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "Banks, CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesDelete(listaTransaction, countryOriginal, queueConnectivity, userSoeid, batchId, newGuid, currencyId, listProcessingLog);

                            if (listaTransaction.Any(c => c.TransactionEventOnlineLogs.Any(d => d.EventOnlineLog != null && d.EventOnlineLog.EventTypeId == EventType.Delete)))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyDeleteInCosmos, EventType.Delete));
                            }
                            else 
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.DeleteInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.DeleteInCosmosPending, EventType.Delete));
                            }                           

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Delete);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToDeleteInCosmos, EventType.Delete));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseDeleteError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1019", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso de creación del archivo de retorno a paybank
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="currentCulture"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingCreatePaybankReturnFile(int fileId, int countryId, string userSoeid, int currentCulture, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            IEnumerable<Transaction> listaTransaction = null;
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Return File Error", newGuid))
            {
                try
                {
                    var countryOriginal = this.countryRepositorio.Encontrar(countryId);

                    listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, TransactionEventOnlineLogs.ErrorMessage, TransactionTrandeFiles.TrandeFile, TransactionEventOnlineLogs.EventOnlineLog, PendingChangeCitiscreenings.RejectCode");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.ReturnFile.HasValue && transactionTypeConfigurationCountry.ReturnFile == true)
                    {
                        var paybankReturnFile = countryOriginal.PaybankReturnFileConfigurations.FirstOrDefault(p => p.CountryId == countryId && p.CurrencyId == currencyId);

                        if (paybankReturnFile == null)
                        {
                            var mensaje = currentCulture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, listaTransaction.FirstOrDefault().Batch.File.Name, listaTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                            return;
                        }

                        CreatePaybankReturnFile(listaTransaction, countryOriginal, paybankReturnFile, userSoeid, currencyId, newGuid, currentCulture, listProcessingLog);

                        if (listaTransaction.Any(q => q.TransactionTrandeFiles.Any(p => p.Id == 0)))
                            this.postACHRepositorio.GenericUpdateTransactionPerProcessTrande(listaTransaction.ToArray(), true, EventType.PaybankReturnFile);
                    }
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseReturnFileError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1020", fileName, fileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));
                    throw newEx;
                }
                finally
                {                      
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso de creación del archivo FACP para paylink
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        public void ProcesoPostAccountingCreateFACPReturnFile(int fileId, int countryId, string userSoeid, int currencyId, int currentCulture)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            IEnumerable<Transaction> listaTransaction = null;
            string fileName = string.Empty;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("FACP File Error", newGuid))
            {
                try
                {
                    var countryOriginal = this.countryRepositorio.Encontrar(countryId);
                    var currencyOriginal = this.currencyRepositorio.Encontrar(currencyId);

                    listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, TransactionEventOnlineLogs.ErrorMessage, TransactionTrandeFiles.TrandeFile, TransactionEventOnlineLogs.EventOnlineLog, PendingChangeCitiscreenings.RejectCode");

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostPaylink == true)
                    {
                        var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostPaylinkConfiguration>().Where(p => p.CurrencyId == currencyId).FirstOrDefault();

                        if (queueConnectivity == null)
                        {
                            var mensaje = currentCulture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, listaTransaction.FirstOrDefault().Batch.File.Name, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                            return;
                        }                        

                        CreateFACPReturnFile(listaTransaction, countryOriginal, queueConnectivity, userSoeid, currencyOriginal, newGuid, listProcessingLog);

                        if (listaTransaction.Any(q => q.TransactionTrandeFiles.Any(p => p.Id == 0)))
                            this.postACHRepositorio.GenericUpdateTransactionPerProcessTrande(listaTransaction.ToArray(), true, EventType.FACPFile);
                    }
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaFACPFileError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1011", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que obtiene el transactionTypeConfiguration asociado a un archivo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TransactionTypeConfigurationCountryDTO ObtenerTransactionTypeConfigurationCountryFromFile(int? id, string identifier, int countryId)
        {
            try
            {
                Dominio.Core.File fileOriginal = null;
                TransactionTypeConfigurationCountry transactionTypeConfigurationCountry = null;

                if (id.HasValue)
                {
                    fileOriginal = this.fileRepositorio.Obtener(p => p.Id == id.Value, null, "TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.InterfaceSystem.SystemParameter").FirstOrDefault();
                    transactionTypeConfigurationCountry = fileOriginal.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;
                }
                else
                {
                    transactionTypeConfigurationCountry = this.transactionTypeConfigurationCountryRepositorio.ObtenerPrimero(p => p.CountryId == countryId && p.TransactionTypeConfiguration.TransactionType.Identifier == identifier);
                }

                return this.adaptadorFile.FabricarTransactionTypeConfigurationCountryFromFileDTO(transactionTypeConfigurationCountry);
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

        #region Inclearing Check

        /// <summary>
        /// Método que realiza el proceso Open Batch de Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currentCulture"></param>
        /// <param name="currencyId"></param>
        public void ProcesoPostAccountingOpenBatchInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currentCulture, int currencyId)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            Dominio.Core.File fileOriginal = null;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Open Batch Error", newGuid))
            {                
                try
                {
                    var countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();
                    fileOriginal = this.fileRepositorio.Obtener(p => p.Id == fileId, null, "Batchs.Transactions.Bank.BankBatchBankEventOnlineLogs.BatchBankEventOnlineLog").FirstOrDefault();

                    if (fileOriginal.Batchs.SelectMany(p => p.Transactions).Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                    {
                        if (countryOriginal.TransactionTypeConfigurationCountries.Any(p => p.PostAccounting == true))
                        {                           
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            IniciarProcesoOpenBatchInclearingCheck(fileOriginal, countryOriginal, queueConnectivity, userSoeid, check, currentCulture, newGuid, currencyId, listProcessingLog);
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileOriginal.Name, fileId, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseOpenBatchError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1015", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    throw newEx;
                }
                finally
                {
                    this.fileRepositorio.Actualizar(fileOriginal);
                    this.fileRepositorio.UnidadDeTrabajo.Confirmar();

                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Upload para el Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        public void ProcesoPostAccountingUploadInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Upload Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();
                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Bank, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesUploadInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, listProcessingLog);

                            if (listaTransaction.All(p => p.StatusId == TransactionState.UploadToCosmosButNotAuthorized || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SucessfullyUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SucessfullyUploadToCosmos, EventType.Upload));
                            }
                            else if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.UploadToCosmos))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UploadToCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.UploadToCosmosPending, EventType.Upload));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Upload);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToUploadToCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToUploadToCosmos, EventType.Upload));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseUploadError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1017", fileName, fileId, newGuid, ProcessFlag.UnableToUploadToCosmos, EventType.Upload));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Authorize Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        /// <param name="executionMode"></param>
        public void ProcesoPostAccountingAuthorizeInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Authorize Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();
                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Bank, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesAuthorizeInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, listProcessingLog);

                            if (listaTransaction.All(p => p.StatusId == TransactionState.AuthorizeByCosmosOk || p.StatusId == TransactionState.AuthorizeByCosmosRejected || p.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || p.StatusId == TransactionState.UploadToCosmosRejected || p.StatusId == TransactionState.UploadToCosmosClientAccountError || p.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyAuthorizeInCosmos, EventType.Authorize));
                            }
                            else if (listaTransaction.Any(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized || c.StatusId == TransactionState.Authorize))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.AuthorizationInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.AuthorizationInCosmosPending, EventType.Authorize));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Authorize);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToAuthorizeInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToAuthorizeInCosmos, EventType.Authorize));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseAuthorizeError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1018", fileName, fileId, newGuid, ProcessFlag.UnableToAuthorizeInCosmos, EventType.Authorize));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Método que realiza el proceso Delete Inclearing Check
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        /// <param name="check"></param>
        /// <param name="currencyId"></param>
        /// <param name="executionMode"></param>
        public void ProcesoPostAccountingDeleteInclearingCheck(int fileId, int countryId, string userSoeid, DailyBrand check, int currencyId, ExecutionMode executionMode)
        {
            var newGuid = Guid.NewGuid();
            var logWritter = Logger.Writer;
            var traceManager = new TraceManager(logWritter);
            string fileName = string.Empty;
            Country countryOriginal = null;
            var listProcessingLog = new List<ProcessingFileLog>();

            using (traceManager.StartTrace("Post Accounting Delete Error", newGuid))
            {
                try
                {
                    countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser, Banks").FirstOrDefault();
                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Bank, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType, Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings, Batch.File.TransactionTypeConfigurationCountryCurrency.Currency, Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes, TransactionEventOnlineLogs.ErrorMessage, TransactionEventOnlineLogs.EventOnlineLog");

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    if (transactionTypeConfigurationCountry.PostAccounting.HasValue && transactionTypeConfigurationCountry.PostAccounting == true)
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                            InitializePostAccountingConnection(queueConnectivity);

                            EnvioTransaccionesDeleteInclearingCheck(listaTransaction, countryOriginal, queueConnectivity, userSoeid, check, newGuid, currencyId, listProcessingLog);

                            if (listaTransaction.Any(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.DeleteInCosmosPending.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.DeleteInCosmosPending, EventType.Delete));
                            }
                            else if (listaTransaction.Any(p => p.StatusId == TransactionState.DeletedInCosmosOk || p.StatusId == TransactionState.DeletedInCosmosClientAccountError || p.StatusId == TransactionState.DeletedInCosmosHoldingAccountError))
                            {
                                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SuccessfullyDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.SuccessfullyDeleteInCosmos, EventType.Delete));
                            }

                            this.postACHRepositorio.GenericUpdateTransactionPerProcess(listaTransaction.ToArray(), true, EventType.Delete);
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToDeleteInCosmos.GetDescription(), null, fileName, fileId, newGuid, ProcessFlag.NothingToDeleteInCosmos, EventType.Delete));
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                catch (ConfigurationExcepcion confEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(confEx, "PoliticaConfigurationError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1032", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseDeleteError");
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1019", fileName, fileId, newGuid, ProcessFlag.UnableToDeleteInCosmos, EventType.Delete));
                    throw newEx;
                }
                finally
                {
                    if (listProcessingLog.Any())
                        this.postACHRepositorio.BulkInsertProcessingFileLog(listProcessingLog.ToArray(), false);
                }
            }
        }

        #endregion

        #region Trande Flexcube

        /// <summary>
        /// Método que realiza el proceso de creación del archivo Trande Flexcube
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
            Dominio.Core.File fileOriginal = null;
            string fileName = null;

            using (traceManager.StartTrace("Trande Flexcube File Error", newGuid))
            {
                try
                {
                    var countryOriginal = this.countryRepositorio.Encontrar(countryId);

                    var listaTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.InterfaceSystem.SystemParameter");

                    var transactionTypeConfigurationCountry = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

                    fileName = listaTransaction.FirstOrDefault().Batch.File.Name;

                    if (transactionTypeConfigurationCountry.InterfaceSystem.SystemParameter.Identifier == "flexcube")
                    {
                        if (listaTransaction.Any(c => c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk))
                        {
                            var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingFlexcubeConfiguration>().Where(p => p.CurrencyOriginalId == currencyId && p.ShippingMethod.Identifier == "trande").FirstOrDefault();

                            if (queueConnectivity == null)
                            {
                                var mensaje = culture == 0 ? "There is no associated configuration for the process." : "No existe configuración asociada para realizar el proceso.";

                                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, mensaje, null, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

                                return;
                            }

                            CreateTrandeFlexCubeFile(listaTransaction, queueConnectivity, currencyId, userSoeid, countryId, newGuid, culture);

                            listaTransaction.ToList().ForEach(p => this.transactionRepositorio.Actualizar(p));
                            this.transactionRepositorio.UnidadDeTrabajo.Confirmar();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));
                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseTrandeFlexcubeFileError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1012", fileName, fileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));
                    throw newEx;
                }
                finally
                {
                    this.processingFileLogRepositorio.UnidadDeTrabajo.Confirmar();
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
            IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration = null;
            IEnumerable<ErrorMessage> errorMessages = null;

            using (traceManager.StartTrace("Post Accounting Close Batch Error", newGuid))
            {
                try
                {
                    var countryOriginal = this.countryRepositorio.Obtener(p => p.Id == countryId, null, "CountryCosmosFunctionalUsers.CosmosFunctionalUser").FirstOrDefault();

                    errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

                    batchConfiguration = this.transactionTypeBatchConfigurationRepositorio.Obtener(p => p.FieldStatusId == FieldStatus.Active && batchIds.Contains(p.Id), null, "BatchConfigurationBatchEventOnlineLogs.BatchEventOnlineLog");                    

                    if (batchConfiguration.Any(p => p.IsOpen))
                    {
                        var queueConnectivity = countryOriginal.QueueConnectivities.OfType<PostAccountingConfiguration>().Where(p => p.ShippingMethod.Identifier == "xmllat009").FirstOrDefault();

                        InitializePostAccountingConnection(queueConnectivity);

                        IniciarProcesoCloseBatch(countryOriginal, queueConnectivity, userSoeid, batchConfiguration, currentCulture, newGuid, currencyId, errorMessages);

                        if (userSoeid == "Scheduler")
                        {
                            var emailConfiguration = countryOriginal.EmailConfiguration;

                            if (emailConfiguration != null)
                            {
                                this.emailServicio.EnviarEmail(emailConfiguration, "Email Scheduler Close Batch", "TemplateEmailCloseBatch", batchConfiguration, userSoeid);
                            }
                        }

                    }
                    else
                    {
                        if (userSoeid == "Scheduler")
                        {
                            var emailConfiguration = countryOriginal.EmailConfiguration;

                            if (emailConfiguration != null)
                            {
                                this.emailServicio.EnviarEmail(emailConfiguration, "Process ACH Scheduler", "No data found.", userSoeid);
                            }
                        }
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(mqEx, "PoliticaQueueConnectionError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1021", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, mqEx.ReasonCode, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    throw newEx;
                }               
                catch (DbUpdateConcurrencyException dbException)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(dbException, "PoliticaDependenciaDatos");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "990", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, 990, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    throw newEx;
                }
                catch (Exception exception)
                {
                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(exception, "PoliticaBaseCloseBatchError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, newEx.Message, "1016", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    batchConfiguration.ToList().ForEach(p => IngresarEventoTransaccionCloseBatchError(p, errorMessages, 1016, ProcessFlag.UnableToCloseBatch.GetDescription(), newGuid, null, newEx.Message, userSoeid, null));

                    throw newEx;
                }
                finally
                {
                    this.transactionTypeBatchConfigurationRepositorio.UnidadDeTrabajo.Confirmar();
                    this.processingFileLogRepositorio.UnidadDeTrabajo.Confirmar();
                }
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
        public IEnumerable<FileResumenGridBodyDTO> ProcesoPostAccountingFilesResume(int fileId)
        {
            var listaOriginal = this.fileRepositorio.Obtener(p => p.Id == fileId, null, "BatchConfigurationBatchEventOnlineLogs.BatchEventOnlineLog,BankBatchBankEventOnlineLogs.BatchBankEventOnlineLog,Batchs.Transactions.TransactionEventOnlineLogs.EventOnlineLog");

            var fileResumenList = this.adaptadorFile.FabricarFileResumenGridBodyDTO(listaOriginal);

            return fileResumenList;
        }

        /// <summary>
        /// Método que retorna el resumen del proceso del archivo
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userSoeid"></param>
        /// <returns></returns>
        public IEnumerable<FileResumenGridBodyDTO> ProcesoPostAccountingFilesResume(List<int> fileIdsList)
        {
            using (ContextoPrincipal contexto = new ContextoPrincipal())
            {
                var listaOriginal = contexto.File.Where(p => fileIdsList.Contains(p.Id));
                var fileResumenList = this.adaptadorFile.FabricarFileResumenGridBodyDTO(listaOriginal);

                return fileResumenList;
            }
        }

        /// <summary>
        /// Método que retorna el resumen del proceso del archivo
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userSoeid"></param>
        /// <returns></returns>
        public IEnumerable<FileDetalleGridBodyDTO> ProcesoPostAccountingFilesDetalle(int fileId, string etapaProceso)
        {
            var listaOriginal = this.fileRepositorio.Encontrar(fileId);

            var fileResumenList = this.adaptadorFile.FabricarFileDetalleGridBodyDTO(listaOriginal, etapaProceso);

            return fileResumenList;
        }

        /// <summary>
        /// Método que obtiene el summary para el proceso citiscreening
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<CitiscreeningFileDetailGridBodyDTO> ObtenerProcessingFileCitiscreeningDetail(int fileId)
        {
            var returnValue = new List<CitiscreeningFileDetailGridBodyDTO>();

            try
            {
                var listTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "TransactionEventOnlineLogs.EventOnlineLog");

                if (!listTransaction.Any(p => p.StatusId == TransactionState.CitiscreeningPending || p.StatusId == TransactionState.CitiscreeningError || p.StatusId == TransactionState.CitiscreeningRejected))
                    return returnValue;

                var listTransactionPending = listTransaction.Where(p => p.StatusId == TransactionState.CitiscreeningPending || p.StatusId == TransactionState.CitiscreeningError || p.StatusId == TransactionState.CitiscreeningRejected);

                foreach (var transactionPending in listTransactionPending)
                {
                    var lastEventOnlineLog = transactionPending.TransactionEventOnlineLogs.OrderByDescending(p => p.Id).FirstOrDefault().EventOnlineLog;

                    returnValue.Add(new CitiscreeningFileDetailGridBodyDTO
                    {
                        Id = transactionPending.Id,
                        AccountNumber = transactionPending.AccountNumber,
                        Amount = transactionPending.Amount,
                        BeneficiaryName = transactionPending.BeneficiaryName,
                        OriginatorAccount = transactionPending.OriginatorAccount,
                        Date = lastEventOnlineLog.Date,
                        MessageSend = lastEventOnlineLog.MessageSend,
                        MessageReceived = lastEventOnlineLog.MessageReceived,
                        StateId = transactionPending.StatusId
                    });
                }

                return returnValue;
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
        /// Método que obtiene el summary para el proceso Post Accounting
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<PostAccountingFileDetailGridBodyDTO> ObtenerProcessingFilePostAccountingDetail(int fileId)
        {
            var returnValue = new List<PostAccountingFileDetailGridBodyDTO>();

            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            try
            {
                var fileOriginal = this.fileRepositorio.Obtener(p => p.Id == fileId, null, "Batchs.Transactions.TransactionEventOnlineLogs.EventOnlineLog, Batchs.Transactions.TransactionEventOnlineLogs.ErrorMessage, BankBatchBankEventOnlineLogs.BatchBankEventOnlineLog, BankBatchBankEventOnlineLogs.ErrorMessage, BatchConfigurationBatchEventOnlineLogs.BatchEventOnlineLog, BatchConfigurationBatchEventOnlineLogs.ErrorMessage").FirstOrDefault();

                //Batch                

                var openBatchErrores = fileOriginal.BatchConfigurationBatchEventOnlineLogs.Where(p => p.BatchEventOnlineLog.EventTypeId == EventType.OpenBatch && p.BatchEventOnlineLog.EventStateId == EventState.Incomplete).Where(p => p.ErrorMessageId.HasValue);

                if (openBatchErrores.Any())
                {
                    if (openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.Code != "50")
                    {
                        var lastBatchEventOnlineLog = openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().BatchEventOnlineLog;

                        returnValue.Add(new PostAccountingFileDetailGridBodyDTO
                        {
                            Id = lastBatchEventOnlineLog.Id,
                            EventType = lastBatchEventOnlineLog.EventTypeId.GetDescription(),
                            AccountNumber = "",
                            Amount = (decimal?)null,
                            BeneficiaryName = "",
                            OriginatorAccount = "",
                            Date = lastBatchEventOnlineLog.Date,
                            StateId = lastBatchEventOnlineLog.EventStateId.GetDescription(),
                            ErrorCode = openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessageId.HasValue ? openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.Code : null,
                            ErrorMessage = openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessageId.HasValue ? (culture == 0 ? openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.EnglishText : openBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.SpanishText) : lastBatchEventOnlineLog.MessageReceived
                        });
                    }
                }

                //Bank Batch

                var openBankBatchErrores = fileOriginal.BankBatchBankEventOnlineLogs.Where(p => p.BatchBankEventOnlineLog.EventTypeId == EventType.OpenBatch && p.BatchBankEventOnlineLog.EventStateId == EventState.Incomplete).Where(p => p.ErrorMessageId.HasValue);

                if (openBankBatchErrores.Any())
                {
                    if (openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.Code != "50")
                    {
                        var lastBatchBankEventOnlineLog = openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().BatchBankEventOnlineLog;

                        returnValue.Add(new PostAccountingFileDetailGridBodyDTO
                        {
                            Id = lastBatchBankEventOnlineLog.Id,
                            EventType = lastBatchBankEventOnlineLog.EventTypeId.GetDescription(),
                            AccountNumber = "",
                            Amount = (decimal?)null,
                            BeneficiaryName = "",
                            OriginatorAccount = "",
                            Date = lastBatchBankEventOnlineLog.Date,
                            StateId = lastBatchBankEventOnlineLog.EventStateId.GetDescription(),
                            ErrorCode = openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessageId.HasValue ? openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.Code : null,
                            ErrorMessage = openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessageId.HasValue ? (culture == 0 ? openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.EnglishText : openBankBatchErrores.OrderByDescending(p => p.Id).FirstOrDefault().ErrorMessage.SpanishText) : lastBatchBankEventOnlineLog.MessageReceived
                        });
                    }
                }

                var arrStates = new[] { TransactionState.UploadToCosmos, TransactionState.UploadToCosmosRejected, TransactionState.UploadToCosmosClientAccountError, TransactionState.UploadToCosmosHoldingAccountError, TransactionState.Authorize, TransactionState.AuthorizeByCosmosRejected, TransactionState.AuthorizeByCosmosClientAccountError, TransactionState.AuthorizeByCosmosHoldingAccountError, TransactionState.DeletedInCosmosClientAccountError, TransactionState.DeletedInCosmosHoldingAccountError, TransactionState.DeletedInCosmosOk, TransactionState.PaybankReturnFileGenerationOk, TransactionState.PaybankReturnFileGenerationError, TransactionState.FACPFileGenerationError, TransactionState.FACPFileGenerationOk };
                var arrEventType = new[] { EventType.Upload, EventType.Authorize, EventType.Delete };

                if (fileOriginal.Batchs.Any(c => c.Transactions.Any(d => arrStates.Contains(d.StatusId))))
                {
                    var listaTransactionStatusError = fileOriginal.Batchs.SelectMany(p => p.Transactions).Where(d => arrStates.Contains(d.StatusId));

                    foreach (var transactionError in listaTransactionStatusError)
                    {
                        if (transactionError.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue))
                        {
                            var lastEventOnlineLog = transactionError.TransactionEventOnlineLogs.Where(c => c.ErrorMessageId.HasValue).LastOrDefault().EventOnlineLog;
                            var lastErrorCode = transactionError.TransactionEventOnlineLogs.Where(c => c.ErrorMessageId.HasValue).LastOrDefault().ErrorMessage.Code;
                            var lastErrorMessage = culture == 0 ? transactionError.TransactionEventOnlineLogs.Where(c => c.ErrorMessageId.HasValue).LastOrDefault().ErrorMessage.EnglishText : transactionError.TransactionEventOnlineLogs.Where(c => c.ErrorMessageId.HasValue).LastOrDefault().ErrorMessage.SpanishText;

                            returnValue.Add(new PostAccountingFileDetailGridBodyDTO
                            {
                                Id = lastEventOnlineLog != null ? lastEventOnlineLog.Id : 0,
                                EventType = lastEventOnlineLog != null ? lastEventOnlineLog.EventTypeId.GetDescription() : null,
                                AccountNumber = transactionError.AccountNumber,
                                Amount = transactionError.Amount,
                                BeneficiaryName = transactionError.BeneficiaryName,
                                OriginatorAccount = transactionError.OriginatorAccount,
                                Date = lastEventOnlineLog != null ? lastEventOnlineLog.Date : DateTime.Now,
                                StateId = lastEventOnlineLog.StatusId.GetDescription(),
                                ErrorCode = lastErrorCode,
                                ErrorMessage = lastErrorMessage
                            });
                        }
                    }
                }

                return returnValue;
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
        /// Método que obtiene el summary para el proceso Paybank return File
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<ReturnFileDetailGridBodyDTO> ObtenerProcessingFileReturnFileDetail(int fileId)
        {
            var returnValue = new List<ReturnFileDetailGridBodyDTO>();

            try
            {
                var listTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "TransactionTrandeFiles.TrandeFile");

                if (!listTransaction.Any(p => p.StatusId == TransactionState.PaybankReturnFileGenerationError))
                    return returnValue;

                var listTransactionPaybankError = listTransaction.Where(p => p.StatusId == TransactionState.PaybankReturnFileGenerationError);

                foreach (var transactionError in listTransactionPaybankError)
                {
                    var lastTrandeFileLog = transactionError.TransactionTrandeFiles.LastOrDefault().TrandeFile;

                    if (lastTrandeFileLog.EventStateId == EventState.Incomplete)
                    {
                        returnValue.Add(new ReturnFileDetailGridBodyDTO
                        {
                            Id = lastTrandeFileLog.Id,
                            AccountNumber = transactionError.AccountNumber,
                            Amount = transactionError.Amount,
                            BeneficiaryName = transactionError.BeneficiaryName,
                            OriginatorAccount = transactionError.OriginatorAccount,
                            Date = lastTrandeFileLog.Date,
                            FileName = lastTrandeFileLog.FileName,
                            EventStateId = lastTrandeFileLog.EventStateId,
                            UserSoeid = lastTrandeFileLog.UserSoeid,
                            MessageReceived = lastTrandeFileLog.MessageReceived
                        });
                    }
                }

                return returnValue;
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
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<FACPDetailGridBodyDTO> ObtenerProcessingFileFACPDetail(int fileId)
        {
            var returnValue = new List<FACPDetailGridBodyDTO>();

            try
            {
                var listTransaction = this.transactionRepositorio.Obtener(p => p.Batch.FileId == fileId, null, "TransactionTrandeFiles.TrandeFile");

                if (!listTransaction.Any(p => p.StatusId == TransactionState.FACPFileGenerationError))
                    return returnValue;

                var listTransactionFACPError = listTransaction.Where(p => p.StatusId == TransactionState.FACPFileGenerationError);

                foreach (var transactionError in listTransactionFACPError)
                {
                    var lastTrandeFileLog = transactionError.TransactionTrandeFiles.LastOrDefault().TrandeFile;

                    if (lastTrandeFileLog.EventStateId == EventState.Incomplete)
                    {
                        returnValue.Add(new FACPDetailGridBodyDTO
                        {
                            Id = lastTrandeFileLog.Id,
                            AccountNumber = transactionError.AccountNumber,
                            Amount = transactionError.Amount,
                            BeneficiaryName = transactionError.BeneficiaryName,
                            OriginatorAccount = transactionError.OriginatorAccount,
                            Date = lastTrandeFileLog.Date,
                            FileName = lastTrandeFileLog.FileName,
                            EventStateId = lastTrandeFileLog.EventStateId,
                            UserSoeid = lastTrandeFileLog.UserSoeid,
                            MessageReceived = lastTrandeFileLog.MessageReceived
                        });
                    }
                }

                return returnValue;
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

        #endregion

        #region Metodos Privados

        #region Import
        
        /// <summary>
        /// Método que verifica todos los estados que debe cumplir para dejar el estado del archivo como Complete
        /// </summary>
        /// <param name="listaFiles"></param>
        /// <param name="paybankReturnFileConfiguration"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<int> VerificarFinalStatus(IEnumerable<Dominio.Core.File> listaFiles, PaybankReturnFileConfiguration paybankReturnFileConfiguration, int flag, int countryId)
        {
            var removeArrids = new List<int>();

            var schedulerConfiguration = this.schedulerPostACHConfigurationRepositorio.ObtenerPrimero(p => p.CountryId == countryId);

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

                    if (flag == 0)
                        this.fileRepositorio.Actualizar(file);

                    continue;
                }
            }

            return removeArrids;
        }        
        
        /// <summary>
        /// Método que obtiene, dependiendo de la fecha de creación, los archivos de pagos desde paybank
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="listaArchivosPaybank"></param>
        /// <param name="listaOriginal"></param>
        /// <param name="fecha"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        private void ProcesarArchivoPaybankExistencia(Country countryOriginal, IEnumerable<FileInfo> listaArchivosPaybank, IList<Dominio.Core.File> listaOriginal, DateTime fecha, int currencyId, int currentCulture, string userSoeid, Guid newGuid)
        {
            var returnCodeMappingConfigurationOriginal = this.returnCodeMappingConfigurationRepositorio.ObtenerTodos();

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

                        var tipoTransaction = VerificarTipoProcesoArchivo(countryOriginal, file, currencyId, currentCulture, newGuid, userSoeid, fecha, EventType.SearchFiles);

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
                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["UnableToReadFileFormat"] = true;

                    SaveCriticalFiles(fileInfo.Name);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(forEx, "PoliticaPostAccountingFormatError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1002", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.SearchFiles));                    
                }
                catch (NachaReturnCodeMappingException nshEx)
                {
                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["UnableToVerifyNachaReturnCodeMapping"] = true;

                    SaveCriticalFiles(fileInfo.Name);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(nshEx, "PoliticaNachaReturnCodeMapping");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1025", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyNachaReturnCodeMapping, EventType.SearchFiles));
                }
                catch (ArgumentOutOfRangeException argEx)
                {
                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["UnableToReadFileFormat"] = true;

                    SaveCriticalFiles(fileInfo.Name);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(argEx, "PoliticaPostAccountingFormatError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1002", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.SearchFiles));
                }               
                catch (Exception ex)
                {
                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["UnableToReadFileFormat"] = true;

                    SaveCriticalFiles(fileInfo.Name);

                    var newEx = ProveedorExcepciones.ManejaExcepcionOut(ex, "PoliticaBaseExistenciaImportError");
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1013", fileInfo.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.SearchFiles));
                }
            }
        }
      
        /// <summary>
        /// Método que obtiene el path de respaldo donde se dejaran el archivo ya importado dependiendo del tipo de proceso asociado
        /// </summary>
        /// <param name="country"></param>
        /// <param name="currency"></param>
        /// <param name="iterator"></param>
        /// <param name="originalPaybankConfiguration"></param>
        /// <returns></returns>
        private string GetPaybankProcessedFiles(Country country, Currency currency, int iterator, IEnumerable<PaybankConfiguration> originalPaybankConfiguration)
        {
            string pathProcessedFiles = null;

            pathProcessedFiles = iterator == 0 ? originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currency.Id).FirstOrDefault().FolderPathBackInclearing : originalPaybankConfiguration.SelectMany(p => p.PaybankConfigurationCurrencies).Where(p => p.CurrencyId == currency.Id).FirstOrDefault().FolderPathBackReturn;      

            return pathProcessedFiles;
        }

        /// <summary>
        /// Método que procesa el archivo desde paybank para su inserción en la base de datos
        /// </summary>
        /// <param name="listaArchivosPaybank"></param>
        /// <param name="listaOriginal"></param>
        private int ProcesarArchivoNuevoPaybank(FileInfo fileSelected, int countryId, int currencyId, string userSoeid, int currentCulture, string pathProcessedFiles, Guid newGuid, DateTime dateSelected, ExecutionMode executionMode)
        {
            Country countryOriginal = null;
            var fileId = 0;

            try
            {
                countryOriginal = this.countryRepositorio.Encontrar(countryId);
                var currencyOriginal = this.currencyRepositorio.Encontrar(currencyId);
                var returnCodeMappingConfigurationOriginal = this.returnCodeMappingConfigurationRepositorio.ObtenerTodos();

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
                    numberConsArchivo = this.paybankRepositorio.ValidateOuputFile(file.Name, file.CreationDate.Value, file.TotalDebit, file.TotalCredit, connectionName);
                }

                if (numberConsArchivo == null)
                    throw new ValidationPaybankException(currentCulture == 0 ? "The file can not be imported because it does not meet validation Paybank." : "El archivo no se puedo importar porque no cumple con la validación de Paybank.");

                file.NumConsArchivo = numberConsArchivo;              

                var tipoTransaction = VerificarTipoProcesoArchivo(countryOriginal, file, currencyId, currentCulture, newGuid, userSoeid, file.DateSelected, EventType.Import);

                if (tipoTransaction == null)
                    return fileId;

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

                file.StateId = FileState.Pending;

                var tipo = tipoTransaction.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryId).SelectMany(p => p.TransactionTypeConfigurationCountryCurrencies).Where(c => c.CurrencyId == currencyId).FirstOrDefault();

                file.TransactionTypeConfigurationCountryCurrencyId = tipo.Id;

                //TEST
                fileId = this.postACHRepositorio.BulkInsertPostACH(file, true);

                fileSelected.CopyTo(Path.Combine(pathProcessedFiles, fileSelected.Name), true);
                fileSelected.Delete();

                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.SucessfullyImported.GetDescription(), null, fileSelected.Name, fileId, newGuid, ProcessFlag.SucessfullyImported, EventType.Import));
            }
            catch (FormatException forEx)
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session["UnableToReadFileFormat"] = true;

                SaveCriticalFiles(fileSelected.Name);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(forEx, "PoliticaPostAccountingFormatError");
                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1002", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToReadFileFormat, EventType.Import));
            }
            catch (ValidationPaybankException payEx)
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session["UnsucessfullyPaybankValidationFile"] = true;

                SaveCriticalFiles(fileSelected.Name);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(payEx, "PoliticaValidationPaybankException");
                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1022", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToImport, EventType.Import));
            }
            catch (NachaReturnCodeMappingException nshEx)
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session["UnableToVerifyNachaReturnCodeMapping"] = true;

                SaveCriticalFiles(fileSelected.Name);

                var newEx = ProveedorExcepciones.ManejaExcepcionOut(nshEx, "PoliticaNachaReturnCodeMapping");
                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, nshEx.Message, "1025", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyNachaReturnCodeMapping, EventType.Import));
            }
            catch (Exception ex)
            {
                var newEx = ProveedorExcepciones.ManejaExcepcionOut(ex, "PoliticaBaseImportError");
                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, newEx.Message, "1003", fileSelected.Name, (int?)null, newGuid, ProcessFlag.UnableToImport, EventType.Import));

                throw newEx;
            }

            return fileId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveCriticalFiles(string fileName)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["FileNameProblem"] != null)
                {
                    var listaFileNameProblem = (List<string>)HttpContext.Current.Session["FileNameProblem"];

                    listaFileNameProblem.Add(fileName);
                }
                else
                {
                    var newListFileName = new List<string>() { fileName };

                    HttpContext.Current.Session["FileNameProblem"] = newListFileName;
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
        /// Método que verifica al tipo de proceso final al cual va a estar asociado el archivo
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="file"></param>
        /// <param name="currencyId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="fecha"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private TransactionTypeConfiguration VerificarTipoProcesoArchivo(Country countryOriginal, Dominio.Core.File file, int currencyId, int currentCulture, Guid newGuid, string userSoeid, DateTime fecha, EventType eventType)
        {
            var mensaje = string.Empty;

            var paybankConfiguration = countryOriginal.PaybankConfigurationCountries.FirstOrDefault().PaybankConfiguration;

            var codDestinoInmediato = file.Batchs.SelectMany(p => p.Transactions).FirstOrDefault().ImmediateDestinationCodeTrade;

            var transactionCodes = file.Batchs.SelectMany(p => p.Transactions).Select(c => int.Parse(c.TransactionCode)).ToArray().Distinct();

            if (codDestinoInmediato == paybankConfiguration.RFICity || codDestinoInmediato == paybankConfiguration.Ofi1)//Destino de archivos propios OnUs
            {
                var answer = false;

                var transactionTypeConfiguration = this.transactionTypeConfigurationRepositorio.ObtenerPrimero(p => p.TransactionType.Identifier == "inclearingcheck" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));                                

                if (transactionTypeConfiguration != null)
                {
                    if (file.Batchs.Any(c => c.SECCode == "TRC"))
                    {
                        answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Any(p => p.CountryId == countryOriginal.Id && p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                        if (answer)
                        {
                            if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid))
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
                            var exceptArrayInclearingCheck = GetExceptArrayTransactionCodes(countryOriginal, currencyId, transactionCodes);

                            return UnableVerifyProcessType(countryOriginal, file, currencyId, currentCulture, newGuid, eventType, ref mensaje, exceptArrayInclearingCheck);
                        }
                    }
                }

                transactionTypeConfiguration = this.transactionTypeConfigurationRepositorio.ObtenerPrimero(p => p.TransactionType.Identifier == "return" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));

                if (transactionTypeConfiguration != null)
                {
                    answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Any(p => p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                    if (answer)
                    {
                        if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid))
                            return null;

                        var arrSecCodes = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Select(p => p.SEC).FirstOrDefault().Split('-');

                        answer = file.Batchs.Any(p => arrSecCodes.Contains(p.SECCode));

                        if (answer)
                            return transactionTypeConfiguration;
                    }
                }

                transactionTypeConfiguration = this.transactionTypeConfigurationRepositorio.ObtenerPrimero(p => p.TransactionType.Identifier == "incomingelectronic" && p.TransactionTypeConfigurationCountrys.Any(c => c.CountryId == countryOriginal.Id && c.TransactionTypeConfigurationCountryCurrencies.Any(d => d.CurrencyId == currencyId)));                

                if (transactionTypeConfiguration != null)
                {
                    if (!file.Batchs.Any(p => p.Originator.Contains("CASH INPUT")))
                    {
                        answer = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Any(p => p.TransactionTypeConfigurationCountryCurrencies.Any(c => c.CurrencyId == currencyId && transactionCodes.All(e => c.TransactionCodes.Where(z => z.FieldStatusId == FieldStatus.Active).ToList().Exists(f => int.Parse(f.PaybankCode) == e))));

                        if (answer)
                        {
                            if (!ValidateSecCodesApplication(transactionTypeConfiguration, file, currentCulture, countryOriginal, currencyId, eventType, newGuid))
                                return null;

                            var arrSecCodes = transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Where(p => p.CountryId == countryOriginal.Id).Select(p => p.SEC).FirstOrDefault().Split('-');

                            answer = file.Batchs.Any(p => arrSecCodes.Contains(p.SECCode));

                            if (answer)
                                return transactionTypeConfiguration;
                        }
                    }
                }

                var exceptArrayReturnInclearing = GetExceptArrayTransactionCodes(countryOriginal, currencyId, transactionCodes);

                return UnableVerifyProcessType(countryOriginal, file, currencyId, currentCulture, newGuid, eventType, ref mensaje, exceptArrayReturnInclearing);
            }

            mensaje = currentCulture == 0 ? "The imported file does not correspond to a destination ONUs own files." : "El archivo importado no corresponde a un destino de archivos propios OnUs.";

            SaveCriticalFiles(file.Name);

            ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaOnusOwnFileValidationException");

            this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1024", file.Name, (int?)null, newGuid, ProcessFlag.NotONUsOwnFileDestinationFile, eventType));

            if (HttpContext.Current != null)
                HttpContext.Current.Session["NotONUsOwnFileDestinationFile"] = true;

            return null;
        }

        /// <summary>
        /// Método que obtiene el array de datos de los transaction codes por pais y moneda
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="currencyId"></param>
        /// <param name="transactionCodes"></param>
        /// <returns></returns>
        private IEnumerable<int> GetExceptArrayTransactionCodes(Country countryOriginal, int currencyId, IEnumerable<int> transactionCodes)
        {
            var systemTransactionCodes = this.transactionCodeRepositorio.Obtener(p => p.TransactionTypeConfigurationCountryCurrency.CurrencyId == currencyId && p.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.CountryId == countryOriginal.Id);

            var paybankCodeArrayInt = systemTransactionCodes.Select(p => int.Parse(p.PaybankCode));

            var exceptArray = transactionCodes.Except(paybankCodeArrayInt);

            return exceptArray;
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
        private TransactionTypeConfiguration UnableVerifyProcessType(Country countryOriginal, Dominio.Core.File file, int currencyId, int currentCulture, Guid newGuid, EventType eventType, ref string mensaje, IEnumerable<int> exceptArray)
        {
            mensaje = currentCulture == 0 ? string.Format("There is no existing transaction code to verify the transaction type associated with the file. Transaction Codes: {0}", exceptArray.Select(p=>p.ToString()).Aggregate((a,b) => a + "," + b)) : string.Format("No existe un código de transaccion existente para verificar el tipo de transacción asociado al archivo. Códigos Transacciones: {0}", exceptArray.Select(p=>p.ToString()).Aggregate((a,b) => a + "," + b));

            SaveCriticalFiles(file.Name);

            ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaProcessTypeVerificationException");

            this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1023", file.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyProcessType, eventType));

            if (HttpContext.Current != null)
                HttpContext.Current.Session["UnableToVerifyProcessType"] = true;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private bool ValidateSecCodesApplication(TransactionTypeConfiguration transactionTypeConfiguration, Dominio.Core.File file, int currentCulture, Country countryOriginal, int currencyId, EventType eventType, Guid newGuid)
        {
            var answer = true;

            if (transactionTypeConfiguration.TransactionTypeConfigurationCountrys.Any(p => p.CountryId == countryOriginal.Id && p.SEC == null))
            {
                var mensaje = currentCulture == 0 ? string.Format("{0} : {1}", "No existe un codigo SEC configurado en la aplicación.", transactionTypeConfiguration.TransactionType.SpanishGloss) : string.Format("{0} : {1}", "There is no SEC configured in the application code.", transactionTypeConfiguration.TransactionType.Gloss);

                SaveCriticalFiles(file.Name);

                ProveedorExcepciones.ManejaExcepcion(new Exception(mensaje), "PoliticaProcessTypeVerificationException");

                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, "1023", file.Name, (int?)null, newGuid, ProcessFlag.UnableToVerifyProcessType, eventType));

                if (HttpContext.Current != null)
                    HttpContext.Current.Session["UnableToVerifyProcessType"] = true;

                answer = false;
            }

            return answer;
        }
        
        #endregion

        #region Citiscreening

        /// <summary>
        /// Método que envia las transacciones segun su estado a GI
        /// </summary>
        /// <param name="listaTransaction"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="newGuid"></param>
        private void EnvioTransaccionesCitiscreening(IEnumerable<Transaction> listaTransaction, CitiscreeningConfiguration configuration, string userSoeid, Guid newGuid)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "citiscreening" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();
            
            var transactionFilter = listaTransaction.Where(c => c.StatusId == TransactionState.CitiscreeningError || c.StatusId == TransactionState.Import).Select((value, index) => new { Value = value, Index = index });

            try
            {
                transactionFilter.AsParallel().ForAll(item =>
                {
                    ValidarTransaccionCitiscreening(item.Index, newGuid, item.Value, configuration, errorMessages, userSoeid);
                });
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }
            
            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesCitiscreening(listaExceptions);
            }                     
        }

        /// <summary>
        /// Método que valida en el caso de que existan excepciones durante el proceso Citiscreening
        /// </summary>
        /// <param name="listaExcepcion"></param>
        private void ValidarExcepcionesCitiscreening(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                 
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();


                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaCitiscreeningError");
                });

                throw groupList.FirstOrDefault();
            }
        }

        /// <summary>
        /// Flattens the aggregate exceptions coming from the task hierarchy into a list.
        /// </summary>
        /// <returns>The flattened list of exceptions.</returns>
        private List<Exception> FlattenAggregateExceptions(AggregateException ae)
        {
            return ae.Flatten().InnerExceptions.ToList();
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = sendMessage, EventTypeId = EventType.CitiscreeningValidation }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
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
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (errorMessage == null)
            {
                var codigoGenerico = ConfigurationManager.AppSettings["GenericErrorMessage"];
                errorMessage = errorMessages.FirstOrDefault(p => p.Code == codigoGenerico);
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
        }   

        /// <summary>
        /// Método que inicializa la conexion a Citiscreening
        /// </summary>
        /// <param name="queueConnectivity"></param>
        private void InitializeCitiscreeningConnection(CitiscreeningConfiguration queueConnectivity)
        {
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

        #region Inclearing True Transaction - Return

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
        private void IniciarProcesoOpenBatch(Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, TransactionTypeBatchConfiguration batchConfiguration, int currentCulture, Guid newGuid, Dominio.Core.File fileOriginal, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xmlString = DominioOpenBatchLogica.GetOpenBatchXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, batchConfiguration);

            EnvioTransaccionOpenBatch(xmlString, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal, currencyId, countryOriginal, listProcessingLog);                   
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
        private void EnvioTransaccionOpenBatch(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, Dominio.Core.File fileOriginal, int currencyId, Country countryOriginal, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchAlreadyOpened.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchAlreadyOpened, EventType.OpenBatch));
                            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);

                            batchConfiguration.IsOpen = true;
                            batchConfiguration.UserOpenedBatch = userSoeid;
                            batchConfiguration.OpenDate = DateTime.Now;
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                            InsertErrorMessageOpenBatchCodeReturn0(xmlMessage, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, codigoError, descripcion);
                        }
                    }
                    else
                    {
                        batchConfiguration.IsOpen = true;
                        batchConfiguration.UserOpenedBatch = userSoeid;
                        batchConfiguration.OpenDate = DateTime.Now;

                        listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchSucessfully.GetDescription(), "0", fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchSucessfully, EventType.OpenBatch));
                        IngresarEventoTransaccionOpenBatchOK(batchConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);                        
                    }
                }
                else
                {
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codeReturn.ToString(), fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    InsertErrorMessageOpenBatchCodeReturnNo0(xmlMessage, userSoeid, newGuid, errorMessages, batchConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, codeReturn);
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
        private void InsertErrorMessageOpenBatchCodeReturnNo0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, int fileId, string replyMessage, int codeReturn)
        {
            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, codeReturn, null, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId);

            var internErrorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        private void InsertErrorMessageOpenBatchCodeReturn0(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, int fileId, string replyMessage, string codigoError, string descripcion)
        {
            IngresarEventoTransaccionOpenBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileId);

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
        private void IngresarEventoTransaccionOpenBatchError(TransactionTypeBatchConfiguration batchConfiguration, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey, int fileId)
        {
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        }

        /// <summary>
        /// Método que valida en el caso de que existan excepciones en el proceso Open Batch
        /// </summary>
        /// <param name="listaExcepcion"></param>        
        private void ValidarExcepcionesOpenBatch(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                  
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingOpenBatchError");
                });

                throw groupList.FirstOrDefault();
            }
        }        

        /// <summary>
        /// Método que inicializa la conexion Open Batch a Cosmos
        /// </summary>
        /// <param name="queueConnectivity"></param>
        private void InitializePostAccountingConnection(PostAccountingConfiguration queueConnectivity)
        {
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
        private void IniciarProcesoCloseBatch(Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration, int currentCulture, Guid newGuid, int currencyId, IEnumerable<ErrorMessage> errorMessages)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            try
            {
                batchConfiguration.AsParallel().ForAll(batch =>
                {
                    try
                    {
                        var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

                        var xmlString = DominioCloseBatchLogica.GetCloseBatchXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, batch);

                        EnvioTransaccionCloseBatch(xmlString, userSoeid, newGuid, errorMessages, batch, uniqueKey, currentCulture, countryOriginal, currencyId);

                        this.transactionTypeBatchConfigurationRepositorio.Actualizar(batch);
                    }
                    catch (Exception ex)
                    {
                        this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ex.Message, "1021", null, null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    }
                });
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesCloseBatch(listaExceptions);
            }
        }

        /// <summary>
        /// Método que valida en el caso de que hayan ocurrido excepciones durante el proceso de Close Batch
        /// </summary>
        /// <param name="exception"></param>
        private void ValidarExcepcionesCloseBatch(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                 
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingCloseBatchError");
                });

                throw groupList.FirstOrDefault();
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
        private void EnvioTransaccionCloseBatch(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, TransactionTypeBatchConfiguration batchConfiguration, string uniqueKey, int currentCulture, Country countryOriginal, int currencyId)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.CloseBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.CloseBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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

                        if (codigoError == "59")
                        {
                            IngresarEventoTransaccionCloseBatchError(batchConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                            batchConfiguration.IsOpen = false;
                            batchConfiguration.CloseDate = DateTime.Now;

                            this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, descripcion, codigoError, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.CloseBatchAlreadyClosed, EventType.CloseBatch));

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

                                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, errorMessage.Code, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                            }
                            else
                            {
                                var mensaje = currentCulture == 0 ? "There was an error when try to close the batch. Contact with your administrator." : "Hubo un error cuando se trato de cerrar el batch. Contactese con su administrador.";

                                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, codigoError, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                            }
                        }
                    }
                    else
                    {
                        batchConfiguration.IsOpen = false;
                        batchConfiguration.CloseDate = DateTime.Now;

                        IngresarEventoTransaccionCloseBatchOK(batchConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                        this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.CloseBatchSucessfully.GetDescription(), "0", batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.CloseBatchSucessfully, EventType.CloseBatch));

                        if (HttpContext.Current != null)
                            HttpContext.Current.Session["CloseBatchSucessfully"] = true;

                        SaveCriticalBatch(batchConfiguration.Id);
                    }
                }
                else
                {
                    IngresarEventoTransaccionCloseBatchError(batchConfiguration, errorMessages, codeReturn, null, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey);

                    errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (HttpContext.Current != null)
                        HttpContext.Current.Session["UnableToCloseBatch"] = true;

                    SaveCriticalBatch(batchConfiguration.Id);

                    if (errorMessage != null)
                    {
                        var mensaje = currentCulture == 0 ? string.Format("There was an error when try to close the batch: {0}", errorMessage.EnglishText) : string.Format("Hubo un error cuando se trato de cerrar el batch: {0}", errorMessage.SpanishText);

                        this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, errorMessage.Code, batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    }
                    else
                    {
                        var mensaje = currentCulture == 0 ? "There was an error when try to close the batch. Contact with your administrator." : "Hubo un error cuando se trato de cerrar el batch. Contactese con su administrador.";

                        this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, mensaje, codeReturn.ToString(), batchConfiguration.Id.ToString(), (int?)null, newGuid, ProcessFlag.UnableToCloseBatch, EventType.CloseBatch));
                    }
                }
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
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        private void EnvioTransaccionesUpload(IEnumerable<Transaction> listaTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();                        

            var postAccountingMode = listaTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var transactionFilterStateNoUploadToCosmos = listaTransaction.Where(c => c.StatusId == TransactionState.CitiscreeningOk || c.StatusId == TransactionState.Import);            
            var transactionFilterStateUploadError = listaTransaction.Where(c => c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

            var statusId = transactionFilterStateNoUploadToCosmos.Any() ? TransactionState.Import : (transactionFilterStateUploadError.Any() ? TransactionState.UploadToCosmosRejected : TransactionState.Import);

            var listaSplitedStateNoUploadToCosmos = transactionFilterStateNoUploadToCosmos.Any() ? transactionFilterStateNoUploadToCosmos.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedStateErrorUploadToCosmos = transactionFilterStateUploadError.Any() ? transactionFilterStateUploadError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : new List<List<Transaction>>();

            switch (statusId) 
            { 
                case TransactionState.Import:
                    try
                    {
                        listaSplitedStateNoUploadToCosmos.AsParallel().ForAll(item =>
                        {
                            if (item.Any())
                                ValidarTransaccionUploadStateNoUploadToCosmos(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                        });                      
                    }
                    catch (AggregateException ex)
                    {
                        FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                    }
                    break;          
                case TransactionState.UploadToCosmosRejected:                       
                    try
                    {
                        listaSplitedStateErrorUploadToCosmos.AsParallel().ForAll(item =>
                        {
                            if (item.Any())
                                ValidarTransaccionUploadStateUploadToCosmos(item, listaTransaction, newGuid, countryOriginal, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                        });                            
                    }
                    catch (AggregateException ex)
                    {
                        FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                    }
                    break;
            }           

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesUpload(listaExceptions);
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
        private void ValidarTransaccionUploadStateNoUploadToCosmos(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

            var xDeclaration = DominioUploadLogica.GetXmlDeclaration(uniqueKey);

            var identificadorTransactionType = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.TransactionTypeConfiguration.TransactionType.Identifier;
            var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;
            var transactionTypeBatchConfiguration = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeBatchConfigurations.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && p.Id == batchId);

            foreach(var transaction in splitList)
            {
                transaction.StatusId = TransactionState.UploadToCosmos;

                DominioUploadLogica.GetUploadMessageByTransactionType(countryOriginal, configuration, xDeclaration, transaction, identificadorTransactionType, postAccountingMode, cosmosFunctionalUser, transactionTypeBatchConfiguration);
            }          

            this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionUploadResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);

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
        private void ValidarTransaccionUploadStateUploadToCosmos(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Upload && (p.EventOnlineLog.StatusId == TransactionState.UploadToCosmos || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosRejected || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosHoldingAccountError))).OrderByDescending(p => p.EventOnlineLog.Date).Select(p => p.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            eventOnlineGroupByUniqueKey.AsParallel().ForAll(eventOnline =>
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
                    ValidarTransactionUploadResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);
            });                
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
        private void ValidarTransactionUploadResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {        
            if (codeReturn == 0)
            {
                switch (postAccountingMode)
                {
                    case PostAccountMode.ClientAccount:
                        ValidarTransactionUploadResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        break;
                    case PostAccountMode.HoldingAccount:
                        ValidarTransactionUploadResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        break;
                    case PostAccountMode.Both:
                        ValidarTransactionUploadResponseClientBoth(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        break;
                }
            }
            else
            {
                listTransaction.ForEach(p => IngresarEventoTransaccionUploadError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ValidarTransactionUploadResponseClientBoth(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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
                            ProcessUploadErrorOriginalTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                        else if (!errorTrxOriginal && errorTrxHoldingAccount)
                        {
                            ProcessUploadErrorHoldingAccountTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                        else
                        {
                            ProcessUploadErrorRejectedByCosmos(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Upload));

                listTransaction.ForEach(p => IngresarEventoTransaccionUploadError(p, errorMessages, -1, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ProcessUploadErrorRejectedByCosmos(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.UploadToCosmosRejected;

            int codeRejected = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejected = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, codeRejected, descriptionRejected, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);

            /*int codeRejectedHoldingAccount = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejectedHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, codeRejectedHoldingAccount, descriptionRejectedHoldingAccount, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);*/
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
        private void ProcessUploadErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.UploadToCosmosHoldingAccountError;

            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
        private void ProcessUploadErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.UploadToCosmosClientAccountError;

            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
        private void ValidarTransactionUploadResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

                        IngresarEventoTransaccionUploadError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Upload));

                listTransaction.ForEach(p => IngresarEventoTransaccionUploadError(p, errorMessages, -1, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void IngresarEventoTransaccionUploadError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        }     
        
        /// <summary>
        /// Método que valida en el caso de que hayan existido excepciones durante el proceso Upload
        /// </summary>
        /// <param name="listaExcepcion"></param>
        private void ValidarExcepcionesUpload(List<Exception> listaExcepcion)
        {            
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                 
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p => {                    
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingUploadError");
                });

                throw groupList.FirstOrDefault();
            }
        }
        
        #endregion

        #region Authorize

        /// <summary>
        /// Método que valida en el caso de que hayan existido excepciones durante el proceso Authorize
        /// </summary>
        /// <param name="listaExcepcion"></param>
        private void ValidarExcepcionesAuthorize(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                 
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();

                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccountingAuthorizeError");
                });

                throw groupList.FirstOrDefault();
            }
        }

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
        private void EnvioTransaccionesAuthorize(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var postAccountingMode = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var transactionFilterStateNoAuthorize = listTransaction.Where(c => c.StatusId == TransactionState.UploadToCosmosButNotAuthorized);            
            var transactionFilterStateAuthorizeError = listTransaction.Where(c => c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

            var statusId = transactionFilterStateNoAuthorize.Any() ? TransactionState.UploadToCosmosButNotAuthorized : (transactionFilterStateAuthorizeError.Any() ? TransactionState.AuthorizeByCosmosRejected : TransactionState.UploadToCosmosButNotAuthorized);

            var listaSplitedStateAuthorizeToCosmosNotAuthorized = transactionFilterStateNoAuthorize.Any() ? transactionFilterStateNoAuthorize.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedStateErrorAuthorizeToCosmos = transactionFilterStateAuthorizeError.Any() ? transactionFilterStateAuthorizeError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : new List<List<Transaction>>();

            switch (statusId)
            {
                case TransactionState.UploadToCosmosButNotAuthorized:
                    try
                    {
                        listaSplitedStateAuthorizeToCosmosNotAuthorized.AsParallel().ForAll(item =>
                        {
                            if (item.Any())
                                ValidarTransaccionAuthorizeStateNotAuthorized(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                        });                        
                    }
                    catch (AggregateException ex)
                    {
                        FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                    }                   
                    break;                
                case TransactionState.AuthorizeByCosmosRejected:
                    try
                    {
                        listaSplitedStateErrorAuthorizeToCosmos.AsParallel().ForAll(item =>
                        {
                            if (item.Any())
                                ValidarTransaccionAuthorizeStateAuthorize(item, listTransaction, newGuid, countryOriginal, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                        });                        
                    }
                    catch (AggregateException ex)
                    {
                        FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                    }                   
                    break;
            }            

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesAuthorize(listaExceptions);                
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
        private void ValidarTransaccionAuthorizeStateAuthorize(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Authorize && (p.EventOnlineLog.StatusId == TransactionState.Authorize || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError))).OrderByDescending(p => p.EventOnlineLog.Date).Select(c => c.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            eventOnlineGroupByUniqueKey.AsParallel().ForAll(eventOnline =>
            {                               
                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionAuthorizeResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);               
            });            
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
        private void ValidarTransaccionAuthorizeStateNotAuthorized(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionAuthorizeResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);

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
        private void ValidarTransactionAuthorizeResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            if (codeReturn == 0)
            {
                switch (postAccountingMode)
                {
                    case PostAccountMode.ClientAccount:
                        ValidarTransactionAuthorizeResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        break;
                    case PostAccountMode.HoldingAccount:
                        ValidarTransactionAuthorizeResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        //SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                        break;
                    case PostAccountMode.Both:
                        ValidarTransactionAuthorizeResponseClientBoth(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryId, currencyId, listProcessingLog);
                        break;
                }
            }
            else
            {
                listTransaction.ForEach(p => IngresarEventoTransaccionAuthorizeError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ValidarTransactionAuthorizeResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingFileLog)
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

                        IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
                listProcessingFileLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Authorize));

                listTransaction.ForEach(p => IngresarEventoTransaccionAuthorizeError(p, errorMessages, -1, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ValidarTransactionAuthorizeResponseClientBoth(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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
                            ProcessAuthorizeErrorOriginalTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                        else if (!errorTrxOriginal && errorTrxHoldingAccount)
                        {
                            ProcessAuthorizeErrorHoldingAccountTransaction(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                        else
                        {
                            ProcessAuthorizeErrorRejectedByCosmos(replyMessage, sendMessage, errorMessages, newGuid, userSoeid, uniqueKey, item, transaction);
                        }
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Authorize));

                listTransaction.ForEach(p => IngresarEventoTransaccionAuthorizeError(p, errorMessages, -1, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ProcessAuthorizeErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosClientAccountError;
            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
        private void IngresarEventoTransaccionAuthorizeError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        private void ProcessAuthorizeErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosHoldingAccountError;
            transaction.InstructiveMessageCode = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
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
        private void ProcessAuthorizeErrorRejectedByCosmos(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.StatusId = TransactionState.AuthorizeByCosmosRejected;

            int codeRejected = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejected = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, codeRejected, descriptionRejected, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);

            /*int codeRejectedHoldingAccount = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var descriptionRejectedHoldingAccount = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionAuthorizeError(transaction, errorMessages, codeRejectedHoldingAccount, descriptionRejectedHoldingAccount, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);*/
        }

        #endregion

        #region Delete

        /// <summary>
        /// Método que valida en el caso de que hayan existido excepciones durante el proceso Delete
        /// </summary>
        /// <param name="listaExcepcion"></param>
        private void ValidarExcepcionesDelete(List<Exception> listaExcepcion)
        {
            if (listaExcepcion.Any())
            {
                var groupList = (from c in listaExcepcion                                      
                                 select c).GroupBy(g => g.GetType().Name).Select(x => x.FirstOrDefault()).ToList();


                groupList.ForEach(p =>
                {
                    ProveedorExcepciones.ManejaExcepcion(p, "PoliticaPostAccoutingDeleteError");
                });

                throw groupList.FirstOrDefault();
            }
        }

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
        private void EnvioTransaccionesDelete(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();
            
            var postAccountingMode = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);
            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();
            var processConfiguration = listTransaction.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry;

            if (processConfiguration.PostAccountModeId.Value == PostAccountMode.ClientAccount)
            {
                DeleteProcessClientAccount(listTransaction, countryOriginal, configuration, userSoeid, batchId, newGuid, currencyId, listProcessingLog, exceptions, maxTransactionNumber, errorMessages);
            }

            if (processConfiguration.PostAccountModeId.Value == PostAccountMode.Both)
            {
                DeleteProcessBoth(listTransaction, countryOriginal, configuration, userSoeid, batchId, newGuid, currencyId, listProcessingLog, exceptions, maxTransactionNumber, errorMessages);                
            }

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesDelete(listaExceptions);                
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
        private void DeleteProcessBoth(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog, ConcurrentQueue<Exception> exceptions, int maxTransactionNumber, List<ErrorMessage> errorMessages)
        {
            var transactionAuthorizeRejected = listTransaction.Where(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected);
            var transactionClientAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.UploadToCosmosClientAccountError));
            var transactionHoldingAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.UploadToCosmosHoldingAccountError));
            var transactionClientHoldingAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError));           

            var listaSplitedAuthorizeRejected = transactionAuthorizeRejected.Any() ? transactionAuthorizeRejected.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientAccountError = transactionClientAccountError.Any() ? transactionClientAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedHoldingAccountError = transactionHoldingAccountError.Any() ? transactionHoldingAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientHoldingAccountError = transactionClientHoldingAccountError.Any() ? transactionClientHoldingAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();            

            try
            {
                listaSplitedAuthorizeRejected.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);

                        if (item.Any())
                            ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });

                listaSplitedClientAccountError.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });

                listaSplitedHoldingAccountError.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });

                listaSplitedClientHoldingAccountError.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);

                        if (item.Any())
                            ValidarTransaccionDeleteStateHoldingAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });               
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
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
        private void DeleteProcessClientAccount(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, int batchId, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog, ConcurrentQueue<Exception> exceptions, int maxTransactionNumber, List<ErrorMessage> errorMessages)
        {
            var transactionAuthorizeRejected = listTransaction.Where(c => c.StatusId == TransactionState.AuthorizeByCosmosRejected);
            var transactionClientAccountError = listTransaction.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError));            

            var listaSplitedAuthorizeRejected = transactionAuthorizeRejected.Any() ? transactionAuthorizeRejected.Split(maxTransactionNumber) : new List<List<Transaction>>();
            var listaSplitedClientAccountError = transactionClientAccountError.Any() ? transactionClientAccountError.Split(maxTransactionNumber) : new List<List<Transaction>>();            

            try 
            {
                listaSplitedAuthorizeRejected.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });

                listaSplitedClientAccountError.AsParallel().ForAll(item =>
                {
                    if (item.SelectMany(c => c.TransactionEventOnlineLogs).All(p => p.EventOnlineLog.EventTypeId != EventType.Delete))
                    {
                        if (item.Any())
                            ValidarTransaccionDeleteStateClientAccountError(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, batchId, currencyId, listProcessingLog);
                    }
                });             
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
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
        private void ValidarTransaccionDeleteStateClientAccountError(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosClientAccountError, countryOriginal.Id, currencyId, listProcessingLog);

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
        private void ValidarTransaccionDeleteStateHoldingAccountError(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, int batchId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosHoldingAccountError, countryOriginal.Id, currencyId, listProcessingLog);

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
        private void ValidarTransactionDeleteResponse(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, TransactionState deletedTransactionState, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            if (codeReturn == 0)
            {
                switch (postAccountingMode)
                {
                    case PostAccountMode.ClientAccount:
                        ValidarTransactionDeleteResponseClientAccount(replyMessage, sendMessage, codeReturn, listTransaction, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, deletedTransactionState, countryId, currencyId, listProcessingLog);
                        break;
                    case PostAccountMode.HoldingAccount:
                        //var xelementHoldingAccount = GetUploadXmlRequestIncomingHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                        //SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                        break;
                }
            }
            else
            {
                listTransaction.ForEach(p => IngresarEventoTransaccionDeleteError(p, errorMessages, codeReturn, null, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ValidarTransactionDeleteResponseClientAccount(string replyMessage, string sendMessage, int codeReturn, List<Transaction> listTransaction, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, PostAccountMode postAccountingMode, TransactionState deletionState, int countryId, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

                        //transaction.StatusId = deletionState;

                        IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                    }
                    else
                    {
                        //transaction.StatusId = TransactionState.DeletedInCosmosOk;

                        IngresarEventoTransaccionDeleteOK(transaction, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
                    }

                    transactionCounter++;
                }
            }
            else
            {
                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), "9000", listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToValidateSubmissionResponses, EventType.Delete));

                listTransaction.ForEach(p => IngresarEventoTransaccionDeleteError(p, errorMessages, -1, ProcessFlag.UnableToValidateSubmissionResponses.GetDescription(), newGuid, sendMessage, replyMessage, userSoeid, uniqueKey));
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
        private void ProcessDeleteErrorOriginalTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            int code = int.Parse(item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").FirstOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
        }

        /// <summary>
        /// Método que valida los errores asociados al holding account del proceso Delete
        /// </summary>
        /// <param name="replyMessage"></param>
        /// <param name="sendMessage"></param>
        /// <param name="errorMessages"></param>
        /// <param name="newGuid"></param>
        /// <param name="userSoeid"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="item"></param>
        /// <param name="transaction"></param>
        private void ProcessDeleteErrorHoldingAccountTransaction(string replyMessage, string sendMessage, IEnumerable<ErrorMessage> errorMessages, Guid newGuid, string userSoeid, string uniqueKey, XElement item, Transaction transaction)
        {
            transaction.InstructiveCounterpartCode = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ACCDETAILS").FirstOrDefault().Element("FCCREF").Value;

            int code = int.Parse(item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("ECODE").Value);
            var description = item.Descendants("REPLY_ACCTXN").LastOrDefault().Descendants("ERROR").FirstOrDefault().Element("EDESC").Value;

            IngresarEventoTransaccionDeleteError(transaction, errorMessages, code, description, newGuid, sendMessage, replyMessage, userSoeid, uniqueKey);
        }

        /// <summary>
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
        private void IngresarEventoTransaccionDeleteError(Transaction transaction, IEnumerable<ErrorMessage> errorMessages, int codeReturn, string description, Guid newGuid, string sendMessage, string receiveMessage, string userSoeid, string uniqueKey)
        {
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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

        #endregion

        #region Inclearing Check

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
        private void IniciarProcesoOpenBatchInclearingCheck(Dominio.Core.File fileOriginal, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, int currentCulture, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var cosmosFunctionalUser = countryOriginal.CountryCosmosFunctionalUsers.FirstOrDefault().CosmosFunctionalUser;

            var transactions = fileOriginal.Batchs.SelectMany(p => p.Transactions);            

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            try
            {
                groupingBank.AsParallel().ForAll(item =>
                {
                    var uniqueKey = HiResDateTime.UtcNowTicks.ToString();

                    var xmlString = DominioOpenBatchLogica.GetOpenBatchInclearingCheckXmlRequest(uniqueKey, configuration, cosmosFunctionalUser, item.Bank, check);

                    EnvioTransaccionOpenBatchInclearingCheck(xmlString, userSoeid, newGuid, errorMessages, item.Bank, check, uniqueKey, currentCulture, fileOriginal, currencyId, countryOriginal, listProcessingLog);
                });                  
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }                      

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesOpenBatch(listaExceptions);
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
        private void EnvioTransaccionOpenBatchInclearingCheck(string xmlMessage, string userSoeid, Guid newGuid, IEnumerable<ErrorMessage> errorMessages, Bank bankConfiguration, DailyBrand check, string uniqueKey, int currentCulture, Dominio.Core.File fileOriginal, int currencyId, Country countryOriginal, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            int codeReturn = 0;

            this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xmlMessage, EventTypeId = EventType.OpenBatch }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchAlreadyOpened.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchAlreadyOpened, EventType.OpenBatch));
                            IngresarEventoTransaccionOpenBatchErrorInclearingCheck(bankConfiguration, errorMessages, int.Parse(codigoError), descripcion, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);
                            SetBankBatchOpen(userSoeid, bankConfiguration, check);                            
                        }
                        else
                        {
                            listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codigoError, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                            InsertErrorMessageOpenBatchInclearingCheckCodeReturn0(xmlMessage, userSoeid, newGuid, errorMessages, bankConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, errorMessage, codigoError, descripcion);                            
                        }
                    }
                    else
                    {
                        listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.OpenBatchSucessfully.GetDescription(), null, fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.OpenBatchSucessfully, EventType.OpenBatch));
                        IngresarEventoTransaccionOpenBatchOKInclearingCheck(bankConfiguration, newGuid, xmlMessage, replyMessage, userSoeid, uniqueKey, fileOriginal.Id);
                        SetBankBatchOpen(userSoeid, bankConfiguration, check);                                               
                    }
                }
                else
                {
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToOpenBatch.GetDescription(), codeReturn.ToString(), fileOriginal.Name, fileOriginal.Id, newGuid, ProcessFlag.UnableToOpenBatch, EventType.OpenBatch));
                    InsertErrorMessageOpenBatchInclearingCheckCodeReturnNo0(xmlMessage, userSoeid, newGuid, errorMessages, bankConfiguration, uniqueKey, currentCulture, fileOriginal.Id, replyMessage, codeReturn, errorMessage);
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

            var internErrorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (errorMessage != null)
            {
                var mensaje = currentCulture == 0 ? String.Format("There was an error when try to open the batch: {0}", internErrorMessage.EnglishText) : String.Format("Hubo un error cuando se trato de abrir el batch: {0}", internErrorMessage.SpanishText);                

                throw new AplicacionExcepcion(mensaje);
            }
            else
            {
                var mensaje = currentCulture == 0 ? string.Format("There was an error when try to open the batch. Contact with your administrator. Error Code: {0}", codeReturn) : string.Format("Hubo un error cuando se trato de abrir el batch. Contactese con su administrador. Código Error : {0}", codeReturn);

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
            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

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
        private void EnvioTransaccionesUploadInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;
            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            try
            {
                groupingBank.AsParallel().ForAll(bankItem =>
                {
                    var transactionFilterStateNoUploadToCosmos = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.Import || c.StatusId == TransactionState.CitiscreeningOk));
                    var transactionFilterStateUploadError = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.UploadToCosmos || c.StatusId == TransactionState.UploadToCosmosRejected || c.StatusId == TransactionState.UploadToCosmosClientAccountError || c.StatusId == TransactionState.UploadToCosmosHoldingAccountError)).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

                    var statusId = transactionFilterStateNoUploadToCosmos.Any() ? TransactionState.Import : (transactionFilterStateUploadError.Any() ? TransactionState.UploadToCosmosRejected : TransactionState.Import);

                    var listaSplitedStateNoUploadToCosmos = transactionFilterStateNoUploadToCosmos.Any() ? transactionFilterStateNoUploadToCosmos.Split(maxTransactionNumber) : new List<List<Transaction>>();
                    var listaSplitedStateErrorUploadToCosmos = transactionFilterStateUploadError.Any() ? transactionFilterStateUploadError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : new List<List<Transaction>>();

                    switch (statusId)
                    {
                        case TransactionState.Import:
                            try
                            {
                                listaSplitedStateNoUploadToCosmos.AsParallel().ForAll(item =>
                                {
                                    if (item.Any())
                                        ValidarTransaccionUploadStateNoUploadToCosmosInclearingCheck(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog);
                                });
                            }
                            catch (AggregateException ex)
                            {
                                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                            }
                            break;
                        case TransactionState.UploadToCosmosRejected:
                            try
                            {
                                listaSplitedStateErrorUploadToCosmos.AsParallel().ForAll(item =>
                                {
                                    if (item.Any())
                                        ValidarTransaccionUploadStateUploadToCosmosInclearingCheck(item, transactions, newGuid, countryOriginal, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog);
                                });
                            }
                            catch (AggregateException ex)
                            {
                                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                            }
                            break;
                    }
                });
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesUpload(listaExceptions);                
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
        private void ValidarTransaccionUploadStateUploadToCosmosInclearingCheck(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bank, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Upload && p.EventOnlineLog.StatusId == TransactionState.UploadToCosmos || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosRejected || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.UploadToCosmosHoldingAccountError)).OrderByDescending(p => p.EventOnlineLog.Date).Select(p => p.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            eventOnlineGroupByUniqueKey.AsParallel().ForAll(eventOnline =>
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
                    ValidarTransactionUploadResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);
            });
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
        private void ValidarTransaccionUploadStateNoUploadToCosmosInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bank, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Upload }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionUploadResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);
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
        private void EnvioTransaccionesAuthorizeInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            try
            {
                groupingBank.AsParallel().ForAll(bankItem =>
                {
                    var transactionFilterStateNoAuthorize = transactions.Where(c => c.BankId == bankItem.Key && c.StatusId == TransactionState.UploadToCosmosButNotAuthorized);
                    var transactionFilterStateAuthorizeError = transactions.Where(c => c.BankId == bankItem.Key && (c.StatusId == TransactionState.Authorize || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError)).Where(p => p.TransactionEventOnlineLogs.Any(c => c.ErrorMessageId.HasValue));

                    var statusId = transactionFilterStateNoAuthorize.Any() ? TransactionState.UploadToCosmosButNotAuthorized : (transactionFilterStateAuthorizeError.Any() ? TransactionState.AuthorizeByCosmosRejected : TransactionState.UploadToCosmosButNotAuthorized);

                    var listaSplitedStateAuthorizeToCosmosNotAuthorized = transactionFilterStateNoAuthorize.Any() ? transactionFilterStateNoAuthorize.Split(maxTransactionNumber) : new List<List<Transaction>>();
                    var listaSplitedStateErrorAuthorizeToCosmos = transactionFilterStateAuthorizeError.Any() ? transactionFilterStateAuthorizeError.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => p.ErrorMessageId.HasValue).Where(p => p.ErrorMessage.FinalReprocessOptionId == FinalReprocessOption.Reprocess || (p.ErrorMessage.IsManualReproAfterAutomaticRetry.HasValue && p.ErrorMessage.IsManualReproAfterAutomaticRetry.Value)).Select(p => p.Transaction).Split(maxTransactionNumber) : new List<List<Transaction>>();

                    switch (statusId)
                    {
                        case TransactionState.UploadToCosmosButNotAuthorized:
                            try
                            {
                                listaSplitedStateAuthorizeToCosmosNotAuthorized.AsParallel().ForAll(item =>
                                {
                                    if (item.Any())
                                        ValidarTransaccionAuthorizeStateNotAuthorizeInclearingCheck(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog);
                                });
                            }
                            catch (AggregateException ex)
                            {
                                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                            }
                            break;
                        case TransactionState.AuthorizeByCosmosRejected:
                            try
                            {
                                listaSplitedStateErrorAuthorizeToCosmos.AsParallel().ForAll(item =>
                                {
                                    if (item.Any())
                                        ValidarTransaccionAuthorizeStateAuthorizeInclearingCheck(item, transactions, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog);
                                });
                            }
                            catch (AggregateException ex)
                            {
                                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                            }
                            break;
                    }
                });
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesAuthorize(listaExceptions);
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
        private void ValidarTransaccionAuthorizeStateAuthorizeInclearingCheck(List<Transaction> splitList, IEnumerable<Transaction> listOriginal, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            var replyMessage = string.Empty;
            var codeReturn = 0;

            var eventOnlineGroupByUniqueKey = splitList.SelectMany(p => p.TransactionEventOnlineLogs).Where(p => !string.IsNullOrEmpty(p.EventOnlineLog.UniqueKeyCosmos) && (p.EventOnlineLog.EventTypeId == EventType.Authorize && p.EventOnlineLog.StatusId == TransactionState.Authorize || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || p.EventOnlineLog.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError)).OrderByDescending(p => p.EventOnlineLog.Date).Select(c => c.EventOnlineLog).GroupBy(p => p.UniqueKeyCosmos).Select(p => new { Key = p.Key, ListEventOnlineLog = p.ToList() });

            eventOnlineGroupByUniqueKey.AsParallel().ForAll(eventOnline =>
            {
                var listaTransactionOnlyUniqueKeyCosmos = listOriginal.Where(p => p.TransactionEventOnlineLogs.Any(c => c.EventOnlineLog.UniqueKeyCosmos == eventOnline.Key));

                var postAccountingMode = splitList.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

                this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                if (codeReturn != 0 && errorMessage != null)
                {
                    for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                    {
                        this.messageQueueFactory.Send(new Message() { Body = eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                        var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                        if (errorMessageInternal != null)
                            if (int.Parse(errorMessageInternal.Code) == 0)
                                break;
                    }
                }

                if (!string.IsNullOrEmpty(replyMessage))
                    ValidarTransactionAuthorizeResponse(replyMessage, eventOnline.ListEventOnlineLog.FirstOrDefault().XmlRequest, codeReturn, listaTransactionOnlyUniqueKeyCosmos.ToList(), errorMessages, newGuid, userSoeid, eventOnline.Key, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);
            });          
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
        private void ValidarTransaccionAuthorizeStateNotAuthorizeInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Authorize }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionAuthorizeResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, postAccountingMode, countryOriginal.Id, currencyId, listProcessingLog);
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
        private void EnvioTransaccionesDeleteInclearingCheck(IEnumerable<Transaction> transactions, Country countryOriginal, PostAccountingConfiguration configuration, string userSoeid, DailyBrand check, Guid newGuid, int currencyId, List<ProcessingFileLog> listProcessingLog)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

            var postAccountingMode = transactions.FirstOrDefault().Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionTypeConfigurationCountry.PostAccountModeId.Value;

            var maxTransactionNumber = postAccountingMode == PostAccountMode.Both ? (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value / 2 : 6) : (configuration.MaxNumberTransaction.HasValue ? configuration.MaxNumberTransaction.Value : 12);

            var errorMessages = this.errorMessageRepositorio.Obtener(p => p.System.SystemParameter.Identifier == "cosmos" && p.FieldStatusId == FieldStatus.Active, null, "System, NachaCode").ToList();

            var groupingBank = transactions.Select(p => p.Bank).GroupBy(c => c.Id).Select(d => new { Key = d.Key, Bank = d.FirstOrDefault() });

            try
            {
                groupingBank.AsParallel().ForAll(bankItem =>
                {
                    var transactionClientAccountError = transactions.Where(c => (c.StatusId == TransactionState.AuthorizeByCosmosRejected || c.StatusId == TransactionState.AuthorizeByCosmosClientAccountError || c.StatusId == TransactionState.AuthorizeByCosmosHoldingAccountError));

                    var listaSplitedClientAccountError = transactionClientAccountError.Any() ? transactionClientAccountError.SelectMany(p => p.TransactionEventOnlineLogs).Select(p => p.Transaction).Split(maxTransactionNumber) : new List<List<Transaction>>();

                    try 
                    {
                        listaSplitedClientAccountError.AsParallel().ForAll(item => {
                            if (item.Any())
                                ValidarTransaccionDeleteStateClientAccountErrorInclearingCheck(item, newGuid, countryOriginal, configuration, errorMessages, userSoeid, bankItem.Bank, check, currencyId, listProcessingLog);
                        });
                    }
                    catch (AggregateException ex)
                    {
                        FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
                    }                    
                });
            }
            catch (AggregateException ex)
            {
                FlattenAggregateExceptions(ex).ForEach(p => exceptions.Enqueue(p));
            }

            // Throw the exceptions here after the loop completes.
            if (exceptions.Any())
            {
                var listaExceptions = new List<Exception>();

                exceptions.ToList().ForEach(p => listaExceptions.AddRange(p.FlattenHierarchy()));

                ValidarExcepcionesDelete(listaExceptions);                  
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
        private void ValidarTransaccionDeleteStateClientAccountErrorInclearingCheck(List<Transaction> splitList, Guid newGuid, Country countryOriginal, PostAccountingConfiguration configuration, IEnumerable<ErrorMessage> errorMessages, string userSoeid, Bank bankConfiguration, DailyBrand check, int currencyId, List<ProcessingFileLog> listProcessingLog)
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

            var errorMessage = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

            if (codeReturn != 0 && errorMessage != null)
            {
                for (var i = 0; i < errorMessage.AutomaticRetriesNumber; i++)
                {
                    this.messageQueueFactory.Send(new Message() { Body = xDeclaration.ToString(SaveOptions.DisableFormatting), EventTypeId = EventType.Delete }, p => { replyMessage = p.BodyAs<string>(); codeReturn = p.Code; });

                    var errorMessageInternal = errorMessages.FirstOrDefault(p => Int16.Parse(p.Code) == codeReturn);

                    if (errorMessageInternal != null)
                        if (int.Parse(errorMessageInternal.Code) == 0)
                            break;
                }
            }

            if (!string.IsNullOrEmpty(replyMessage))
                ValidarTransactionDeleteResponse(replyMessage, xDeclaration.ToString(), codeReturn, splitList, errorMessages, newGuid, userSoeid, uniqueKey, PostAccountMode.ClientAccount, TransactionState.DeletedInCosmosClientAccountError, countryOriginal.Id, currencyId, listProcessingLog);
        }
        
        #endregion

        #endregion

        #region ReturnFile

        /// <summary>
        /// Método que obtiene la lista de transacciones, verifica y genera el archivo de retorno a Paybank
        /// </summary>
        /// <param name="listTransaction"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="userSoeid"></param>
        /// <param name="currencyId"></param>
        /// <param name="newGuid"></param>
        /// <param name="currentCulture"></param>
        private void CreatePaybankReturnFile(IEnumerable<Transaction> listTransaction, Country countryOriginal, PaybankReturnFileConfiguration configuration, string userSoeid, int currencyId, Guid newGuid, int currentCulture, List<ProcessingFileLog> listaProcessingLog)
        {
            var eventLogFilterResult = new List<EventOnlineLog>();
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;

            try
            {
                var arrTrandeFileId = listTransaction.SelectMany(p => p.TransactionTrandeFiles).Where(p => p.TrandeFile.EventTypeId == EventType.PaybankReturnFile && p.TrandeFile.EventStateId == EventState.Complete).Select(p => p.TransactionId).ToArray();

                if (listTransaction.All(p => arrTrandeFileId.Contains(p.Id) && p.TransactionTrandeFiles.All(d => d.TrandeFile.EventTypeId == EventType.PaybankReturnFile && d.TrandeFile.EventStateId == EventState.Complete)))
                {
                    listaProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToWriteInAPaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
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
                    listaProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.UnableToGeneratePaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
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
                    listaProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.PaybankReturnFileSucessfullyGenerated.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.PaybankReturnFileSucessfullyGenerated, EventType.PaybankReturnFile));
                }
                else
                {
                    listaProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ProcessFlag.NothingToWriteInAPaybankReturnFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInAPaybankReturnFile, EventType.PaybankReturnFile));
                }
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                listaProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyId, ex.Message, null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGeneratePaybankReturnFile, EventType.PaybankReturnFile));

                var transactions = listTransaction.Where(p => (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue && ((p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().EventOnlineLog.EventTypeId == EventType.Upload || (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().EventOnlineLog.EventTypeId == EventType.Authorize) && (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.CategoryErrorId == CategoryError.Business);

                IngresarEventoFileReturnError(transactions.Any() ? transactions : new List<Transaction>(), nombreArchivo, null, ex.Message, userSoeid);
            }
        }


        /// <summary>
        /// Método que ingresa un evento correcto asociado al file return 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="userSoeid"></param>
        private void IngresarEventoFileReturnOK(IEnumerable<Transaction> transactions, string fileName, byte[] file, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.PaybankReturnFileGenerationOk;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TransactionId = transaction.Id,
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
        /// Método que ingresa un evento incorrecto al file return
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="message"></param>
        /// <param name="userSoeid"></param>
        private void IngresarEventoFileReturnError(IEnumerable<Transaction> transactions, string fileName, byte[] file, string message, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.PaybankReturnFileGenerationError;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TransactionId = transaction.Id,
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
        private void CreateFACPReturnFile(IEnumerable<Transaction> listTransaction, Country countryOriginal, PostPaylinkConfiguration configuration, string userSoeid, Currency currencyOriginal, Guid newGuid, List<ProcessingFileLog> listProcessingLog)
        {
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;

            try
            {
                var nombreArchivoArr = listTransaction.FirstOrDefault().Batch.File.Name.Split('.');

                var connectionName = GetPaybankConnectionName(countryOriginal, currencyOriginal);

                var paybankOriginalTransaction = this.paybankRepositorio.ObtenerOriginalPaybankTransaction(nombreArchivoArr[1], connectionName);

                if (paybankOriginalTransaction != null)
                {
                    if (!paybankOriginalTransaction.Any())
                    {
                        listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
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
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
                    return;
                }

                if (listTransaction.Any(p => p.TransactionTrandeFiles.Any(c => c.TrandeFile.EventStateId == EventState.Complete)))
                {
                    listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.NothingToWriteInFACPFile.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInFACPFile, EventType.FACPFile));
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

                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ProcessFlag.FACPFileSuccessfullyGenerated.GetDescription(), null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.FACPFileSuccessfullyGenerated, EventType.FACPFile));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                listProcessingLog.Add(this.adaptadorFile.FabricarProcessingLogFileEvent(countryOriginal.Id, currencyOriginal.Id, ex.Message, null, listTransaction.FirstOrDefault().Batch.File.Name, listTransaction.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGenerateFACPFile, EventType.FACPFile));

                var logTransactions = listTransaction.Where(p => !((p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue) || (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessageId.HasValue && (p.TransactionEventOnlineLogs.OrderBy(c => c.Id)).LastOrDefault().ErrorMessage.Code != "9999");

                IngresarEventoFACPFileReturnError(logTransactions, nombreArchivo, null, ex.Message, userSoeid);
            }
        }

        /// <summary>
        /// Método que registra un evento FACP trande event online log
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="userSoeid"></param>
        private void IngresarEventoFACPFileReturnOK(IEnumerable<Transaction> transactions, string fileName, byte[] file, string userSoeid)
        {
            foreach (Transaction transaction in transactions)
            {
                if (transaction.TransactionTrandeFiles == null)
                    transaction.TransactionTrandeFiles = new HashSet<TransactionTrandeFile>();

                transaction.StatusId = TransactionState.FACPFileGenerationOk;

                transaction.TransactionTrandeFiles.Add(new TransactionTrandeFile
                {
                    TransactionId = transaction.Id,
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
                    TransactionId = transaction.Id,
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

        #region TrandeFlexCube

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
        /// <param name="fileId"></param>
        /// <param name="countryId"></param>
        /// <param name="userSoeid"></param>
        private void CreateTrandeFlexCubeFile(IEnumerable<Transaction> listaTransactions, PostAccountingFlexcubeConfiguration configuration, int currencyId, string userSoeid, int countryId, Guid newGuid, int currentCulture)
        {
            string nombreArchivo = string.Empty;
            string rutaArchivo = string.Empty;

            try
            {
                bool localCurrency = false;

                if (listaTransactions.All(p => p.StatusId == TransactionState.TrandeGenerationOK))
                {
                    this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.NothingToWriteInATrandeFile.GetDescription(), null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.NothingToWriteInATrandeFile, EventType.FlexcubeFile));
                    return;
                }

                var currenyOriginal = this.currencyRepositorio.Encontrar(currencyId);

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

                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ProcessFlag.TrandeFileSucessfullyGenerated.GetDescription(), null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.TrandeFileSucessfullyGenerated, EventType.FlexcubeFile));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                this.processingFileLogRepositorio.Crear(this.adaptadorFile.FabricarProcessingLogFileEvent(countryId, currencyId, ex.Message, null, listaTransactions.FirstOrDefault().Batch.File.Name, listaTransactions.FirstOrDefault().Batch.FileId, newGuid, ProcessFlag.UnableToGenerateTrandeFile, EventType.FlexcubeFile));

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

                transaction.StatusId = TransactionState.TrandeGenerationOK;

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

        #endregion

        #region Email

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private MailMessage GenerarEmailSchedulerCloseBatch(IEnumerable<TransactionTypeBatchConfiguration> batchConfiguration)
        {
            try
            {
                var mensaje = FabricaEmail.ObtenerEmailCloseBatch(batchConfiguration,
                "TemplateEmailCloseBatch",
                "",
                "Email Scheduler Close Batch",
                "noreply@minsal.cl");

                return mensaje;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Custom Log Entry Process

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customLog"></param>
        private void InsertCustomLogEntryErrorLog(string mensaje, int eventId, string title, string userSoeid)
        {
            var customLogEntry = new CustomLogEntry
            {
                Message = mensaje,
                Severity = System.Diagnostics.TraceEventType.Error,
                Title = title,
                CustomData = userSoeid,
                EventId = eventId
            };


            Logger.Write(customLogEntry);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// 
        /// </summary>
        ~FileServicioParallel()
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
                FileDispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void FileDispose()
        {
            if (this.fileRepositorio != null)
            {
                if (this.fileRepositorio is IDisposable)
                {
                    ((IDisposable)this.fileRepositorio).Dispose();
                }

                this.fileRepositorio = null;
            }

            if (this.countryRepositorio != null)
            {
                if (this.countryRepositorio is IDisposable)
                {
                    ((IDisposable)this.countryRepositorio).Dispose();
                }

                this.countryRepositorio = null;
            }

            if (this.paybankConfigurationRepositorio != null)
            {
                if (this.paybankConfigurationRepositorio is IDisposable)
                {
                    ((IDisposable)this.paybankConfigurationRepositorio).Dispose();
                }

                this.paybankConfigurationRepositorio = null;
            }

            if (this.transactionTypeConfigurationRepositorio != null)
            {
                if (this.transactionTypeConfigurationRepositorio is IDisposable)
                {
                    ((IDisposable)this.transactionTypeConfigurationRepositorio).Dispose();
                }

                this.transactionTypeConfigurationRepositorio = null;
            }

            if (this.messageQueueFactory != null)
            {
                if (this.messageQueueFactory is IDisposable)
                {
                    ((IDisposable)this.messageQueueFactory).Dispose();
                }

                this.messageQueueFactory = null;
            }

            if (this.errorMessageRepositorio != null)
            {
                if (this.errorMessageRepositorio is IDisposable)
                {
                    ((IDisposable)this.errorMessageRepositorio).Dispose();
                }

                this.errorMessageRepositorio = null;
            }

            if (this.returnCodeMappingConfigurationRepositorio != null)
            {
                if (this.returnCodeMappingConfigurationRepositorio is IDisposable)
                {
                    ((IDisposable)this.returnCodeMappingConfigurationRepositorio).Dispose();
                }

                this.returnCodeMappingConfigurationRepositorio = null;
            }

            if (this.transactionTypeBatchConfigurationRepositorio != null)
            {
                if (this.transactionTypeBatchConfigurationRepositorio is IDisposable)
                {
                    ((IDisposable)this.transactionTypeBatchConfigurationRepositorio).Dispose();
                }

                this.transactionTypeBatchConfigurationRepositorio = null;
            }

            if (this.bankRepositorio != null)
            {
                if (this.bankRepositorio is IDisposable)
                {
                    ((IDisposable)this.bankRepositorio).Dispose();
                }

                this.bankRepositorio = null;
            }

            if (this.eventOnlineLogRepositorio != null)
            {
                if (this.eventOnlineLogRepositorio is IDisposable)
                {
                    ((IDisposable)this.eventOnlineLogRepositorio).Dispose();
                }

                this.eventOnlineLogRepositorio = null;
            }

            if (this.transactionTypeConfigurationCountryRepositorio != null)
            {
                if (this.transactionTypeConfigurationCountryRepositorio is IDisposable)
                {
                    ((IDisposable)this.transactionTypeConfigurationCountryRepositorio).Dispose();
                }

                this.transactionTypeConfigurationCountryRepositorio = null;
            }

            if (this.currencyRepositorio != null)
            {
                if (this.currencyRepositorio is IDisposable)
                {
                    ((IDisposable)this.currencyRepositorio).Dispose();
                }

                this.currencyRepositorio = null;
            }

            if (this.paybankRepositorio != null)
            {
                if (this.paybankRepositorio is IDisposable)
                {
                    ((IDisposable)this.paybankRepositorio).Dispose();
                }

                this.paybankRepositorio = null;
            }

            if (this.processingFileLogRepositorio != null)
            {
                if (this.processingFileLogRepositorio is IDisposable)
                {
                    ((IDisposable)this.processingFileLogRepositorio).Dispose();
                }

                this.processingFileLogRepositorio = null;
            }

            if (this.adaptadorFile != null)
            {
                if (this.adaptadorFile is IDisposable)
                {
                    ((IDisposable)this.adaptadorFile).Dispose();
                }

                this.adaptadorFile = null;
            }

            if (this.transactionRepositorio != null)
            {
                if (this.transactionRepositorio is IDisposable)
                {
                    ((IDisposable)this.transactionRepositorio).Dispose();
                }

                this.transactionRepositorio = null;
            }

            if (this.mapper != null)
            {
                if (this.mapper is IDisposable)
                {
                    ((IDisposable)this.mapper).Dispose();
                }

                this.mapper = null;
            }

            if (this.emailServicio != null)
            {
                if (this.emailServicio is IDisposable)
                {
                    ((IDisposable)this.emailServicio).Dispose();
                }

                this.emailServicio = null;
            }

            if (this.schedulerPostACHConfigurationRepositorio != null)
            {
                if (this.schedulerPostACHConfigurationRepositorio is IDisposable)
                {
                    ((IDisposable)this.schedulerPostACHConfigurationRepositorio).Dispose();
                }

                this.schedulerPostACHConfigurationRepositorio = null;
            }
        }

        #endregion

        #region Métodos Post ACH Controller

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public bool VerifyProcessPostACHAutomatic(int countryId, int currencyId)
        {
            var answer = false;

            try
            {
                var schedulerConfigurationOriginal = this.schedulerPostACHConfigurationRepositorio.ObtenerPrimero(p => p.CountryId == countryId && p.CurrencyId == currencyId);

                if (schedulerConfigurationOriginal != null)
                    answer = schedulerConfigurationOriginal.ExecutionModeId == ExecutionMode.Automatic;
            }
            catch (Exception ex)
            {
                throw ProveedorExcepciones.ManejaExcepcion(ex, "PoliticaBase");
            }

            return answer;
        }

        #endregion
    }
}
