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


        public List<Entity> GetPurchaseOrderLineList(IOrganizationService service, int topCount, Guid purchaseId)
        {

            QueryExpression query = new QueryExpression(PurchaseOrderLine.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And),
                TopCount = topCount+1,
                NoLock = true
            };
            query.Criteria.AddCondition(PurchaseOrderLine.PurchaseId, ConditionOperator.Equal, purchaseId);
            query.Criteria.AddCondition(PurchaseOrderLine.IsExpirationCalculationProcessed,ConditionOperator.Equal,false);
            query.Criteria.AddCondition(PurchaseOrderLine.StateCode, ConditionOperator.Equal, 0); // Active
            query.AddOrder(PurchaseOrderLine.CreatedOn, OrderType.Ascending);

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
                purchaseUpdate[Purchase.CancelReason] = new OptionSetValue(126400000); // ExpiredCart
                purchaseUpdate[Purchase.IsExpired] = true;
                purchaseUpdate[Purchase.StatusPurchase] = new OptionSetValue(126400002); // Annullato
            }

            service.Update(purchaseUpdate);
        }

        public void updatePurchaseOLIsExpiration(IOrganizationService service, Entity purchaseOL)
        {
            Entity updatePurchaseOL = new Entity(PurchaseOrderLine.LogicalName)
            {
                Id = purchaseOL.Id
            };
            updatePurchaseOL[PurchaseOrderLine.IsExpirationCalculationProcessed] = true;
            service.Update(updatePurchaseOL);
        }
        public List<DigitalProduct> GetDigitalProductList(int IntTopQueryDigitalProduct)
        {
            throw new NotImplementedException();
        }
    }
}


