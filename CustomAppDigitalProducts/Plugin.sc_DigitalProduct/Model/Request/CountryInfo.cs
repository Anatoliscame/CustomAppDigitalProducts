using System.Collections.Generic;

namespace Plugin.sc_DigitalProduct.Model.Request
{
    public class CountryInfo
    {
        public string Id { get; set; }
        public decimal Threshold { get; set; }
    }
    public class CountryConfig
    {
        public Dictionary<string, CountryInfo> Countries { get; set; }
    }
}
