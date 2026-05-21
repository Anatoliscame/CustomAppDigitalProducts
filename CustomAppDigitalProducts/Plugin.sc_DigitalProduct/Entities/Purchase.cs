using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.Entities
{
    public class Purchase // Acquisto
    {
        public const string LogicalName = "sc_purchase";
        public const string PurchaseId = "sc_purchaseid";
        public const string Name = "sc_name";
        public const string Code = "sc_code";
        public const string AccountClientId = "sc_accountclientid";
        public const string PurchaseDate = "sc_purchasedate";
        public const string Invoice = "sc_invoice";
        public const string Total = "sc_total";
        public const string StatusPurchase = "sc_statuspurchase";
        public const string ModifiedBy = "modifiedby";
        public const string CreatedBy = "createdby";
        public const string Assignee = "ownerid";

        public const string StatusReason = "statuscode";
        public const string Status = "statecode";

        public const string CancelReason = "sc_cancelreason";
        public const string IsExpired = "sc_isexpired";
        public const string ExpirationDate = "sc_expirationdate"; //Data di scadenza
    } 
} 
