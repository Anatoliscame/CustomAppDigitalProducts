using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugin.sc_DigitalProduct.CorePlugins;
using Plugin.sc_DigitalProduct.Entities;
using Plugin.sc_DigitalProduct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.BusinessLogicPlugins
{
    public class OnCreateUpdateDigitalProductLogic
    {
        public void ExecuteLogic(IOrganizationService service, Entity prodottoDigitale, string messageName, ITracingService trace)
        {
            trace?.Trace("Prodotto Digitale (message Create, Update and Delete) is successfully.");
            if (prodottoDigitale != null)
            {
                ExecuteProdottoDigitale(service, prodottoDigitale, messageName, trace);
            }
            else
            {
                trace?.Trace("Product Digitale is null");
                return;
            }
        }

        public void ExecuteProdottoDigitale(IOrganizationService service, Entity prodottoDigitale, string messageName, ITracingService trace)
        {
            DigitalProductHelper _prodottoDigitaleHelper = new DigitalProductHelper();
            ProductDetailsHelper _productDetailsHelper = new ProductDetailsHelper();

            int? typePD = prodottoDigitale.Contains(DigitalProduct.TypeDigitalProduct)
                ? prodottoDigitale.GetAttributeValue<OptionSetValue>(DigitalProduct.TypeDigitalProduct)?.Value
                : null;

            if (messageName.ToLower() == "create")
            {
                List<string> prodottoDigitaleArray = null;
                string arraynomiProdDig = "";
                /*
                if (!prodottoDigitale.TryGetAttributeValue(ProdottoDigitale.Codice, out string codiceTo) || codiceTo.Trim() == "")
                {
                    trace?.Trace("Codice di Prodotto Digitale: il campo è assente o il valore è nullo.");
                    throw new InvalidPluginExecutionException("Codice di Prodotto Digitale: il campo è assente o il valore è nullo.");
                }*/
                var codiceTo = prodottoDigitale.GetAttributeValue<string>(DigitalProduct.Codice);

                if (!prodottoDigitale.TryGetAttributeValue(DigitalProduct.Name, out string nameTo))
                {
                    trace?.Trace("Name di Prodotto Digitale: il campo è assente.");
                    throw new InvalidPluginExecutionException("Name di Prodotto Digitale: il campo è assente.");
                }

                string nameToNoTypeExpansion = $"{nameTo}" + " - " + "Specifica tipo di prodotto digitale";

                int? typePiattaformaPD = prodottoDigitale.Contains(DigitalProduct.TypePlatform)
                    ? prodottoDigitale.GetAttributeValue<OptionSetValue>(DigitalProduct.TypePlatform)?.Value
                     : null;
                int? statoePD = prodottoDigitale.Contains(DigitalProduct.StatoDigitalProduct)
                     ? prodottoDigitale.GetAttributeValue<OptionSetValue>(DigitalProduct.StatoDigitalProduct)?.Value
                     : null;

                if (typePD == null || typePiattaformaPD == null) { throw new InvalidPluginExecutionException("Tipo di Prodotto Digitale o Tipo di Piattaforma non e' stato compilato. per favore inserisce il valore ."); }
                if (statoePD == null) { throw new InvalidPluginExecutionException("Stato di Prodotto Digitale non e' stato compilato. per favore inserisce il valore ."); }

                if (typePD == 126400000)
                {
                    // Video Game
                    arraynomiProdDig = Utilities.GetNameCodiceForProdottoDigitalePrivateConfig(service, "NamesForProdottoDigitaleVideoGame");

                }
                else if (typePD == 126400001)
                {
                    // Software
                    arraynomiProdDig = Utilities.GetNameCodiceForProdottoDigitalePrivateConfig(service, "NamesForProdottoDigitaleLicenzaSoftware");
                }

                if (string.IsNullOrWhiteSpace(arraynomiProdDig)) //arraynomiProdDig.Trim() == ""
                {
                    trace?.Trace("Private Configuration vuota o non trovata per key: NameCodiceForProdottoDigitale.");
                    throw new InvalidPluginExecutionException("Configurazione mancante: NameCodiceForProdottoDigitale.");
                }

                List<Entity> prodottiDigitaleAll = _prodottoDigitaleHelper.GeNamesProdottiDigitaleActivedTypeExpansionNull(service, nameToNoTypeExpansion, typePD);
                if (prodottiDigitaleAll.Count > 0) { throw new InvalidPluginExecutionException("Esiste il prodotto digitale con stesso Nome con type expansion null."); }

                prodottoDigitaleArray = Utilities.GetPrivateConfigurationValueSplit(arraynomiProdDig);
                if (prodottoDigitaleArray.Count == 0)
                {
                    trace?.Trace("prodottoDigitaleArray : il campo non è valorizzato.");
                    throw new InvalidPluginExecutionException("prodottoDigitaleArray : il campo non è valorizzato.");
                }

                bool nameTrovato = IsNameValid(nameTo, prodottoDigitaleArray);

                if (!nameTrovato)
                {
                    trace?.Trace($"Name di Prodotto Digitale non valido e non corrisponde a PrivateConfiguration: {nameTo}");
                    throw new InvalidPluginExecutionException($"Il nome '{nameTo}' non è presente nella Private Configuration.");
                }

                // Creazione ProductDigital

                Guid prodDigitID = _productDetailsHelper.CreateProductDetails(service, nameTo, typePD.Value);
                if (prodDigitID == Guid.Empty) { throw new InvalidPluginExecutionException($"Errore durante la creazione dell'prodDigitID."); }

                prodottoDigitale[DigitalProduct.ProductDetails] = new EntityReference(ProductDetails.LogicalName, prodDigitID);

                prodottoDigitale[DigitalProduct.BasePrice] = new Money(0); ;
                prodottoDigitale[DigitalProduct.Name] = nameToNoTypeExpansion;
                prodottoDigitale[DigitalProduct.Codice] = $"{nameTo}" + " - " + Utilities.GeneraCodice();

            }
            if (messageName.ToLower() == "update")
            {

                if (typePD == 126400000) // Video Game
                {

                    var entityDigDetailsParentTo = service.Retrieve(ProductDetails.LogicalName, prodottoDigitale.GetAttributeValue<EntityReference>(DigitalProduct.ProductDetails).Id, new ColumnSet(true));
                    var typeExpansionParent = entityDigDetailsParentTo.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion)?.Value;
                    var parentDigitProdLookupTo = prodottoDigitale.GetAttributeValue<EntityReference>(DigitalProduct.ParentDigitalProductId);

                    if (typeExpansionParent == 126400001) // DLC
                    {

                        if (parentDigitProdLookupTo == null)
                        {
                            trace?.Trace($"Non e' possibile verificare il prodotto digitale padre."); return;
                        }

                        prodottoDigitale[DigitalProduct.BasePrice] = new Money(0);

                        var padreDigitProdTo = service.Retrieve(DigitalProduct.LogicalName, parentDigitProdLookupTo.Id, new ColumnSet(true));
                        var entityDigDetailsTo = service.Retrieve(ProductDetails.LogicalName, padreDigitProdTo.GetAttributeValue<EntityReference>(DigitalProduct.ProductDetails).Id, new ColumnSet(true));

                        var typeExpansionPadre = entityDigDetailsTo.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion)?.Value;
                        if (typeExpansionPadre != 126400003) // Espansione
                        {
                            throw new InvalidPluginExecutionException("DLC deve essere solo associato al prodotto gigitale di tipo Espansione");
                        }
                    }
                    else if (typeExpansionParent != 126400001) // ! DLC
                    {
                        Money prezzoBaseMoney = prodottoDigitale.GetAttributeValue<Money>(DigitalProduct.BasePrice);

                        if (prezzoBaseMoney == null || prezzoBaseMoney.Value <= 0)
                        {
                            throw new InvalidPluginExecutionException("Prezzo base del Prodotto Digitale non valorizzato.");
                        }

                    }
                }
            }
        }

        public bool IsNameValid(string nameTo, List<string> prodottoDigitaleArray)
        {
            foreach (var prodottoDigitaleName in prodottoDigitaleArray)
            {
                if (!string.IsNullOrEmpty(prodottoDigitaleName))
                {
                    if (prodottoDigitaleName.Equals(nameTo))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
