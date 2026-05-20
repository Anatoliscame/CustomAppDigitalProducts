using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugin.sc_DigitalProduct.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.Helper
{
    public class PurchaseOrderlineHelper
    {
        public PurchaseOrderlineHelper() { }

        public List<Entity> GetOrderAcquisto(IOrganizationService service, Entity targetUpdateAcquisto)
        {
            QueryExpression query = new QueryExpression(PurchaseOrderLine.LogicalName)
            {
                ColumnSet = new ColumnSet(PurchaseOrderLine.PurchaseOrderlineId, PurchaseOrderLine.KeyDigitalProduct, PurchaseOrderLine.PurchaseId, PurchaseOrderLine.DigitalProductId),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(PurchaseOrderLine.PurchaseId, ConditionOperator.Equal, targetUpdateAcquisto.Id);
            query.NoLock = true;
            //query.TopCount = 1;

            // Collega l'entità KeyGame alla query (join)
            LinkEntity keyProductLink = new LinkEntity
            {
                LinkFromEntityName = PurchaseOrderLine.LogicalName,
                LinkFromAttributeName = PurchaseOrderLine.KeyDigitalProduct,
                LinkToEntityName = KeyDigitalProduct.LogicalName,
                LinkToAttributeName = KeyDigitalProduct.KeyDigitalProductName,
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet(KeyDigitalProduct.KeyDigitalProductId, KeyDigitalProduct.StatusPresentKey),
                EntityAlias = "PurchaseOrderLineKeyProduct"
            };
            keyProductLink.LinkCriteria.AddCondition(KeyDigitalProduct.StatusPresentKey, ConditionOperator.Equal, 126400004); //Temporaneamente

            query.LinkEntities.Add(keyProductLink);// KeyProduct

            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }

        public List<Entity> GetOrderAcquistoEspansion(IOrganizationService service, Guid target)
        {
            QueryExpression query = new QueryExpression(PurchaseOrderLineExpansion.LogicalName)
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(PurchaseOrderLineExpansion.PurchaseOrderLineId, ConditionOperator.Equal, target);
            query.NoLock = true;
            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }
    }
}

