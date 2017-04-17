using Dominio.Core;
using FrameLog;
using FrameLog.Contexts;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Utilitarios.Auditoria;

namespace Datos.Persistencia.Core
{
    public class ContextoPrincipal : DbContext, IContextoUnidadDeTrabajo
    {
        #region Constructor

        /// <summary>
        /// Genera instancia de Contexto Principal
        /// </summary>
        public ContextoPrincipal()
            : base("DefaultConnection")
        {
            base.Configuration.LazyLoadingEnabled = true;            

            Auditor = new FrameLogModule<ControlCambios, UsuarioLog>(new FabricaCambios(), ContextoAuditoria);
            //Database.SetInitializer<ContextoPrincipal>(new MigrateDatabaseToLatestVersion<ContextoPrincipal, Datos.Persistencia.Core.Configuration>("DefaultConnection"));
        }

        #endregion

        #region Auditoria

        /// <summary>
        /// 
        /// </summary>
        public readonly FrameLogModule<ControlCambios, UsuarioLog> Auditor;

        /// <summary>
        /// 
        /// </summary>
        public IFrameLogContext<ControlCambios, UsuarioLog> ContextoAuditoria {
            get { return new AdaptadorContexto(this); }
        }

        #endregion

        #region Implementacion de Interfaz Unidad de Trabajo

        #region Miembros Auditoria

        IDbSet<ControlCambios> controlesCambios;
        IDbSet<CambioObjetos> cambiosObjetos;
        IDbSet<CambioPropiedades> cambiosPropiedades;

        #endregion

        #region Miembros

        IDbSet<GroupParameter> groupParameter;
        IDbSet<Parameter> parameter;
        IDbSet<Accounting> accounting;
        IDbSet<Bank> bank;
        IDbSet<BankCurrency> bankCurrency;
        IDbSet<Batch> batch;
        IDbSet<Business> business;
        IDbSet<CitiscreeningConfiguration> citiscreeningConfiguration;
        IDbSet<CitiscreeningConfigurationCitiscreeningField> citiscreeningConfigurationCitiscreeningField;
        IDbSet<CitiscreeningField> citiscreeningField;
        IDbSet<CosmosFunctionalUser> cosmosFunctionalUser;
        IDbSet<Country> country;
        IDbSet<CountryCosmosFunctionalUser> countryCosmosFunctionalUser;
        IDbSet<Currency> currency;
        IDbSet<EmailConfiguration> emailConfiguration;
        IDbSet<ErrorMessage> errorMessage;
        IDbSet<EventOnlineLog> eventOnlineLog;
        IDbSet<File> file;
        IDbSet<LoansPaymentFileConfiguration> loansPaymentFileConfiguration;
        IDbSet<LanguageUser> languageUser;
        IDbSet<NotificationEmail> notificationEmail;
        IDbSet<PaybankConfiguration> paybankConfiguration;
        IDbSet<PaybankConfigurationCurrency> paybankConfigurationCurrency;
        IDbSet<PaybankReturnFileConfiguration> paybankReturnFileConfiguration;
        IDbSet<PendingChange> pendingChange;
        IDbSet<PostPaylinkConfiguration> postPaylinkConfiguration;
        IDbSet<ReturnCodeMappingConfiguration> returnCodeMappingConfiguration;
        IDbSet<SchedulerClosingProcessesConfiguration> schedulerClosingProcessesConfiguration;
        IDbSet<SchedulerGeneralSettingConfiguration> schedulerGeneralSettingConfiguration;
        IDbSet<SchedulerPostACHConfiguration> schedulerPostACHConfiguration;
        IDbSet<SettlementParameterConfiguration> settlementParameterConfiguration;        
        IDbSet<TrandeFile> trandeFile;
        IDbSet<Transaction> transaction;
        IDbSet<TransactionCode> transactionCode;
        IDbSet<TransactionEventOnlineLog> transactionEventOnlineLog;
        IDbSet<TransactionTrandeFile> transactionTrandeFile;
        IDbSet<TransactionTypeBatchConfiguration> transactionTypeBatchConfiguration;
        IDbSet<TransactionTypeConfiguration> transactionTypeConfiguration;
        IDbSet<TransactionTypeConfigurationCountry> transactionTypeConfigurationCountry;
        IDbSet<TransactionTypeConfigurationCountryCurrency> transactionTypeConfigurationCountryCurrency;
        IDbSet<QueueConnectivity> queueConnectivity;
        IDbSet<ShippingConfiguration> shippingConfiguration;
        IDbSet<InterfaceSystem> interfaceSystem;
        IDbSet<Menu> menu;
        IDbSet<FlexcubeAllowedSoeid> flexcubeAllowedSoeid;
        IDbSet<SettlementEntry> settlementEntry;
        IDbSet<PendingChangeCitiscreening> pendingChangeCitiscreening;
        IDbSet<BatchConfigurationBatchEventOnlineLog> batchConfigurationBatchEventOnlineLog;
        IDbSet<BatchEventOnlineLog> batchEventOnlineLog;
        IDbSet<BankBatchBankEventOnlineLog> bankBatchBankEventOnlineLog;
        IDbSet<BatchBankEventOnlineLog> batchBankEventOnlineLog;
        IDbSet<SettlementEntrySettlementEventOnlineLog> settlementEntrySettlementEventOnlineLog;
        IDbSet<SettlementEventOnlineLog> settlementEventOnlineLog;
        IDbSet<AuditLog> auditLog;
        IDbSet<ProcessingFileLog> processingFileLog;
        IDbSet<BulkInsertSession> bulkInsertSession;
        IDbSet<SchedulerReportHistorical> schedulerReportHistorical;
        IDbSet<CitiscreeningPendingComposition> citiscreeningPendingComposition;
        IDbSet<EmailMakerCheckerSetting> emailMakerCheckerSetting;
        IDbSet<EmailMakerCheckerListSetting> emailMakerCheckerListSetting;

        #endregion

        #region Miembros Custom Database Trace Listener

        IDbSet<Category> category;
        IDbSet<CategoryLog> categoryLog;
        IDbSet<Log> log;

        #endregion

        #region Atributos y Definiciones de Tablas

        #region Auditoria

        public IDbSet<ControlCambios> ControlesCambios 
        {
            get { return controlesCambios ?? (controlesCambios = base.Set<ControlCambios>()); }
        }

        public IDbSet<CambioObjetos> CambiosObjetos
        {
            get { return cambiosObjetos ?? (cambiosObjetos = base.Set<CambioObjetos>()); }
        }

        public IDbSet<CambioPropiedades> CambiosPropiedades
        {
            get { return cambiosPropiedades ?? (cambiosPropiedades = base.Set<CambioPropiedades>()); }
        }

        #endregion

        #region Aplicación

        public IDbSet<GroupParameter> GroupParameter
        {
            get { return groupParameter ?? (groupParameter = base.Set<GroupParameter>()); }
        }

        public IDbSet<Parameter> Parameter
        {
            get { return parameter ?? (parameter = base.Set<Parameter>()); }
        }        

        public IDbSet<Accounting> Accounting
        {
            get { return accounting ?? (accounting = base.Set<Accounting>()); }
        }

        public IDbSet<Bank> Bank
        {
            get { return bank ?? (bank = base.Set<Bank>()); }       
        }

        public IDbSet<BankCurrency> BankCurrency
        {
            get { return bankCurrency ?? (bankCurrency = base.Set<BankCurrency>()); }           
        }

        public IDbSet<Batch> Batch
        {
            get { return batch ?? (batch = base.Set<Batch>()); }
        }

        public IDbSet<Business> Business
        {
            get { return business ?? (business = base.Set<Business>()); }      
        }

        public IDbSet<CitiscreeningConfiguration> CitiscreeningConfiguration
        {
            get { return citiscreeningConfiguration ?? (citiscreeningConfiguration = base.Set<CitiscreeningConfiguration>()); }
        }

        public IDbSet<CitiscreeningConfigurationCitiscreeningField> CitiscreeningConfigurationCitiscreeningField
        {
            get { return citiscreeningConfigurationCitiscreeningField ?? (citiscreeningConfigurationCitiscreeningField = base.Set<CitiscreeningConfigurationCitiscreeningField>()); }
        }

        public IDbSet<CitiscreeningField> CitiscreeningField
        {
            get { return citiscreeningField ?? (citiscreeningField = base.Set<CitiscreeningField>()); }
        }

        public IDbSet<CosmosFunctionalUser> CosmosFunctionalUser
        {
            get { return cosmosFunctionalUser ?? (cosmosFunctionalUser = base.Set<CosmosFunctionalUser>()); }
        }

        public IDbSet<Country> Country
        {
            get { return country ?? (country = base.Set<Country>()); }
        }

        public IDbSet<CountryCosmosFunctionalUser> CountryCosmosFunctionalUser
        {
            get { return countryCosmosFunctionalUser ?? (countryCosmosFunctionalUser = base.Set<CountryCosmosFunctionalUser>()); }
        }

        public IDbSet<Currency> Currency
        {
            get { return currency ?? (currency = base.Set<Currency>()); }
        }

        public IDbSet<EmailConfiguration> EmailConfiguration
        {
            get { return emailConfiguration ?? (emailConfiguration = base.Set<EmailConfiguration>()); }
        }

        public IDbSet<ErrorMessage> ErrorMessage
        {
            get { return errorMessage ?? (errorMessage = base.Set<ErrorMessage>()); }
        }

        public IDbSet<EventOnlineLog> EventOnlineLog
        {
            get { return eventOnlineLog ?? (eventOnlineLog = base.Set<EventOnlineLog>()); }
        }

        public IDbSet<File> File
        {
            get { return file ?? (file = base.Set<File>()); }
        }

        public IDbSet<LoansPaymentFileConfiguration> LoansPaymentFileConfiguration
        {
            get { return loansPaymentFileConfiguration ?? (loansPaymentFileConfiguration = base.Set<LoansPaymentFileConfiguration>()); }
        }

        public IDbSet<LanguageUser> LanguageUser
        {
            get { return languageUser ?? (languageUser = base.Set<LanguageUser>()); }
        }

        public IDbSet<NotificationEmail> NotificationEmail
        {
            get { return notificationEmail ?? (notificationEmail = base.Set<NotificationEmail>()); }
        }

        public IDbSet<PaybankConfiguration> PaybankConfiguration
        {
            get { return paybankConfiguration ?? (paybankConfiguration = base.Set<PaybankConfiguration>()); }
        }

        public IDbSet<PaybankConfigurationCurrency> PaybankConfigurationCurrency
        {
            get { return paybankConfigurationCurrency ?? (paybankConfigurationCurrency = base.Set<PaybankConfigurationCurrency>()); }
        }

        public IDbSet<PaybankReturnFileConfiguration> PaybankReturnFileConfiguration
        {
            get { return paybankReturnFileConfiguration ?? (paybankReturnFileConfiguration = base.Set<PaybankReturnFileConfiguration>()); }
        }

        public IDbSet<PendingChange> PendingChange
        {
            get { return pendingChange ?? (pendingChange = base.Set<PendingChange>()); }
        }       

        public IDbSet<PostPaylinkConfiguration> PostPaylinkConfiguration
        {
            get { return postPaylinkConfiguration ?? (postPaylinkConfiguration = base.Set<PostPaylinkConfiguration>()); }
        }

        public IDbSet<ReturnCodeMappingConfiguration> ReturnCodeMappingConfiguration
        {
            get { return returnCodeMappingConfiguration ?? (returnCodeMappingConfiguration = base.Set<ReturnCodeMappingConfiguration>()); }
        }

        public IDbSet<SchedulerClosingProcessesConfiguration> SchedulerClosingProcessesConfiguration
        {
            get { return schedulerClosingProcessesConfiguration ?? (schedulerClosingProcessesConfiguration = base.Set<SchedulerClosingProcessesConfiguration>()); }
        }

        public IDbSet<SchedulerGeneralSettingConfiguration> SchedulerGeneralSettingConfiguration
        {
            get { return schedulerGeneralSettingConfiguration ?? (schedulerGeneralSettingConfiguration = base.Set<SchedulerGeneralSettingConfiguration>()); }
        }

        public IDbSet<SchedulerPostACHConfiguration> SchedulerPostACHConfiguration
        {
            get { return schedulerPostACHConfiguration ?? (schedulerPostACHConfiguration = base.Set<SchedulerPostACHConfiguration>()); }
        }       

        public IDbSet<SettlementParameterConfiguration> SettlementParameterConfiguration
        {
            get { return settlementParameterConfiguration ?? (settlementParameterConfiguration = base.Set<SettlementParameterConfiguration>()); }
        }

        public IDbSet<TrandeFile> TrandeFile
        {
            get { return trandeFile ?? (trandeFile = base.Set<TrandeFile>()); }
        }

        public IDbSet<Transaction> Transaction
        {
            get { return transaction ?? (transaction = base.Set<Transaction>()); }
        }

        public IDbSet<TransactionCode> TransactionCode
        {
            get { return transactionCode ?? (transactionCode = base.Set<TransactionCode>()); }
        }

        public IDbSet<TransactionEventOnlineLog> TransactionEventOnlineLog
        {
            get { return transactionEventOnlineLog ?? (transactionEventOnlineLog = base.Set<TransactionEventOnlineLog>()); }
        }

        public IDbSet<TransactionTrandeFile> TransactionTrandeFile
        {
            get { return transactionTrandeFile ?? (transactionTrandeFile = base.Set<TransactionTrandeFile>()); }
        }

        public IDbSet<TransactionTypeBatchConfiguration> TransactionTypeBatchConfiguration
        {
            get { return transactionTypeBatchConfiguration ?? (transactionTypeBatchConfiguration = base.Set<TransactionTypeBatchConfiguration>()); }
        }

        public IDbSet<TransactionTypeConfiguration> TransactionTypeConfiguration
        {
            get { return transactionTypeConfiguration ?? (transactionTypeConfiguration = base.Set<TransactionTypeConfiguration>()); }
        }

        public IDbSet<TransactionTypeConfigurationCountry> TransactionTypeConfigurationCountry
        {
            get { return transactionTypeConfigurationCountry ?? (transactionTypeConfigurationCountry = base.Set<TransactionTypeConfigurationCountry>()); }
        }

        public IDbSet<TransactionTypeConfigurationCountryCurrency> TransactionTypeConfigurationCountryCurrency
        {
            get { return transactionTypeConfigurationCountryCurrency ?? (transactionTypeConfigurationCountryCurrency = base.Set<TransactionTypeConfigurationCountryCurrency>()); }
        }

        public IDbSet<QueueConnectivity> QueueConnectivity
        {
            get { return queueConnectivity ?? (queueConnectivity = base.Set<QueueConnectivity>()); }
        }

        public IDbSet<ShippingConfiguration> ShippingConfiguration
        {
            get { return shippingConfiguration ?? (shippingConfiguration = base.Set<ShippingConfiguration>()); }
        }

        public IDbSet<InterfaceSystem> InterfaceSystem
        {
            get { return interfaceSystem ?? (interfaceSystem = base.Set<InterfaceSystem>()); }
        }

        public IDbSet<Menu> Menu
        {
            get { return menu ?? (menu = base.Set<Menu>()); }
        }

        public IDbSet<FlexcubeAllowedSoeid> FlexcubeAllowedSoeid
        {
            get { return flexcubeAllowedSoeid ?? (flexcubeAllowedSoeid = base.Set<FlexcubeAllowedSoeid>()); }
        }

        public IDbSet<SettlementEntry> SettlementEntry
        {
            get { return settlementEntry ?? (settlementEntry = base.Set<SettlementEntry>()); }
        }

        public IDbSet<PendingChangeCitiscreening> PendingChangeCitiscreening
        {
            get { return pendingChangeCitiscreening ?? (pendingChangeCitiscreening = base.Set<PendingChangeCitiscreening>()); }
        }

        public IDbSet<BatchConfigurationBatchEventOnlineLog> BatchConfigurationBatchEventOnlineLog
        {
            get { return batchConfigurationBatchEventOnlineLog ?? (batchConfigurationBatchEventOnlineLog = base.Set<BatchConfigurationBatchEventOnlineLog>()); }
        }

        public IDbSet<BatchEventOnlineLog> BatchEventOnlineLog
        {
            get { return batchEventOnlineLog ?? (batchEventOnlineLog = base.Set<BatchEventOnlineLog>()); }
        }

        public IDbSet<BankBatchBankEventOnlineLog> BankBatchBankEventOnlineLog
        {
            get { return bankBatchBankEventOnlineLog ?? (bankBatchBankEventOnlineLog = base.Set<BankBatchBankEventOnlineLog>()); }
        }

        public IDbSet<BatchBankEventOnlineLog> BatchBankEventOnlineLog
        {
            get { return batchBankEventOnlineLog ?? (batchBankEventOnlineLog = base.Set<BatchBankEventOnlineLog>()); }
        }

        public IDbSet<SettlementEntrySettlementEventOnlineLog> SettlementEntrySettlementEventOnlineLog
        {
            get { return settlementEntrySettlementEventOnlineLog ?? (settlementEntrySettlementEventOnlineLog = base.Set<SettlementEntrySettlementEventOnlineLog>()); }
        }

        public IDbSet<SettlementEventOnlineLog> SettlementEventOnlineLog
        {
            get { return settlementEventOnlineLog ?? (settlementEventOnlineLog = base.Set<SettlementEventOnlineLog>()); }
        }

        public IDbSet<AuditLog> AuditLog
        {
            get { return auditLog ?? (auditLog = base.Set<AuditLog>()); }
        }

        public IDbSet<ProcessingFileLog> ProcessingFileLog
        {
            get { return processingFileLog ?? (processingFileLog = base.Set<ProcessingFileLog>()); }
        }

        public IDbSet<BulkInsertSession> BulkInsertSession
        {
            get { return bulkInsertSession ?? (bulkInsertSession = base.Set<BulkInsertSession>()); }
        }

        public IDbSet<SchedulerReportHistorical> SchedulerReportHistorical
        {
            get { return schedulerReportHistorical ?? (schedulerReportHistorical = base.Set<SchedulerReportHistorical>()); }
        }

        public IDbSet<CitiscreeningPendingComposition> CitiscreeningPendingComposition
        {
            get { return citiscreeningPendingComposition ?? (citiscreeningPendingComposition = base.Set<CitiscreeningPendingComposition>()); }
        }

        public IDbSet<EmailMakerCheckerSetting> EmailMakerCheckerSetting
        {
            get { return emailMakerCheckerSetting ?? (emailMakerCheckerSetting = base.Set<EmailMakerCheckerSetting>()); }
        }

        public IDbSet<EmailMakerCheckerListSetting> EmailMakerCheckerListSetting
        {
            get { return emailMakerCheckerListSetting ?? (emailMakerCheckerListSetting = base.Set<EmailMakerCheckerListSetting>()); }
        }     

        #endregion

        #region Custom Database Trace Listener

        public IDbSet<Category> Category
        {
            get { return category ?? (category = base.Set<Category>()); }
        }

        public IDbSet<CategoryLog> CategoryLog
        {
            get { return categoryLog ?? (categoryLog = base.Set<CategoryLog>()); }
        }

        public IDbSet<Log> Log
        {
            get { return log ?? (log = base.Set<Log>()); }
        }

        #endregion

        #endregion

        #region Métodos Base

        /// <summary>
        /// Setea ingreso de entidad a contexto
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <returns>Nueva instancia de contexto para entidad T</returns>
        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        /// <summary>
        /// Adjunta a contexto entidad asociada
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <param name="item">Item a adjuntar</param>
        public void Attach<T>(T item) where T : class
        {
            if (Entry(item).State == EntityState.Detached)
            {
                base.Set<T>().Attach(item);
            }
        }

        /// <summary>
        /// Configura estado de la entidad como "Modificado"
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <param name="item">Item a modificar</param>
        public void SetModified<T>(T item) where T : class
        {
            //var objeto = Entry(item);//Entry(item);
            Entry(item).State = EntityState.Modified;            
        }

        /// <summary>
        /// Configura estado de la entidad como "Modificado"
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <param name="item">Item a modificar</param>
        public void SetModifiedAutoDetectChangeDisabled<T>(T item) where T : class
        {
            base.Configuration.AutoDetectChangesEnabled = false;

            try
            {
                //var objeto = Entry(item);//Entry(item);
                Entry(item).State = EntityState.Modified;
            }
            finally
            {
                base.Configuration.AutoDetectChangesEnabled = true;
            }   
        }

        /// <summary>
        /// Configura estado de la entidad como "Modificado"
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <param name="item">Item a modificar</param>
        public void SetModifiedCollectionAutoDetectChangeDisabled<T>(IEnumerable<T> itemList) where T : class
        {
            base.Configuration.AutoDetectChangesEnabled = false;
            
            try
            {
                foreach (var item in itemList)
                {
                    Entry(item).State = EntityState.Modified;
                }
            }
            finally 
            {
                base.Configuration.AutoDetectChangesEnabled = true;
            }            
        }

        /// <summary>
        /// Ejecuta consulta en base de datos
        /// </summary>
        /// <typeparam name="T">Tipo entidad</typeparam>
        /// <param name="sqlQuery">Consulta SQL</param>
        /// <param name="parameters">Parámetros asociados a consulta SQL</param>
        /// <returns>Lista de resultados de consulta sql</returns>
        public IEnumerable<T> ExecuteQuery<T>(string sqlQuery, params object[] parameters)
        {
            return Database.SqlQuery<T>(sqlQuery, parameters);
        }

        /// <summary>
        /// Ejecuta comando SQL en base de datos
        /// </summary>
        /// <param name="sqlCommand">Comando SQL a ejecutar</param>
        /// <param name="parameters">Parámetros asociados a comando SQL</param>
        /// <returns>Indicador de operación</returns>
        public int ExecuteCommand(string sqlCommand, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(sqlCommand, parameters);
        }

        /// <summary>
        /// Confirma todos los cambios existentes en el contenedor (Commit)
        /// </summary>
        ///<remarks>
        /// Commit sobre la base de datos
        /// Si la entidad posee propiedades reparadas y ocurre algún problema de persistencia optimista,  
        /// se levantará una excepción
        ///</remarks>
        public void Confirmar()
        {
            base.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefrescarTodo()
        {                        
            foreach (var entity in this.ChangeTracker.Entries())
            {
                entity.Reload();
            }
        }

        /// <summary>
        /// Confirma todos los cambios existentes en el contenedor (Commit)
        /// </summary>
        ///<remarks>
        /// Commit sobre la base de datos
        /// Si la entidad posee propiedades reparadas y ocurre algún problema de persistencia optimista,  
        /// se levantará una excepción
        ///</remarks>
        public void ConfirmarAutoDetectChangeDisabled()
        {
            base.Configuration.AutoDetectChangesEnabled = false;
            base.Configuration.ValidateOnSaveEnabled = false;

            try
            {
                base.SaveChanges();
            }
            finally 
            {
                base.Configuration.AutoDetectChangesEnabled = true;
                base.Configuration.ValidateOnSaveEnabled = true;
            }
        }

        /// <summary>
        /// Confirma los cambios y genera registros asociados a auditoria de cambios para la entidad
        /// </summary>
        /// <param name="autor"></param>
        public void ConfirmarConAuditoria(string autor) 
        {
            Auditor.SaveChanges(new UsuarioLog()
            {
                Name = autor
            });
        }

        /// <summary>
        /// Confirma todos los cambios existentes en el contenedor (Commit)
        /// </summary>
        ///<remarks>
        /// Commit sobre la base de datos
        /// Si hay un problema de concurrencia "refrescará" los datos del cliente. Aproximación "Client wins"
        ///</remarks>
        public void ConfirmarYRefrescarCambios()
        {
            bool saveFailed;

            do
            {
                try
                {
                    base.SaveChanges();
                    saveFailed = false;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    ex.Entries.ToList()
                              .ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
                }
            } while (saveFailed);
        }

        /// <summary>
        /// Rollback de los cambios que se han producido en la Unit of Work y que están siendo observados por ella
        /// </summary>
        public void DeshacerCambios()
        {
            ChangeTracker.Entries()
                        .ToList()
                        .ForEach(entry => entry.State = EntityState.Unchanged);
        }

        #endregion

        #endregion

        #region Extensiones

        /// <summary>
        /// Gatilla eventos al momento de crear el modelo
        /// </summary>
        /// <param name="modelBuilder">Constructor de modelo de entidades</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new AccountingTypeConfiguration());
            modelBuilder.Configurations.Add(new BankCurrencyTypeConfiguration());
            modelBuilder.Configurations.Add(new BankTypeConfiguration());
            modelBuilder.Configurations.Add(new BatchTypeConfiguration());
            modelBuilder.Configurations.Add(new BusinessTypeConfiguration());
            modelBuilder.Configurations.Add(new CategoryLogTypeConfiguration());
            modelBuilder.Configurations.Add(new CategoryTypeConfiguration());            
            modelBuilder.Configurations.Add(new CitiscreeningConfigurationCitiscreeningFieldTypeConfiguration());
            modelBuilder.Configurations.Add(new CitiscreeningConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new CitiscreeningFieldTypeConfiguration());
            modelBuilder.Configurations.Add(new CosmosFunctionalUserTypeConfiguration());
            modelBuilder.Configurations.Add(new CountryCosmosFunctionalUserTypeConfiguration());
            modelBuilder.Configurations.Add(new CountryTypeConfiguration());
            modelBuilder.Configurations.Add(new CurrencyTypeConfiguration());
            modelBuilder.Configurations.Add(new EmailConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new ErrorMessageTypeConfiguration());
            modelBuilder.Configurations.Add(new EventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new FileTypeConfiguration());
            modelBuilder.Configurations.Add(new GroupParameterTypeConfiguration());
            modelBuilder.Configurations.Add(new LanguageUserTypeConfiguration());
            modelBuilder.Configurations.Add(new LoansPaymentFileConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new LogTypeConfiguration());
            modelBuilder.Configurations.Add(new NotificationEmailTypeConfiguration());
            modelBuilder.Configurations.Add(new ParameterTypeConfiguration());
            modelBuilder.Configurations.Add(new PaybankConfigurationCurrencyTypeConfiguration());
            modelBuilder.Configurations.Add(new PaybankConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PaybankReturnFileConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PendingChangeTypeConfiguration());
            modelBuilder.Configurations.Add(new PostAccountingConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PostCardCosmosConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PostCardSystemConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PostPaylinkConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new QueueConnectivityTypeConfiguration());
            modelBuilder.Configurations.Add(new ReturnCodeMappingConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new SchedulerClosingProcessesConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new SchedulerGeneralSettingConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new SchedulerPostACHConfigurationTypeConfiguration());            
            modelBuilder.Configurations.Add(new SettlementConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new SettlementParameterConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new TrandeFileTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionCodeTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionEventOnlineLogTypeConfiguration());            
            modelBuilder.Configurations.Add(new TransactionTrandeFileTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionTypeBatchConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionObjectTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionTypeConfigurationCountryCurrencyTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionTypeConfigurationCountryTypeConfiguration());
            modelBuilder.Configurations.Add(new TransactionTypeConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new PaybankConfigurationCountryTypeConfiguration());            
            modelBuilder.Configurations.Add(new ShippingConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new InterfaceSystemTypeConfiguration());
            modelBuilder.Configurations.Add(new MenuTypeConfiguration());
            modelBuilder.Configurations.Add(new PostAccountingFlexcubeConfigurationTypeConfiguration());
            modelBuilder.Configurations.Add(new FlexcubeAllowedSoeidTypeConfiguration());
            modelBuilder.Configurations.Add(new SettlementEntryTypeConfiguration());
            modelBuilder.Configurations.Add(new PendingChangeCitiscreeningTypeConfiguration());
            modelBuilder.Configurations.Add(new BatchConfigurationBatchEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new BatchEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new BankBatchBankEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new BatchBankEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new SettlementEntrySettlementEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new SettlementEventOnlineLogTypeConfiguration());
            modelBuilder.Configurations.Add(new AuditLogTypeConfiguration());
            modelBuilder.Configurations.Add(new ProcessingFileLogTypeConfiguration());
            modelBuilder.Configurations.Add(new BulkInsertSessionTypeConfiguration());
            modelBuilder.Configurations.Add(new SchedulerReportHistoricalTypeConfiguration());
            modelBuilder.Configurations.Add(new CitiscreeningPendingCompositionTypeConfiguration());
            modelBuilder.Configurations.Add(new EmailMakerCheckerSettingTypeConfiguration());
            modelBuilder.Configurations.Add(new EmailMakerCheckerListSettingTypeConfiguration());
        }

        #endregion

        #region Override Dispose

        /// <summary>
        /// Dispose Override
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
        
    }
}
