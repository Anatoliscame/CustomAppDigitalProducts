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
    public class PurchaseHelper
    {
        public PurchaseHelper() { }

        public EntityCollection GetAcquisto(IOrganizationService service, Entity targetNew)
        {
            QueryExpression acquistiQ = new QueryExpression(Purchase.LogicalName);
            acquistiQ.ColumnSet = new ColumnSet(true);
            acquistiQ.Criteria.AddCondition(Purchase.PurchaseId, ConditionOperator.NotEqual, targetNew.Id);
            return service.RetrieveMultiple(acquistiQ);
        }
         
        public Guid CreateAcquisto(IOrganizationService service, int quantitaAcquisto, Guid accountId, int KestatusAcquistoValue, string generatedCode, decimal totaleRiga)
        {
            Entity nuovoAcquisto = new Entity(Purchase.LogicalName);
            nuovoAcquisto[Purchase.Name] = $"acquisto" + quantitaAcquisto.ToString() + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            nuovoAcquisto[Purchase.AccountClientId] = new EntityReference("account", accountId); // Associa l'account
            nuovoAcquisto[Purchase.StatusPurchase] = new OptionSetValue(KestatusAcquistoValue); // Stato "In Attesa" (Assumendo che il valore sia 100000000)
            nuovoAcquisto[Purchase.Code] = generatedCode;
            nuovoAcquisto[Purchase.Total] = totaleRiga;
            nuovoAcquisto[Purchase.ExpirationDate] = DateTime.UtcNow;
            Guid acquistoId = service.Create(nuovoAcquisto);
            return acquistoId;
        }


        public List<Entity> GetAcquistoTargetAndInAttesa(IOrganizationService service, Entity entity)
        {
            QueryExpression query = new QueryExpression(Purchase.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(Purchase.StatusPurchase, ConditionOperator.Equal, 126400001); // In Attesa
            query.Criteria.AddCondition(Purchase.PurchaseId, ConditionOperator.NotEqual, entity.Id);
            query.NoLock = true;
            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }

        public List<Entity> GetAcquistoInAttesa(IOrganizationService service, Guid accountId)
        {
            QueryExpression query = new QueryExpression(Purchase.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(Purchase.StatusPurchase, ConditionOperator.Equal, 126400001); // In Attesa
            query.Criteria.AddCondition(Purchase.AccountClientId, ConditionOperator.Equal, accountId);
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
