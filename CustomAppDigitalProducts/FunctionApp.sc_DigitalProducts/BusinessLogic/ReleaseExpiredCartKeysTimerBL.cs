using FunctionApp.sc_DigitalProducts.Entities;
using FunctionApp.sc_DigitalProducts.Interfaces;
using Microsoft.Extensions.Logging;


namespace FunctionApp.sc_DigitalProducts.BusinessLogic
{
    public class ReleaseExpiredCartKeysTimerBL : IReleaseExpiredCartKeysTimerBL
    {
        private readonly ILogger _logger;

        public ReleaseExpiredCartKeysTimerBL(ILogger logger)
        {
            _logger = logger;
        }

        public List<DigitalProduct> GetDigitalProductList(int IntTopQueryDigitalProduct)
        {
            throw new NotImplementedException();
        }
    }
}
