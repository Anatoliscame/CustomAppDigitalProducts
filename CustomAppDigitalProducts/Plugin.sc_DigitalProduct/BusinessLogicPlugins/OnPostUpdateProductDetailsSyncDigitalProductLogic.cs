using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Plugin.sc_DigitalProduct.CorePlugins;
using Plugin.sc_DigitalProduct.Entities;
using Plugin.sc_DigitalProduct.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Plugin.sc_DigitalProduct.BusinessLogicPlugins
{
    public class OnPostUpdateProductDetailsSyncDigitalProductLogic
    {
        public void ExecuteLogic(IOrganizationService service, Entity postImage, ITracingService trace)
        {
            trace?.Trace("Product Details (message Create, Update and Delete) is successfully.");
            if (postImage != null)
            {
                ExecuteProductDetails(service, postImage, trace);
            }
            else
            {
                trace?.Trace("Product Details is null");
                return;
            }
        }

        public void ExecuteProductDetails(IOrganizationService service, Entity postImage, ITracingService trace)
        {
            DigitalProductHelper _prodottoDigitaleHelper = new DigitalProductHelper();
            ProductDetailsHelper _productDetailsHelper = new ProductDetailsHelper();

            List<Entity> prodottiDigitale = _prodottoDigitaleHelper.GeDigitalProductActived(service, postImage);
            if (prodottiDigitale.Count == 0)
            {
                throw new InvalidPluginExecutionException("Non esiste un prodotto digitale attivo relazionato con Product Details.");
            }
             

            Guid idProdDigital = prodottiDigitale[0].GetAttributeValue<Guid>(DigitalProduct.DigitalProductId);

            string nameTo = GetNameBeforeDash(prodottiDigitale[0].GetAttributeValue<string>(DigitalProduct.Name));

            decimal percCommissione = postImage.GetAttributeValue<decimal>(ProductDetails.CommissionPercentageApp);
            if (percCommissione <= 0)
            {
                throw new InvalidPluginExecutionException("percCommissione non e' valorizzato, inserisci un valore.");
            }

            int? secondTypePlatform = prodottiDigitale[0].Contains(DigitalProduct.TypePlatform)
                ? prodottiDigitale[0].GetAttributeValue<OptionSetValue>(DigitalProduct.TypePlatform)?.Value
                : null;

            EntityCollection digitProdFiltr = null;

            int ? typeproductdetail = postImage.GetAttributeValue<OptionSetValue>(ProductDetails.TypeDigitalProduct)?.Value;
            if (typeproductdetail == 126400000) //VideoGame
            {
                trace?.Trace($"Valore di Tipo di Prodotto Digitale e' recuperato 'Video Game': {typeproductdetail.Value}");

                int? typeexpansion = postImage.Contains(ProductDetails.TypeExpansion)
                    ? postImage.GetAttributeValue<OptionSetValue>(ProductDetails.TypeExpansion)?.Value
                    : null;

                if (typeexpansion == null)
                {
                    trace?.Trace("sc_typeexpansion non valorizzato o non presente nella PostImage.");
                    throw new InvalidPluginExecutionException("Il campo Type Expansion è obbligatorio. Seleziona un valore prima di salvare.");
                }

                digitProdFiltr = _prodottoDigitaleHelper.GetDigitalProductWitchTypePlatformAndTypePD(service, postImage, nameTo);

                bool isDuplicateDigitProdVG = VerifyDigitalProductDetails(digitProdFiltr, secondTypePlatform.Value, typeexpansion.Value, ProductDetails.TypeExpansion);

                if (isDuplicateDigitProdVG)
                {
                    throw new InvalidPluginExecutionException("Non puoi creare un prodotto digitale con la stessa combinazione di Type Expansion e Type Platform selezionata.");
                }

                EntityReference parentDigitProd = prodottiDigitale[0].GetAttributeValue<EntityReference>(DigitalProduct.ParentDigitalProductId);
               
                /////// NON E' DA CONTROLLARE QUESTO FUNCTION
                _prodottoDigitaleHelper.UpdateRemoveValueDigitalProduct(service, idProdDigital, parentDigitProd, typeexpansion);
                ///////
                 _productDetailsHelper.UpdateNameProductDetails(service, postImage, nameTo);

                _prodottoDigitaleHelper.UpdateNameCodiceProdottoDigitale(service, idProdDigital, nameTo, typeexpansion, typeproductdetail);
            }
            if (typeproductdetail == 126400001) //Licenza Software
            {
                trace?.Trace($"Valore di Tipo di Prodotto Digitale e' recuperato 'Licenza Software': {typeproductdetail.Value}");

                int? tipoLicenza = postImage.Contains(ProductDetails.TypeLicense)
                    ? postImage.GetAttributeValue<OptionSetValue>(ProductDetails.TypeLicense)?.Value
                    : null;

                if (tipoLicenza == null)
                {
                    trace?.Trace("tipoLicenza non valorizzato o non presente nella PostImage.");
                    throw new InvalidPluginExecutionException("Il campo Tipo di Licenza Software è obbligatorio. Seleziona un valore prima di salvare.");
                }

                digitProdFiltr = _prodottoDigitaleHelper.GetDigitalProductWitchTypePlatformAndTypePD(service, postImage, nameTo);
                bool isDuplicateDigitProdLS = VerifyDigitalProductDetails(digitProdFiltr, secondTypePlatform.Value, tipoLicenza.Value, ProductDetails.TypeLicense);

                if (isDuplicateDigitProdLS)
                {
                    throw new InvalidPluginExecutionException("Non puoi creare un prodotto digitale con la stessa combinazione di Type License Software e Type Platform selezionata.");
                }

                _productDetailsHelper.UpdateNameProductDetails(service, postImage, nameTo);

                _prodottoDigitaleHelper.UpdateNameCodiceProdottoDigitale(service, idProdDigital, nameTo, tipoLicenza, typeproductdetail);
            }

            var countryLookup = postImage.GetAttributeValue<EntityReference>(ProductDetails.Country);
            if (countryLookup == null)
            {
                trace?.Trace("Country non valorizzato.");
                return;
                //throw new InvalidPluginExecutionException("Country non valorizzato.");
            }

            //-----------------TEMPORANEO, DA BLOCCARE COUNTRY NON CORRISPONDENTE SU PRIVE CONFIG---------------------//

            VerifyCountryProductDetailsWitchPriveConfig(service, countryLookup.Id, trace);


            ///--------------------------------------------------------------------------///
        }

        public bool VerifyDigitalProductDetails(EntityCollection digitProdFiltr, int secondTypePlatform, int type, string nameTypeProductDig)
        {
            var isDuplicateDigitProd = false;

            for (int i = 0; i < digitProdFiltr.Entities.Count; i++)
            {

                var aliasedType = digitProdFiltr.Entities[i].GetAttributeValue<AliasedValue>($"pd.{nameTypeProductDig}");
                var optionSetType = aliasedType?.Value as OptionSetValue;

                int? firstTypePlatform = digitProdFiltr.Entities[i].Contains(DigitalProduct.TypePlatform)
                    ? digitProdFiltr.Entities[i].GetAttributeValue<OptionSetValue>(DigitalProduct.TypePlatform)?.Value
                    : null;

                if (firstTypePlatform == secondTypePlatform
                    && optionSetType?.Value == type) 
                {
                    isDuplicateDigitProd = true;
                    break;
                }
            }
            return isDuplicateDigitProd;
        }

        private string GetNameBeforeDash(string nameTo)
        {
            string[] nameParts = nameTo.Split('-');

            if (nameParts.Length > 0)
            {
                nameTo = nameParts[0].Trim();
            }

            return nameTo;
        }
        public void VerifyCountryProductDetailsWitchPriveConfig(IOrganizationService service, Guid countrid, ITracingService trace)
        {
            var countryConfig = Utilities.GetDeserializeCountryConfig(service, trace, "CountryConfig");
            if (countryConfig == null) { return; }

            bool isCountry = Utilities.CheckCountryPrivateConfig(service, countryConfig, countrid);
            if (!isCountry)
            {
                trace?.Trace($"Il paese del prodotto digitale non è tra quelli autorizzati per generare l’approvazione.");
                throw new InvalidPluginExecutionException("Il paese del prodotto digitale non è tra quelli autorizzati per generare l’approvazione.");
            }
        }
    }
}

