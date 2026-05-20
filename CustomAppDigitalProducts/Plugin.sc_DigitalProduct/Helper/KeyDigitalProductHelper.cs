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
    public class KeyDigitalProductHelper
    {
        public KeyDigitalProductHelper() { }

        public List<Entity> ExistKeyProduct(IOrganizationService service, EntityReference productdigitaleTo, int statusKeyProduct, int? typePiattaforma)
        {

            QueryExpression query = new QueryExpression(KeyDigitalProduct.LogicalName)
            {
                ColumnSet = new ColumnSet(KeyDigitalProduct.KeyDigitalProductName),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(KeyDigitalProduct.DigitalProductId, ConditionOperator.Equal, productdigitaleTo.Id);
            query.Criteria.AddCondition(KeyDigitalProduct.StatusPresentKey, ConditionOperator.Equal, statusKeyProduct);
            query.Criteria.AddCondition(KeyDigitalProduct.TypePlatform, ConditionOperator.Equal, typePiattaforma.Value);
            query.NoLock = true;
            //query.TopCount = 1;
            // query.AddOrder("createdon", OrderType.Descending);
            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }

        public void UpdateKeyProduct(IOrganizationService service, Guid keyProductArray, int StatusKeyProduct)
        {
            Entity keyDigitalProduct = new Entity(KeyDigitalProduct.LogicalName);
            keyDigitalProduct.Id = keyProductArray;
            keyDigitalProduct[KeyDigitalProduct.StatusPresentKey] = new OptionSetValue(StatusKeyProduct);
            service.Update(keyDigitalProduct);
        }
    }
}
