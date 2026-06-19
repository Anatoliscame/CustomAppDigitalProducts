using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.Model.Request
{
    public class CountryInfoNativa
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public decimal Threshold { get; set; }
    }

    public class CountryConfigNativa
    {
        public List<CountryInfoNativa> Countries { get; set; }
    }
}
