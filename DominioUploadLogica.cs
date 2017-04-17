using Dominio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Utilitarios.Base;
using Utilitarios.Excepciones;

namespace Dominio.Process
{
    public static class DominioUploadLogica
    {
        #region Global

        /// <summary>
        /// Declaration xml upload
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        public static XDocument GetXmlDeclaration(string uniqueKey)
        {
            XDocument xdeclaration = new XDocument(new XDeclaration("1.0", null, "yes"),
            new XElement("FCC_AC_SERVICE",
                new XElement("UNIQUE_KEY", uniqueKey)));

            return xdeclaration;
        }

        #endregion

        #region Inclearing True Transaction

        #region Métodos Publicos

        /// <summary>
        /// Método que verifica el mensaje a generar dependiendo del tipo de proceso asociado a las transacciones
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="xDeclaration"></param>
        /// <param name="transaction"></param>
        /// <param name="identificadorTransactionType"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="cosmosFunctionalUser"></param>
        /// <param name="transactionTypeBatchConfiguration"></param>
        public static void GetUploadMessageByTransactionType(Country countryOriginal, PostAccountingConfiguration configuration, XDocument xDeclaration, Transaction transaction, string identificadorTransactionType, PostAccountMode postAccountingMode, CosmosFunctionalUser cosmosFunctionalUser, TransactionTypeBatchConfiguration transactionTypeBatchConfiguration)
        {
            switch (identificadorTransactionType)
            {
                case "incomingelectronic":
                    switch (postAccountingMode)
                    {
                        case PostAccountMode.ClientAccount:
                            var xelementClient = GetUploadXmlRequestClientOnlyClient(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal, identificadorTransactionType);
                            SetXmlPostAccountingClientMode(xDeclaration, xelementClient);
                            break;
                        case PostAccountMode.HoldingAccount:
                            var xelementHoldingAccount = GetUploadXmlRequestIncomingHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            SetXmlPostAccountingHoldingAccountMode(xDeclaration, xelementHoldingAccount);
                            break;
                        case PostAccountMode.Both:
                            var xelementClientBoth = GetUploadXmlRequestIncomingClient(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            var xelementHoldingAccountBoth = GetUploadXmlRequestIncomingHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            SetXmlPostAccountingBothMode(xDeclaration, xelementClientBoth, xelementHoldingAccountBoth);
                            break;
                    };
                    break;
                case "return":
                    switch (postAccountingMode)
                    {
                        case PostAccountMode.ClientAccount:
                            var xelementClient = GetUploadXmlRequestClientOnlyClient(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal, identificadorTransactionType);
                            SetXmlPostAccountingClientMode(xDeclaration, xelementClient);
                            break;
                        case PostAccountMode.HoldingAccount:
                            var xelementHoldingAccount = GetUploadXmlRequestReturnHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            SetXmlPostAccountingHoldingAccountMode(xDeclaration, xelementHoldingAccount);
                            break;
                        case PostAccountMode.Both:
                            var xelementClientBoth = GetUploadXmlRequestReturnClient(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            var xelementHoldingAccountBoth = GetUploadXmlRequestReturnHoldingAccount(configuration, cosmosFunctionalUser, transactionTypeBatchConfiguration, transaction, countryOriginal);
                            SetXmlPostAccountingBothMode(xDeclaration, xelementClientBoth, xelementHoldingAccountBoth);
                            break;
                    };
                    break;
            }
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// 
        /// </summary>
        internal static void SetXmlPostAccountingBothMode(XDocument declaration, XElement xElementClient, XElement xElementHolding)
        {
            declaration.Element("FCC_AC_SERVICE").Add(xElementClient);
            declaration.Element("FCC_AC_SERVICE").Add(xElementHolding);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void SetXmlPostAccountingClientMode(XDocument declaration, XElement xElementClient)
        {
            declaration.Element("FCC_AC_SERVICE").Add(xElementClient);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void SetXmlPostAccountingHoldingAccountMode(XDocument declaration, XElement xElementHolding)
        {
            declaration.Element("FCC_AC_SERVICE").Add(xElementHolding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestClientOnlyClient(PostAccountingConfiguration configuration, CosmosFunctionalUser user, TransactionTypeBatchConfiguration batchConfiguration, Transaction transactionOriginal, Country countryOriginal, string identificadorTransactionType)
        {
            var transactionCode = int.Parse(transactionOriginal.TransactionCode);

            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.Any(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode))
            {
                throw new ConfigurationExcepcion(culture == 0 ? string.Format("There is no active configuration for the Transaction Code : {0} - Transaction Id : {1} - Beneficiary Name : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName) : string.Format("No existe una configuración activa para el Código de Transacción : {0} - Id Transacción : {1} - Nombre Beneficiario : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName));
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
            new XElement("UPLOAD_ACCTXNS",
                new XElement("REMITTER_NAME", GetUploadRemitterNameOnlyClient(countryOriginal, transactionOriginal)),
                new XElement("REMITTER_ID", GetUploadRemitterIdOnlyClient(countryOriginal, transactionOriginal)),
                new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdOnlyClient(countryOriginal, transactionOriginal)),
                new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryNameOnlyClient(countryOriginal, transactionOriginal)),
                new XElement("ACC", identificadorTransactionType == "incomingelectronic" ? (string.IsNullOrEmpty(transactionOriginal.AccountNumber.Trim()) ? string.Empty : transactionOriginal.AccountNumber.Trim()) : (string.IsNullOrEmpty(transactionOriginal.OriginatorAccount) ? string.Empty : transactionOriginal.OriginatorAccount.Trim())),
                new XElement("ADDL_TEXT", ""),
                new XElement("AMT", transactionOriginal.Amount.ToString()),
                new XElement("BC_INFO",
                    new XElement("APA_NUMBER", string.IsNullOrEmpty(configuration.APACosmos) ? string.Empty : configuration.APACosmos.Trim()),
                    new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                    new XElement("BC_CBATCH", batchConfiguration.BatchContingent),
                    new XElement("BC_DBATCH", batchConfiguration.BatchNumber),
                    new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                    new XElement("BC_USER", user.MakerACH),
                    new XElement("BENEFICIARY", ""),
                    new XElement("MIS_CCY", ""),
                    new XElement("MIS_SPREAD", ""),
                    new XElement("THIRD_PARTY", "")),
                new XElement("BRN", "000"),
                new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "C" : "D"),
                new XElement("INSTR_CODE", trackingNumber),
                new XElement("SCODE", "ACH"),
                new XElement("TCODE", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode).MappingCode),
                new XElement("TRNDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("UPBRN", "000"),
                new XElement("VALDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("XRATE", ""),
                new XElement("XREF",
                    new XElement("NODENAME", Environment.MachineName),
                    new XElement("SERIAL", "000001"),
                    new XElement("SUPERVISOR", user.CheckerACH),
                    new XElement("TERMINAL", Environment.MachineName),
                    new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                    new XElement("USER", user.MakerACH)),
                new XElement("CUSTOMER_ID", "N/A"),
                new XElement("COLLECT_CODE", "1"),
                new XElement("COLLECTION_NAME", "NA")
                ));

            return xElement;
        }

        /// <summary>
        /// Método que devuelve el remitter name dependiendo del pais
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadRemitterNameOnlyClient(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterName = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim();
            }
            else
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim();

                if (!string.IsNullOrEmpty(transactionOriginal.Addenda2))
                {
                    int asteristIndex = transactionOriginal.Addenda2.IndexOf('*');

                    remitterName = asteristIndex > 0 ? transactionOriginal.Addenda2.Trim().Substring(transactionOriginal.Addenda2.LastIndexOf('*') + 1) : transactionOriginal.Addenda2.Trim();
                }
            }

            return remitterName;
        }

        /// <summary>
        /// Método que devuelve el remitter id dependiendo del pais
        /// </summary>
        /// <returns></returns>
        internal static string GetUploadRemitterIdOnlyClient(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterId = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Batch.OriginatorId) ? string.Empty : transactionOriginal.Batch.OriginatorId.Trim();
            }
            else
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Addenda2) ? transactionOriginal.Addenda2.Trim() : (string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim());
            }

            return remitterId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string GetUploadEntityCodeIdOnlyClient(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }
            else
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }

            return entityCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string GetUploadBeneficiaryNameOnlyClient(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                if (countryOriginal.Banks.Any(p => p.FieldStatusId == FieldStatus.Active && p.BankTypeId == BankType.NonFinancialInstitution && p.Name.ToUpper() == transactionOriginal.BeneficiaryName.ToUpper()))
                {
                    entityCode = string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim();
                }
                else
                {
                    entityCode = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
                }
            }
            else
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
            }

            return entityCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestIncomingClient(PostAccountingConfiguration configuration, CosmosFunctionalUser user, TransactionTypeBatchConfiguration batchConfiguration, Transaction transactionOriginal, Country countryOriginal)
        {
            var transactionCode = int.Parse(transactionOriginal.TransactionCode);

            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.Any(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode))
            {
                throw new ConfigurationExcepcion(culture == 0 ? string.Format("There is no active configuration for the Transaction Code : {0} - Transaction Id : {1} - Beneficiary Name : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName) : string.Format("No existe una configuración activa para el Código de Transacción : {0} - Id Transacción : {1} - Nombre Beneficiario : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName));
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
            new XElement("UPLOAD_ACCTXNS",
                new XElement("REMITTER_NAME", GetUploadRemitterNameIncoming(countryOriginal, transactionOriginal)),
                new XElement("REMITTER_ID", GetUploadRemitterIdIncoming(countryOriginal, transactionOriginal)),
                new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdIncoming(countryOriginal, transactionOriginal)),
                new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryName(countryOriginal, transactionOriginal)),
                new XElement("ACC", string.IsNullOrEmpty(transactionOriginal.AccountNumber.Trim()) ? string.Empty : transactionOriginal.AccountNumber.Trim()),
                new XElement("ADDL_TEXT", GetUploadAddlTextIncomingClient(countryOriginal, transactionOriginal)),
                new XElement("AMT", transactionOriginal.Amount.ToString()),
                new XElement("BC_INFO",
                   new XElement("APA_NUMBER", string.IsNullOrEmpty(configuration.APACosmos) ? string.Empty : configuration.APACosmos.Trim()),
                    new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                    new XElement("BC_CBATCH", batchConfiguration.BatchContingent),
                    new XElement("BC_DBATCH", batchConfiguration.BatchNumber),
                    new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                    new XElement("BC_USER", user.MakerACH),
                    new XElement("BENEFICIARY", ""),
                    new XElement("MIS_CCY", ""),
                    new XElement("MIS_SPREAD", ""),
                    new XElement("THIRD_PARTY", "")),
                new XElement("BRN", "000"),
                new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "C" : "D"),
                new XElement("INSTR_CODE", trackingNumber),
                new XElement("SCODE", "ACH"),
                new XElement("TCODE", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode).MappingCode),
                new XElement("TRNDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("UPBRN", "000"),
                new XElement("VALDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("XRATE", ""),
                new XElement("XREF",
                    new XElement("NODENAME", Environment.MachineName),
                    new XElement("SERIAL", "000001"),
                    new XElement("SUPERVISOR", user.CheckerACH),
                    new XElement("TERMINAL", Environment.MachineName),
                    new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                    new XElement("USER", user.MakerACH)),
                new XElement("CUSTOMER_ID", "N/A"),
                new XElement("COLLECT_CODE", "1"),
                new XElement("COLLECTION_NAME", "NA")
                ));

            return xElement;
        }

        /// <summary>
        /// Método que devuelve el remitter name dependiendo del pais
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadRemitterNameIncoming(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterName = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim();

                if (!string.IsNullOrEmpty(transactionOriginal.Addenda2))
                {
                    int asteristIndex = transactionOriginal.Addenda2.IndexOf('*');

                    remitterName = asteristIndex > 0 ? transactionOriginal.Addenda2.Trim().Substring(transactionOriginal.Addenda2.LastIndexOf('*') + 1) : transactionOriginal.Addenda2.Trim();
                }
            }
            else
            {
                remitterName = transactionOriginal != null ? transactionOriginal.Batch.Originator.Trim() : string.Empty;
            }

            return remitterName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadRemitterIdIncoming(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterId = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Batch.OriginatorId) ? string.Empty : transactionOriginal.Batch.OriginatorId.Trim();

                if (!string.IsNullOrEmpty(transactionOriginal.Addenda2))
                {
                    int asteristIndex = transactionOriginal.Addenda2.IndexOf('*');

                    remitterId = asteristIndex > 0 ? transactionOriginal.Addenda2.Trim().Substring(0, asteristIndex) : transactionOriginal.Addenda2.Trim();
                }
            }
            else
            {
                remitterId = transactionOriginal.Batch.OriginatorId != null ? transactionOriginal.Batch.OriginatorId.Trim() : string.Empty;
            }

            return remitterId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadEntityCodeIdIncoming(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }
            else
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }

            return entityCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadBeneficiaryName(Country countryOriginal, Transaction transactionOriginal)
        {
            var beneficiaryName = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                if (countryOriginal.Banks.Any(p => p.FieldStatusId == FieldStatus.Active && p.BankTypeId == BankType.NonFinancialInstitution && p.Name.ToUpper() == transactionOriginal.BeneficiaryName.ToUpper()))
                {
                    beneficiaryName = string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim();
                } 
                else
                {
                    beneficiaryName = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
                }
            }
            else
            {
                beneficiaryName = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
            }

            return beneficiaryName;
        }

        /// <summary>
        /// Método que obtiene el valor final asociado a la etiqueta ADDL_TEXT 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadAddlTextIncomingClient(Country countryOriginal, Transaction transactionOriginal)
        {
            var addlText = string.Empty;

            var arrCountryName = new string[] { "Guatemala", "Trinidad Y Tobago" };

            addlText = arrCountryName.Contains(countryOriginal.Name) ? (string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim()) : GetUploadRemitterNameIncoming(countryOriginal, transactionOriginal);

            return addlText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadAddlTextIncomingHolding(Country countryOriginal, Transaction transactionOriginal)
        {
            var addlText = string.Empty;

            addlText = transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).DebitText : transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).CreditText;

            if (countryOriginal.Name.Contains("Guatemala"))
                addlText = string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim();

            return addlText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestIncomingHoldingAccount(PostAccountingConfiguration configuration, CosmosFunctionalUser user, TransactionTypeBatchConfiguration batchConfiguration, Transaction transactionOriginal, Country countryOriginal)
        {
            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.Any(p => p.FieldStatusId == FieldStatus.Active))
            {
                throw new ConfigurationExcepcion(culture == 0 ? "There is no active configuration for the holding account" : "No existe una configuración activa para la cuenta puente");
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
                   new XElement("UPLOAD_ACCTXNS",
                       new XElement("REMITTER_NAME", GetUploadRemitterNameIncoming(countryOriginal, transactionOriginal)),
                       new XElement("REMITTER_ID", GetUploadRemitterIdIncoming(countryOriginal, transactionOriginal)),
                       new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdIncoming(countryOriginal, transactionOriginal)),
                       new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryName(countryOriginal, transactionOriginal)),
                       new XElement("ACC", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active).HoldingAccount.Trim()),
                       new XElement("ADDL_TEXT", GetUploadAddlTextIncomingHolding(countryOriginal, transactionOriginal)),
                       new XElement("AMT", transactionOriginal.Amount.ToString()),
                       new XElement("BC_INFO",
                           new XElement("APA_NUMBER", string.IsNullOrEmpty(configuration.APACosmos) ? string.Empty : configuration.APACosmos.Trim()),
                           new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                           new XElement("BC_CBATCH", batchConfiguration.BatchContingent),
                           new XElement("BC_DBATCH", batchConfiguration.BatchNumber),
                           new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                           new XElement("BC_USER", user.MakerACH),
                           new XElement("BENEFICIARY", ""),
                           new XElement("MIS_CCY", ""),
                           new XElement("MIS_SPREAD", ""),
                           new XElement("THIRD_PARTY", "")),
                        new XElement("BRN", "000"),
                        new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                        new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "D" : "C"),
                        new XElement("INSTR_CODE", trackingNumber),
                        new XElement("SCODE", "ACH"),
                        new XElement("TCODE", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).DebitTransactionId : transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).CreditTransactionId),
                        new XElement("TRNDT", DateTime.Now.ToString("yyyyMMdd")),
                        new XElement("UPBRN", "000"),
                        new XElement("VALDT", DateTime.Now.ToString("yyyyMMdd")),
                        new XElement("XRATE", ""),
                        new XElement("XREF",
                            new XElement("NODENAME", Environment.MachineName),
                            new XElement("SERIAL", "000001"),
                            new XElement("SUPERVISOR", user.CheckerACH),
                            new XElement("TERMINAL", Environment.MachineName),
                            new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                            new XElement("USER", user.MakerACH)),
                        new XElement("CUSTOMER_ID", "N/A"),
                        new XElement("COLLECT_CODE", "1"),
                        new XElement("COLLECTION_NAME", "NA")
                        ));

            return xElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestReturnClient(PostAccountingConfiguration configuration, CosmosFunctionalUser user, TransactionTypeBatchConfiguration batchConfiguration, Transaction transactionOriginal, Country countryOriginal)
        {
            var transactionCode = int.Parse(transactionOriginal.TransactionCode);

            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.Any(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode))
            {
                throw new ConfigurationExcepcion(culture == 0 ? string.Format("There is no active configuration for the Transaction Code : {0} - Transaction Id : {1} - Beneficiary Name : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName) : string.Format("No existe una configuración activa para el Código de Transacción : {0} - Id Transacción : {1} - Nombre Beneficiario : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName));
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
                   new XElement("UPLOAD_ACCTXNS",
                       new XElement("REMITTER_NAME", GetUploadRemitterNameReturn(countryOriginal, transactionOriginal)),
                       new XElement("REMITTER_ID", GetUploadRemitterIdReturn(countryOriginal, transactionOriginal)),
                       new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdReturn(countryOriginal, transactionOriginal)),
                       new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryNameReturn(countryOriginal, transactionOriginal)),
                       new XElement("ACC", transactionOriginal.OriginatorAccount),
                       new XElement("ADDL_TEXT", string.Format("{0}{1}", transactionOriginal.BeneficiaryId, transactionOriginal.Addenda)),
                       new XElement("AMT", transactionOriginal.Amount.ToString()),
                       new XElement("BC_INFO",
                           new XElement("APA_NUMBER", string.IsNullOrEmpty(configuration.APACosmos) ? string.Empty : configuration.APACosmos.Trim()),
                           new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                           new XElement("BC_CBATCH", batchConfiguration.BatchContingent),
                           new XElement("BC_DBATCH", batchConfiguration.BatchNumber),
                           new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                           new XElement("BC_USER", user.MakerACH),
                           new XElement("BENEFICIARY", ""),
                           new XElement("MIS_CCY", ""),
                           new XElement("MIS_SPREAD", ""),
                           new XElement("THIRD_PARTY", "")),
                        new XElement("BRN", "000"),
                        new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                        new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "C" : "D"),
                        new XElement("INSTR_CODE", trackingNumber.Length > 10 ? trackingNumber.Substring(trackingNumber.Length - 10) : trackingNumber),
                        new XElement("SCODE", "ACH"),
                        new XElement("TCODE", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode).MappingCode),
                        new XElement("TRNDT", transactionOriginal.Batch.File.CreationDate.Value.ToString("yyyyMMdd")),
                        new XElement("UPBRN", "000"),
                        new XElement("VALDT", transactionOriginal.Batch.File.CreationDate.Value.ToString("yyyyMMdd")),
                        new XElement("XRATE", ""),
                        new XElement("XREF",
                            new XElement("NODENAME", Environment.MachineName),
                            new XElement("SERIAL", "000001"),
                            new XElement("SUPERVISOR", user.CheckerACH),
                            new XElement("TERMINAL", Environment.MachineName),
                            new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                            new XElement("USER", user.MakerACH)),
                        new XElement("CUSTOMER_ID", "N/A"),
                        new XElement("COLLECT_CODE", "1"),
                        new XElement("COLLECTION_NAME", "NA")
                        ));

            return xElement;
        }

        /// <summary>
        /// Método que devuelve el remitter name dependiendo del pais
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        private static string GetUploadRemitterNameReturn(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterName = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim();

                if (!string.IsNullOrEmpty(transactionOriginal.Addenda2))
                {
                    remitterName = transactionOriginal.Addenda2.Trim();
                }
            }
            else
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator;
            }

            return remitterName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetUploadRemitterIdReturn(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterId = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Batch.OriginatorId) ? string.Empty : transactionOriginal.Batch.OriginatorId.Trim();

                if (!string.IsNullOrEmpty(transactionOriginal.Addenda2))
                {
                    remitterId = transactionOriginal.Addenda2.Trim();
                }
            }
            else
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Batch.OriginatorId) ? string.Empty : transactionOriginal.Batch.OriginatorId;
            }

            return remitterId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadEntityCodeIdReturn(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }
            else
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }

            return entityCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadBeneficiaryNameReturn(Country countryOriginal, Transaction transactionOriginal)
        {
            string beneficiaryName = string.Empty;

            if (!countryOriginal.Name.Contains("Jamaica"))
            {
                if (countryOriginal.Banks.Any(p => p.FieldStatusId == FieldStatus.Active && p.BankTypeId == BankType.NonFinancialInstitution && p.Name.ToUpper() == transactionOriginal.BeneficiaryName.ToUpper()))
                {
                    beneficiaryName = string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim();
                }
                else
                {
                    beneficiaryName = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
                }
            }
            else
            {
                beneficiaryName = string.IsNullOrEmpty(transactionOriginal.BeneficiaryName) ? string.Empty : transactionOriginal.BeneficiaryName.Trim();
            }

            return beneficiaryName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="batchConfiguration"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestReturnHoldingAccount(PostAccountingConfiguration configuration, CosmosFunctionalUser user, TransactionTypeBatchConfiguration batchConfiguration, Transaction transactionOriginal, Country countryOriginal)
        {
            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.Any(p => p.FieldStatusId == FieldStatus.Active))
            {
                throw new ConfigurationExcepcion(culture == 0 ? "There is no active configuration for the holding account" : "No existe una configuración activa para la cuenta puente");
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
                   new XElement("UPLOAD_ACCTXNS",
                       new XElement("REMITTER_NAME", GetUploadRemitterNameReturn(countryOriginal, transactionOriginal)),
                       new XElement("REMITTER_ID", GetUploadRemitterIdReturn(countryOriginal, transactionOriginal)),
                       new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdReturn(countryOriginal, transactionOriginal)),
                       new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryNameReturn(countryOriginal, transactionOriginal)),
                       new XElement("ACC", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).HoldingAccount.Trim()),
                       new XElement("ADDL_TEXT", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).DebitText : transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).CreditText),
                       new XElement("AMT", transactionOriginal.Amount.ToString()),
                       new XElement("BC_INFO",
                           new XElement("APA_NUMBER", string.IsNullOrEmpty(configuration.APACosmos) ? string.Empty : configuration.APACosmos.Trim()),
                           new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                           new XElement("BC_CBATCH", batchConfiguration.BatchContingent),
                           new XElement("BC_DBATCH", batchConfiguration.BatchNumber),
                           new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                           new XElement("BC_USER", user.MakerACH),
                           new XElement("BENEFICIARY", ""),
                           new XElement("MIS_CCY", ""),
                           new XElement("MIS_SPREAD", ""),
                           new XElement("THIRD_PARTY", "")),
                        new XElement("BRN", "000"),
                        new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                        new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "D" : "C"),
                        new XElement("INSTR_CODE", trackingNumber.Length > 10 ? trackingNumber.Substring(trackingNumber.Length - 10) : trackingNumber),
                        new XElement("SCODE", "ACH"),
                        new XElement("TCODE", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).DebitTransactionId : transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Accountings.FirstOrDefault(a => a.FieldStatusId == FieldStatus.Active).CreditTransactionId),
                        new XElement("TRNDT", DateTime.Now.ToString("yyyyMMdd")),
                        new XElement("UPBRN", "000"),
                        new XElement("VALDT", DateTime.Now.ToString("yyyyMMdd")),
                        new XElement("XRATE", ""),
                        new XElement("XREF",
                            new XElement("NODENAME", Environment.MachineName),
                            new XElement("SERIAL", "000001"),
                            new XElement("SUPERVISOR", user.CheckerACH),
                            new XElement("TERMINAL", Environment.MachineName),
                            new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                            new XElement("USER", user.MakerACH)),
                        new XElement("CUSTOMER_ID", "N/A"),
                        new XElement("COLLECT_CODE", "1"),
                        new XElement("COLLECTION_NAME", "NA")
                        ));

            return xElement;
        }

        #endregion

        #endregion

        #region Inclearing Check

        #region Métodos Publicos

        /// <summary>
        /// Método que verifica el mensaje a generar dependiendo del tipo de proceso asociado a las transacciones
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="configuration"></param>
        /// <param name="xDeclaration"></param>
        /// <param name="transaction"></param>
        /// <param name="identificadorTransactionType"></param>
        /// <param name="postAccountingMode"></param>
        /// <param name="cosmosFunctionalUser"></param>
        /// <param name="transactionTypeBatchConfiguration"></param>
        public static void GetUploadMessageByTransactionTypeInclearingCheck(Country countryOriginal, PostAccountingConfiguration configuration, XDocument xDeclaration, Transaction transaction, PostAccountMode postAccountingMode, CosmosFunctionalUser cosmosFunctionalUser, Bank bankConfiguration, DailyBrand check)
        {
            switch (postAccountingMode)
            {
                case PostAccountMode.ClientAccount:
                    var xelementClientAccount = GetUploadXmlRequestInclearingCheck(configuration, cosmosFunctionalUser, transaction, countryOriginal, bankConfiguration, check);
                    DominioUploadLogica.SetXmlPostAccountingClientMode(xDeclaration, xelementClientAccount);
                    break;
                case PostAccountMode.HoldingAccount:
                    var xelementHoldingAccount = GetUploadXmlRequestInclearingCheck(configuration, cosmosFunctionalUser, transaction, countryOriginal, bankConfiguration, check);
                    DominioUploadLogica.SetXmlPostAccountingClientMode(xDeclaration, xelementHoldingAccount);
                    break;
                case PostAccountMode.Both:
                    var xelementBoth = GetUploadXmlRequestInclearingCheck(configuration, cosmosFunctionalUser, transaction, countryOriginal, bankConfiguration, check);
                    DominioUploadLogica.SetXmlPostAccountingClientMode(xDeclaration, xelementBoth);
                    break;
            };
        }

        #endregion

        #region Métodos Privados
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="user"></param>
        /// <param name="transactionOriginal"></param>
        /// <param name="countryOriginal"></param>
        /// <param name="bankConfiguration"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        private static XElement GetUploadXmlRequestInclearingCheck(PostAccountingConfiguration configuration, CosmosFunctionalUser user, Transaction transactionOriginal, Country countryOriginal, Bank bankConfiguration, DailyBrand check)
        {
            var transactionCode = Convert.ToByte(transactionOriginal.BeneficiaryName.Substring(4, 2));

            var culture = HttpContext.Current != null ? (int)HttpContext.Current.Session["CurrentCulture"] : 0;

            if (!transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.Any(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode))
            {
                throw new ConfigurationExcepcion(culture == 0 ? string.Format("There is no active configuration for the Transaction Code : {0} - Transaction Id : {1} - Beneficiary Name : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName) : string.Format("No existe una configuración activa para el Código de Transacción : {0} - Id Transacción : {1} - Nombre Beneficiario : {2}", transactionCode, transactionOriginal.Id, transactionOriginal.BeneficiaryName));
            }

            var trackingNumber = long.Parse(transactionOriginal.TrackingNumber).ToString();

            XElement xElement = new XElement(
            new XElement("UPLOAD_ACCTXNS",
                new XElement("REMITTER_NAME", GetUploadRemitterNameInclearingCheck(countryOriginal, transactionOriginal)),
                new XElement("REMITTER_ID", GetUploadRemitterIdInclearingCheck(countryOriginal, transactionOriginal)),
                new XElement("ENTITY_CODE_ID", GetUploadEntityCodeIdInclearingCheck(countryOriginal, transactionOriginal)),
                new XElement("BENEFICIARY_NAME", GetUploadBeneficiaryNameInclearingCheck(countryOriginal, transactionOriginal)),
                new XElement("ACC", string.IsNullOrEmpty(transactionOriginal.AccountNumber.Trim()) ? string.Empty : transactionOriginal.AccountNumber.Trim()),
                new XElement("ADDL_TEXT", ""),
                new XElement("AMT", transactionOriginal.Amount.ToString()),
                new XElement("BC_INFO",
                    new XElement("APA_NUMBER", configuration.APACosmos),
                    new XElement("BC_BRANCH", configuration.BasicCosmosBranch),
                    new XElement("BC_CBATCH", check == DailyBrand.AM ? bankConfiguration.MorningBatchContingent : bankConfiguration.AfternoonBatchContingent),
                    new XElement("BC_DBATCH", check == DailyBrand.AM ? bankConfiguration.MorningBatch : bankConfiguration.AfternoonBatch),
                    new XElement("BC_ENV", configuration.BasicCosmosEnviroment),
                    new XElement("BC_USER", user.MakerACH),
                    new XElement("BENEFICIARY", ""),
                    new XElement("MIS_CCY", ""),
                    new XElement("MIS_SPREAD", ""),
                    new XElement("THIRD_PARTY", "")),
                new XElement("BRN", "000"),
                new XElement("CCY", transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.Currency.Code),
                new XElement("DRCR", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? "C" : "D"),
                new XElement("INSTR_CODE", string.IsNullOrEmpty(transactionOriginal.BeneficiaryId) ? string.Empty : transactionOriginal.BeneficiaryId.Trim()),
                new XElement("SCODE", "ACH"),
                new XElement("TCODE", transactionOriginal.NatureTransactionId == NatureTransaction.Credit ? transactionCode.ToString() : transactionOriginal.Batch.File.TransactionTypeConfigurationCountryCurrency.TransactionCodes.FirstOrDefault(p => p.FieldStatusId == FieldStatus.Active && int.Parse(p.PaybankCode) == transactionCode).MappingCode),
                new XElement("TRNDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("UPBRN", "000"),
                new XElement("VALDT", DateTime.Now.ToString("yyyyMMdd")),
                new XElement("XRATE", ""),
                new XElement("XREF",
                    new XElement("NODENAME", Environment.MachineName),
                    new XElement("SERIAL", "000001"),
                    new XElement("SUPERVISOR", user.CheckerACH),
                    new XElement("TERMINAL", Environment.MachineName),
                    new XElement("TIMESTAMP", DateTime.Now.ToString("yyyy-MM-dd:hh:mm:ss").Replace(" ", "").Replace("-", "").Replace(":", "")),
                    new XElement("USER", user.MakerACH)),
                new XElement("CUSTOMER_ID", "N/A"),
                new XElement("COLLECT_CODE", "1"),
                new XElement("COLLECTION_NAME", "NA")
                ));

            return xElement;
        }

        /// <summary>
        /// Método que devuelve el remitter name dependiendo del pais
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadRemitterNameInclearingCheck(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterName = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                remitterName = string.IsNullOrEmpty(transactionOriginal.Batch.Originator) ? string.Empty : transactionOriginal.Batch.Originator.Trim();
            }

            return remitterName;
        }

        /// <summary>
        /// Método que devuelve el remitter id dependiendo del pais
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadRemitterIdInclearingCheck(Country countryOriginal, Transaction transactionOriginal)
        {
            var remitterId = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                remitterId = string.IsNullOrEmpty(transactionOriginal.Batch.OriginatorId) ? string.Empty : transactionOriginal.Batch.OriginatorId.Trim();
            }

            return remitterId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadEntityCodeIdInclearingCheck(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                entityCode = string.IsNullOrEmpty(transactionOriginal.FinancialInstitutionOriginCode) ? string.Empty : transactionOriginal.FinancialInstitutionOriginCode.Trim();
            }

            return entityCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryOriginal"></param>
        /// <param name="transactionOriginal"></param>
        /// <returns></returns>
        internal static string GetUploadBeneficiaryNameInclearingCheck(Country countryOriginal, Transaction transactionOriginal)
        {
            var entityCode = string.Empty;

            if (countryOriginal.Name.Contains("Jamaica"))
            {
                if (countryOriginal.Banks.Any(p => p.FieldStatusId == FieldStatus.Active && p.BankTypeId == BankType.NonFinancialInstitution && p.Name.ToUpper() == transactionOriginal.BeneficiaryName.ToUpper()))
                {
                    entityCode = string.IsNullOrEmpty(transactionOriginal.Addenda) ? string.Empty : transactionOriginal.Addenda.Trim();
                }
            }

            return entityCode;
        }

        #endregion

        #endregion
    }
}
