using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugin.sc_DigitalProduct.CorePlugins;
using Plugin.sc_DigitalProduct.Entities;
using Plugin.sc_DigitalProduct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.sc_DigitalProduct.BusinessLogicPlugins
{
    public class OnPostCreatePurchaseOrderLineProcessDigitalProductPurchaseLogic
    {
        public void ExecuteLogic(IOrganizationService service, Entity entity, ITracingService tracingService)
        {
 
            tracingService?.Trace("PurchaseOrderLine (message Create, Update and Delete) is successfully.");

            ExecutePurchaseOrderLineCreate(service, entity, tracingService); // OrderAcquisto

        }

        public void ExecutePurchaseOrderLineCreate(IOrganizationService service, Entity target, ITracingService trace)
        {
            PurchaseHelper _acquistoHelper = new PurchaseHelper();
            KeyDigitalProductHelper _keyProductHelper = new KeyDigitalProductHelper();
            DigitalProductHelper _prodottoDigitaleHelper = new DigitalProductHelper();
            Entity entityUpdate = new Entity(PurchaseOrderLine.LogicalName);
            entityUpdate.Id = target.Id;
            Guid acquistoIdRetrive = Guid.Empty;
            Guid pdIdRetrive = Guid.Empty;

            if (!target.TryGetAttributeValue(PurchaseOrderLine.DigitalProductId, out EntityReference prodottodigitaleTo))
            {
                trace?.Trace($"ProdottoDigitaleTO is null: {prodottodigitaleTo}");
                return;
            }
            //  get ProdottoDigitaleTO
            Entity getPDToTo = service.Retrieve(prodottodigitaleTo.LogicalName, prodottodigitaleTo.Id, new ColumnSet(true));

            int? typePiattaforma = ((OptionSetValue)getPDToTo.Attributes[DigitalProduct.TypePlatform]).Value;
            // Key Game di PD Based
            List<Entity> keyProductArray = _keyProductHelper.ExistKeyProduct(service, prodottodigitaleTo, 126400000, typePiattaforma); // Disponibile;
            if (keyProductArray.Count == 0)
            {
                trace?.Trace($"Le chiavi non sono disponibile: {keyProductArray.Count}");
                return;
            }
            EntityReference prdPD_Details = getPDToTo.GetAttributeValue<EntityReference>(DigitalProduct.ProductDetails);
            Entity getPrdDetails = service.Retrieve(ProductDetails.LogicalName, prdPD_Details.Id, new ColumnSet(true));

            ////////////////////////////////////////////////
            
            var countryConfig = Utilities.GetDeserializeCountryConfig(service, trace, "CountryConfig");
            if (countryConfig == null) { trace?.Trace($"countryConfig non esiste ");  return; }
            // Se il valore keye_country corisponde a uno di valori CountryConfig, continua il flusso.

            var country = getPrdDetails.GetAttributeValue<EntityReference>(ProductDetails.Country);

            var foundCountryInConfig = countryConfig.FirstOrDefault(c => c.Value.Id == country.Id.ToString());
            trace?.Trace(foundCountryInConfig.Key);
            decimal thresholdCountry = foundCountryInConfig.Value.Threshold;
            trace?.Trace($"Threshold Country %: {thresholdCountry}.");


            var prezzoBase = getPDToTo.GetAttributeValue<Money>(DigitalProduct.BasePrice);
            decimal percentComis = getPrdDetails.GetAttributeValue<decimal>(ProductDetails.CommissionPercentageApp);

            decimal importoIva = 0m;
            decimal importoCommissione = 0m;
            decimal totaleRiga = 0m;

            if (thresholdCountry == 0)
            {
                importoCommissione = prezzoBase.Value * percentComis / 100;
                totaleRiga = prezzoBase.Value + importoCommissione;
            }
            else
            {
                importoIva = prezzoBase.Value * thresholdCountry / 100;
                importoCommissione = prezzoBase.Value * percentComis / 100;
                totaleRiga = prezzoBase.Value + importoIva + importoCommissione;
            }

            trace?.Trace($"importoCommissione: {importoCommissione} \n  totaleRiga : {totaleRiga}.\n ");
            ///////////////////////////////////////////////////

            if (!target.TryGetAttributeValue(PurchaseOrderLine.PurchaseId, out EntityReference acquistoTo))
            {
                trace?.Trace($"acquistoTo is null: {acquistoTo}");

                Guid accountId = getPDToTo.GetAttributeValue<EntityReference>(DigitalProduct.AccountClientId)?.Id ?? Guid.Empty;
                pdIdRetrive = getPDToTo.GetAttributeValue<Guid>(DigitalProduct.DigitalProductId);
                List<Entity> acquistiInattesa = _acquistoHelper.GetAcquistoInAttesa(service, accountId);
                DateTime createdOn = target.GetAttributeValue<DateTime>(PurchaseOrderLine.CreatedOn);
                if (acquistiInattesa.Count > 0)
                {
                    Guid acquistoId = acquistiInattesa[0].GetAttributeValue<Guid>(Purchase.PurchaseId);
                    acquistoIdRetrive = acquistoId;
                }
                else
                {
                    int quantitaAcquisto = _acquistoHelper.GetAcquisto(service, target).Entities.Count + 1;

                    Guid acquistoId = _acquistoHelper.CreateAcquisto(service, quantitaAcquisto, accountId, 126400001, GeneraCodiceAcquisto(), createdOn, totaleRiga);
                    if (acquistoId == Guid.Empty) { throw new InvalidPluginExecutionException($"Errore durante la creazione dell'Acquisto."); }
                    acquistoIdRetrive = acquistoId;

                    trace?.Trace($"Nuovo Acquisto creato: {acquistoTo}");
                }

                entityUpdate[PurchaseOrderLine.PurchaseId] = new EntityReference(Purchase.LogicalName, acquistoIdRetrive);

            }
            trace?.Trace($"AssignTo {acquistoTo}");

            int? tipoExpansion = getPrdDetails.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion)?.Value;

            if (tipoExpansion == 126400003) //Espansione
            {
                var contentVideoGames = _prodottoDigitaleHelper.GeVideoGameWithEspansionDisponib(service, prodottodigitaleTo.Id);  // 746200003 -> Disponibile content
                if (contentVideoGames == null || contentVideoGames.Count <= 0)
                {
                    return;
                }
                foreach (var content in contentVideoGames)
                {
                    Entity getPrdDetailsContent = service.Retrieve(ProductDetails.LogicalName, content.GetAttributeValue<EntityReference>(DigitalProduct.ProductDetails).Id, new ColumnSet(true));
                    var valueExpansionChild = getPrdDetailsContent.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion);
                    if (valueExpansionChild.Value == 126400001) // DLC
                    {
                        var contentVideoGameGuid = content.GetAttributeValue<Guid>(DigitalProduct.DigitalProductId);
                        //if (contentVideoGameGuid == Guid.Empty) { continue; }

                        var arrayKeyGamesContent = _keyProductHelper.ExistKeyProduct(service, new EntityReference(DigitalProduct.LogicalName, contentVideoGameGuid), 126400000, typePiattaforma); // Disponibile;
                        if (arrayKeyGamesContent == null || arrayKeyGamesContent.Count == 0) { continue; }

                        Entity nuovoOrderAcquistoEspansione = new Entity(PurchaseOrderLineExpansion.LogicalName);
                        nuovoOrderAcquistoEspansione[PurchaseOrderLineExpansion.PurchaseOrderLineExpansionName] = "Name_" + arrayKeyGamesContent.Count + 1 + "_" + arrayKeyGamesContent[0].GetAttributeValue<string>(KeyDigitalProduct.KeyDigitalProductName);
                        nuovoOrderAcquistoEspansione[PurchaseOrderLineExpansion.KeyDigitalProduct] = arrayKeyGamesContent[0].GetAttributeValue<string>(KeyDigitalProduct.KeyDigitalProductName);
                        nuovoOrderAcquistoEspansione[PurchaseOrderLineExpansion.PurchaseOrderLineId] = new EntityReference(PurchaseOrderLine.LogicalName, target.Id);
                        nuovoOrderAcquistoEspansione[PurchaseOrderLineExpansion.NameContentDigitalProduct] = content.GetAttributeValue<string>(DigitalProduct.Key);
                        service.Create(nuovoOrderAcquistoEspansione);

                        _keyProductHelper.UpdateKeyProduct(service, arrayKeyGamesContent[0].Id, 126400004);// Temporaneamente 

                    }
                }
            }

            _keyProductHelper.UpdateKeyProduct(service, keyProductArray[0].Id, 126400004);// Temporaneamente 

            entityUpdate[PurchaseOrderLine.KeyDigitalProduct] = keyProductArray[0].GetAttributeValue<string>(KeyDigitalProduct.KeyDigitalProductName);// Padre key
            
            entityUpdate[PurchaseOrderLine.SellingPrice] = new Money(prezzoBase.Value);
            entityUpdate[PurchaseOrderLine.AmountIVA] = new Money(importoIva);
            entityUpdate[PurchaseOrderLine.CommissionAmount] = new Money(importoCommissione);
            entityUpdate[PurchaseOrderLine.TotalRow] = new Money(totaleRiga);
            
            service.Update(entityUpdate);
        }

        private string GeneraCodiceAcquisto(int lunghezza = 6)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return "ACQ-" + new string(Enumerable.Repeat(chars, 6)
                                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

