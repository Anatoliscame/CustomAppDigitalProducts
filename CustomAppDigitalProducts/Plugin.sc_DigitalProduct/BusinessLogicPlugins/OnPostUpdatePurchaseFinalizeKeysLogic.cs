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
    public class OnPostUpdatePurchaseFinalizeKeysLogic
    {
        public void ExecuteLogic(IOrganizationService service, Entity targetPost, ITracingService trace)
        {
            trace?.Trace("Purchase (message Create, Update and Delete) is successfully.");
            if (targetPost != null)
            {
                int? optionSetValue = ((OptionSetValue)targetPost.Attributes[Purchase.StatusPurchase]).Value;

                switch (optionSetValue)
                {
                    case 126400000: //Effetuato                               
                        ExecuteAcquistoUpdate(service, targetPost, trace);
                        break;
                    case 126400001: //In_attesa
                        throw new ApplicationException("il valore di stato ordine selezionato 'Completato', acquisto non puo essere eliminato");

                    case 126400002: //Annulato
                        throw new ApplicationException("'Annulato' non consentito");

                    default:
                        return;
                }
            }
            else
            {
                trace?.Trace("PostEntityImage non trovata, niente da processare.");
                return;
            }
        }

        public void ExecuteAcquistoUpdate(IOrganizationService service, Entity targetPost, ITracingService trace)
        {
            PurchaseOrderlineHelper _oderAcquistoHelper = new PurchaseOrderlineHelper();
            KeyDigitalProductHelper _keyProductHelper = new KeyDigitalProductHelper();

            var arrayOrderAcquisto = _oderAcquistoHelper.GetOrderAcquisto(service, targetPost);
            if (arrayOrderAcquisto.Count <= 0) { return; }


            Guid keyPDIdGuid = Guid.Empty;

            foreach (var crmOrderAcquisto in arrayOrderAcquisto)
            {
                var keyPDId = crmOrderAcquisto.GetAttributeValue<AliasedValue>($"PurchaseOrderLineKeyProduct.{KeyDigitalProduct.KeyDigitalProductId}");
                keyPDIdGuid = (Guid)keyPDId.Value;

                _keyProductHelper.UpdateKeyProduct(service, keyPDIdGuid, 126400005); // Temporaneamente Acquistato
            }
            trace.Trace($"Acquisto has been updated");
            return;
        }
    }
}

