using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.Entities
{
    public class DigitalProduct
    {
        public const string LogicalName = "sc_digitalproduct";
        public const string DigitalProductId = "sc_digitalproductid"; // Guid
        public const string Name = "sc_name";
        public const string ProductDetails = "sc_productdetailsid"; // Lookup
        public const string Codice = "sc_codice"; 
        public const string AccountClientId = "sc_accountclientid";
        public const string StatoDigitalProduct = "sc_statodigitalproduct"; // Optionset
        public const string TypeDigitalProduct = "sc_typedigitalproduct";// Optionset 
        public const string TypePlatform = "sc_typeplatform";// Optionset 
        public const string Key = "sc_key";// String 
        public const string BasePrice = "sc_baseprice";// Money 
        public const string StateCode = "statecode";
         
        public const string RequirementsNotesActivation = "sc_requirementsnotesactivation";//
        public const string StockVailable = "sc_stockavailable";// Money 
        public const string Description = "sc_description";// Memo 
          
        public const string ParentDigitalProductId = "sc_parentdigitalproductid";// Parent

    }
}
