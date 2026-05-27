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

        public List<Entity> GetPurchaseList(IOrganizationService service, int topCount, int statuspurchase)
        {
            if (topCount <= 0)
            {
                topCount = 50;
            }
            QueryExpression query = new QueryExpression(Purchase.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And),
                TopCount = topCount,
                NoLock = true
            };
            query.Criteria.AddCondition(Purchase.StatusPurchase, ConditionOperator.Equal, statuspurchase); // In Attesa
            query.Criteria.AddCondition(Purchase.IsExpired, ConditionOperator.Equal, false);
            query.Criteria.AddCondition(Purchase.Status, ConditionOperator.Equal, 0); // Active
            query.AddOrder(Purchase.ExpirationDate, OrderType.Ascending);

            EntityCollection result = service.RetrieveMultiple(query);

            return result.Entities.ToList();
        }        
        public void ExpireUpdatePurchase(IOrganizationService service, Entity purchase, DateTime? newExpirationDate)
        {
            if (purchase.Id == Guid.Empty)
            {
                throw new InvalidPluginExecutionException("Purchase Id non valido.");
            }

            Entity purchaseUpdate = new Entity(Purchase.LogicalName)
            {
                Id = purchase.Id
            };
            if (newExpirationDate != null)
            {
                purchaseUpdate[Purchase.ExpirationDate] = newExpirationDate.Value;
            }
            else
            {
                //purchaseUpdate[Purchase.StatusPurchase] = new OptionSetValue(126400002); // Annullato
                purchaseUpdate[Purchase.CancelReason] = new OptionSetValue(126400000); // ExpiredCart
                purchaseUpdate[Purchase.IsExpired] = true;
            }

            service.Update(purchaseUpdate);
            //string purchaseName = purchase.GetAttributeValue<string>(Purchase.Name) ?? purchase.Id.ToString();
        }

    }
}


