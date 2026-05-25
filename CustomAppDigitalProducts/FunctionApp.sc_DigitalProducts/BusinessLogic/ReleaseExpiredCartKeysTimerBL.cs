using FunctionApp.sc_DigitalProducts.Entities;
using FunctionApp.sc_DigitalProducts.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


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

        public List<Entity> GetPurchaseInAttesaList(IOrganizationService service,int topQuery)
        {
            QueryExpression query = new QueryExpression(Purchase.LogicalName)
            {
                ColumnSet = new ColumnSet(Purchase.ExpirationDate, Purchase.StatusPurchase, Purchase.PurchaseId, Purchase.AccountClientId, Purchase.Code),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(Purchase.StatusReason, ConditionOperator.Equal, 1); // To be processed
            query.NoLock = true;
            query.TopCount = topQuery;

            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }

    }
}
