using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.Entities;
using Plugin.sc_DigitalProduct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.BusinessLogicPlugins
{
    public class OnCreateUpdateStatusPurchaseRegisterPurchaseHistoryLogic
    {

        public void ExecuteStatusPurchaseRegister(IOrganizationService service, Entity target, Entity preImage, ITracingService trace, string nameEntityUpdate)
        {
            PurchaseHelper _acquistoHelper = new PurchaseHelper();

            int? typeStatusPurchaseNew = target.GetAttributeValue<OptionSetValue>(Purchase.StatusPurchase)?.Value;
            if (typeStatusPurchaseNew != 126400002) //Annulato
            {
                trace?.Trace("typeStatusPurchaseNew non valorizzato come 'Annulato'.");
                return;
            }

            Guid updatePurchaseId = new Guid(target.GetAttributeValue<EntityReference>(nameEntityUpdate).Id.ToString());
            if (updatePurchaseId == Guid.Empty)
            {
                trace?.Trace("updatePurchase non e' valorizzato GUID'.");
                return;
            }
            Guid accountPurchaseId = new Guid(target.GetAttributeValue<EntityReference>(Purchase.AccountClientId).Id.ToString());
            if (accountPurchaseId == Guid.Empty)
            {
                trace?.Trace("accountPurchase non e' valorizzato GUID'.");
                return;
            }
            int? typeStatusPurchaseOld = preImage.GetAttributeValue<OptionSetValue>(Purchase.StatusPurchase)?.Value;
            if (typeStatusPurchaseOld == null || typeStatusPurchaseNew == null)
            {
                trace?.Trace("typeStatusPurchaseOld non valorizzato o non presente nella PreImage.");
                throw new InvalidPluginExecutionException("typeStatusPurchaseOld non valorizzato o non presente nella PreImage or PostImage.");
            }

            DateTime? expirationDate = target.GetAttributeValue<DateTime?>(Purchase.ExpirationDate);

            EntityReference assigneeRef = target.Contains(Purchase.Assignee) ? target.GetAttributeValue<EntityReference>(Purchase.Assignee) : null;

            trace.Trace($"All of the following conditions are met");
            _acquistoHelper.CreatePurchaseHistory(service, target, updatePurchaseId, accountPurchaseId, assigneeRef, expirationDate, typeStatusPurchaseOld.Value, typeStatusPurchaseNew.Value, trace);

            // Update lo stato Purchase
            _acquistoHelper.UpdatePurchaseInactived(service, target);
        }
    }
}
 