

namespace FunctionApp.sc_DigitalProducts.Entities
{
    public class PurchaseOrderLine 
    {
        public const string LogicalName = "sc_purchaseorderline";
        public const string PurchaseOrderlineId = "sc_purchaseorderlineid";
        public const string Name = "sc_name";
        public const string AccountClientId = "sc_accountclientid";
        public const string DigitalProductId = "sc_digitalproductid";
        public const string PurchaseId = "sc_purchaseid";
        public const string KeyDigitalProduct = "sc_keydigitalproduct";
        public const string CreatedOn = "createdon";

        public const string SellingPrice = "sc_sellingprice"; // Prezzo Vendita
        public const string CommissionAmount = "sc_commissionamount"; // ImportoCommissione
        public const string AmountIVA = "sc_amountiva"; // ImportoIVA
        public const string TotalRow = "sc_totalrow"; // totaleriga

    }
}
