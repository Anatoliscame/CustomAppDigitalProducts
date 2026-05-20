using FunctionApp.sc_DigitalProducts.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp.sc_DigitalProducts.Interfaces
{
    public interface IReleaseExpiredCartKeysTimerBL
    {
        public List<DigitalProduct> GetDigitalProductList(int IntTopQueryDigitalProduct);
    }
}
 