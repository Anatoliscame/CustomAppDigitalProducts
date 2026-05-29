using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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

            DateTime? expirationDate = target.GetAttributeValue<DateTime?>(Purchase.ExpirationDate);

            EntityReference assigneeRef = target.Contains(Purchase.Assignee) ? target.GetAttributeValue<EntityReference>(Purchase.Assignee) : null;

            trace.Trace($"All of the following conditions are met");
            _acquistoHelper.CreatePurchaseHistory(service, target, updatePurchaseId, accountPurchaseId, assigneeRef, expirationDate, typeStatusPurchaseOld.Value, typeStatusPurchaseNew.Value, trace);

            ExecuteOrderAcquistoDelete(service, preImage, trace);
            // Update lo stato Purchase
            _acquistoHelper.UpdatePurchaseInactived(service, target);
        }

        public void ExecuteOrderAcquistoDelete(IOrganizationService service, Entity preImage, ITracingService trace)
        {
            PurchaseOrderlineHelper _oderAcquistoHelper = new PurchaseOrderlineHelper();
            KeyDigitalProductHelper _keyProductHelper = new KeyDigitalProductHelper();
            DigitalProductHelper _prodottoDigitaleHelper = new DigitalProductHelper();


            var arrayOrderAcquisto = _oderAcquistoHelper.GetOrderAcquisto(service, preImage);
            if (arrayOrderAcquisto.Count <= 0) { trace?.Trace($"arrayOrderAcquisto e' vuoto: {arrayOrderAcquisto.Count} "); return; }

            Guid keyProductGuid = Guid.Empty;
            int deletedCount = 0;
            trace?.Trace($"Recupero la lista di OrdineAcquisto: {arrayOrderAcquisto.Count} ");

            foreach (var crmOrderAcquisto in arrayOrderAcquisto)
            {
                if (crmOrderAcquisto == null) continue;
                var keyPDId = crmOrderAcquisto.GetAttributeValue<AliasedValue>($"PurchaseOrderLineKeyProduct.{KeyDigitalProduct.KeyDigitalProductId}");
                if (keyPDId == null) continue;
                trace?.Trace($"Viene ciclato Ogni OrdineAcquisto");

                // Prodotto Digitale
                var pdId = crmOrderAcquisto.GetAttributeValue<EntityReference>(PurchaseOrderLine.DigitalProductId);
                Entity getProdDigitTo = service.Retrieve(DigitalProduct.LogicalName, pdId.Id, new ColumnSet(true));
                trace?.Trace($"Prodotto Digitale di OrdineAcquisto recuperato: {getProdDigitTo.Id.ToString()}");
                int? typePiattaforma = ((OptionSetValue)getProdDigitTo.Attributes[DigitalProduct.TypePlatform]).Value;

                // Prodottto Details
                var prodDetailsId = getProdDigitTo.GetAttributeValue<EntityReference>(DigitalProduct.ProductDetails);
                Entity getProdDetailsTo = service.Retrieve(ProductDetails.LogicalName, prodDetailsId.Id, new ColumnSet(true));

                int? typeProdDigValue = getProdDigitTo.GetAttributeValue<OptionSetValue>(DigitalProduct.TypeDigitalProduct)?.Value;
                switch (typeProdDigValue)
                {
                    case 126400000:// VideoGame
                        int? typeexpansion = getProdDetailsTo.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion)?.Value;
                        trace?.Trace($"Tipo Video Game di Prodottto Details: {typeexpansion.Value} \n Tipo Piattaforma di Prodotto Digitale: {typePiattaforma.Value}");

                        if (typeexpansion.Value == 126400003)//Espansione
                        {
                            trace?.Trace($"Hai scelto VideoGame di tipo Espansione");
                            var contentVideoGames = _prodottoDigitaleHelper.GeVideoGameWithEspansionDisponib(service, getProdDigitTo.Id);  // 746200003 -> Disponibile content
                            if (contentVideoGames == null || contentVideoGames.Count <= 0)
                            {
                                return;
                            }
                            trace?.Trace($"Recupero di tutti contenuti di videogame base: N -> {contentVideoGames.Count}");
                            foreach (var content in contentVideoGames)
                            {
                                //trace?.Trace($"");
                                var videoGameId = content.GetAttributeValue<Guid>(DigitalProduct.DigitalProductId);
                                if (videoGameId == Guid.Empty)
                                { //continue;
                                    throw new InvalidPluginExecutionException($"Non esiste videgame: {videoGameId.ToString()} ");
                                }
                                trace?.Trace($"Il contenuto di VideoGame: {videoGameId}");
                                // Da risolvere -->
                                var arrayKeyGamesContent = _keyProductHelper.ExistKeyProduct(service, new EntityReference(DigitalProduct.LogicalName, videoGameId), 126400004, typePiattaforma); // Temporaneamente;
                                trace?.Trace($"Un elenco di KeyGame (chiavi di contenuti disponibili): {arrayKeyGamesContent.Count}");
                                if (arrayKeyGamesContent.Count == 0)
                                { //continue;
                                    throw new InvalidPluginExecutionException($"La lista di KeyGamesContent: {arrayKeyGamesContent.Count} \n Mentre DLC esiste {videoGameId}, e il numero di DLC sono: {contentVideoGames.Count} ");
                                }

                                _keyProductHelper.UpdateKeyProduct(service, arrayKeyGamesContent[0].Id, 126400000);// Disponibile 
                            }

                            var crmOrderAcquistoEsp = crmOrderAcquisto.GetAttributeValue<Guid>(PurchaseOrderLine.PurchaseOrderlineId);
                            List<Entity> orderAcquistoEspansions = _oderAcquistoHelper.GetOrderAcquistoEspansion(service, crmOrderAcquistoEsp);
                            int deletedCountEspnsion = 0;
                            foreach (var orderAcquistoEspansion in orderAcquistoEspansions)
                            {
                                if (orderAcquistoEspansion == null) continue; // Mmm, dovrei toglierlo

                                service.Delete(PurchaseOrderLineExpansion.LogicalName, orderAcquistoEspansion.Id);
                                deletedCountEspnsion++;
                            }
                            trace?.Trace($"Numero di OrdineAcquisto di Espansione rimossi + {deletedCountEspnsion}");
                        }
                        break;

                    case 126400001:// Licenza Software

                        break;
                    default:
                        break;
                }
                keyProductGuid = (Guid)keyPDId.Value;

                _keyProductHelper.UpdateKeyProduct(service, keyProductGuid, 126400000); // Disponibile
                trace?.Trace($"Effetuata un UPDATE di stato acn_keygame genitore");
                service.Delete(PurchaseOrderLine.LogicalName, crmOrderAcquisto.Id);
                deletedCount++;
            }
            trace.Trace($"{deletedCount} OrderAcquisto eliminati e KeyGame aggiornati.");
            return;
        }
    }
}
 