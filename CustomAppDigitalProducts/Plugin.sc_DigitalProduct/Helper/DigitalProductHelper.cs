using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugin.sc_DigitalProduct.CorePlugins;
using Plugin.sc_DigitalProduct.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.Helper
{
    public class DigitalProductHelper
    {
        public DigitalProductHelper() { }

        public List<Entity> GeNamesProdottiDigitaleActivedTypeExpansionNull(IOrganizationService service, string nameToNoTypeExpansion, int? typePD)
        {

            QueryExpression query = new QueryExpression(DigitalProduct.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition(DigitalProduct.Name, ConditionOperator.Equal, nameToNoTypeExpansion);
            query.Criteria.AddCondition(DigitalProduct.TypeDigitalProduct, ConditionOperator.Equal, typePD);
            query.Criteria.AddCondition(DigitalProduct.StateCode, ConditionOperator.Equal, 0); // Active
            query.NoLock = true;

            LinkEntity productDetailsLink = query.AddLink(
                ProductDetails.LogicalName,
                DigitalProduct.ProductDetails,       // lookup su Digital Product verso ProductDetails
                ProductDetails.ProdottoDigitaleId,     // primary key di ProductDetails
                JoinOperator.Inner);

            productDetailsLink.LinkCriteria.AddCondition(ProductDetails.TypeExpansion, ConditionOperator.Null);

            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }

            return result.Entities.ToList();
        }

        public List<Entity> GeVideoGameWithEspansionDisponib(IOrganizationService service, Guid prdID)
        {

            QueryExpression query = new QueryExpression(DigitalProduct.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(DigitalProduct.ParentDigitalProductId, ConditionOperator.Equal, prdID);
            query.Criteria.AddCondition(DigitalProduct.StatoDigitalProduct, ConditionOperator.Equal, 126400000); // Disponibile
            query.NoLock = true;
            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                return new List<Entity>();
            }
            return result.Entities.ToList();
        }


        public List<Entity> GeDigitalProductActived(IOrganizationService service, Entity target)
        {

            QueryExpression query = new QueryExpression(DigitalProduct.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(DigitalProduct.ProductDetails, ConditionOperator.Equal, target.Id);
            query.Criteria.AddCondition(DigitalProduct.StateCode, ConditionOperator.Equal, 0);//Active
            query.NoLock = true;
            query.TopCount = 1;

            return service.RetrieveMultiple(query).Entities.ToList();
        }

        public EntityCollection GetDigitalProductWitchTypePlatformAndTypePD(IOrganizationService service,Entity postImage,string nameToProductDigit)
        {
            QueryExpression query = new QueryExpression(DigitalProduct.LogicalName)
            {
                ColumnSet = new ColumnSet(DigitalProduct.DigitalProductId,DigitalProduct.Name, DigitalProduct.TypePlatform),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(DigitalProduct.Name, ConditionOperator.Equal, nameToProductDigit);
            query.Criteria.AddCondition(DigitalProduct.ProductDetails, ConditionOperator.NotEqual, postImage.Id);
            query.Criteria.AddCondition(DigitalProduct.StateCode, ConditionOperator.Equal, 0); // Active

            query.NoLock = true;

           LinkEntity productDetailsLink = query.AddLink(
                ProductDetails.LogicalName,
                DigitalProduct.ProductDetails,       // lookup su Digital Product verso ProductDetails
                ProductDetails.ProdottoDigitaleId,     // primary key di ProductDetails
                JoinOperator.Inner);

            productDetailsLink.EntityAlias = "pd";
            productDetailsLink.Columns = new ColumnSet(ProductDetails.TypeExpansion,ProductDetails.TypeLicense);
            EntityCollection result = service.RetrieveMultiple(query);
            return result;
        }

        public void UpdateKeyProdottoDigitale(IOrganizationService service, Guid keyProdottoDigitale, int typePiattaforma)
        {
            Entity entityUpdatePD = new Entity(DigitalProduct.LogicalName);
            entityUpdatePD.Id = keyProdottoDigitale;
            entityUpdatePD[DigitalProduct.TypePlatform] = new OptionSetValue(typePiattaforma);
            service.Update(entityUpdatePD);
        }

        public void UpdateRemoveValueDigitalProduct(IOrganizationService service, Guid idProdDigital, EntityReference parentDigitProd, int? typeExpansion)
        {
            if (typeExpansion != 126400001) // ! DLC
            {
                if (parentDigitProd != null)
                {
                    Entity updateDigitalProd = new Entity(DigitalProduct.LogicalName)
                    {
                        Id = idProdDigital
                    };
                    updateDigitalProd[DigitalProduct.ParentDigitalProductId] = null;
                    service.Update(updateDigitalProd);
                }
            }
        }

        public void UpdateNameCodiceProdottoDigitale(IOrganizationService service, Guid idProdDigital, string nameTo, int? value, int? typeProdDigit)
        {
            string nameSave = string.Empty;
            Entity updateProdottoDigitale = new Entity(DigitalProduct.LogicalName)
            {
                Id = idProdDigital
            };

            if (typeProdDigit == 126400000) // Video Game
            {
                nameSave = GetNameVerifyTypeExpansion(value);
            }
            if (typeProdDigit == 126400001) // Licenza Software
            {
                nameSave = GetNameVerifyTypeLicenzeSfotware(value);
            }
            updateProdottoDigitale.Attributes[DigitalProduct.Name] = $"{nameTo}";
            updateProdottoDigitale.Attributes[DigitalProduct.Codice] = $"{Utilities.GeneraCodice()}" + " - " + $"{nameSave}";
            
            service.Update(updateProdottoDigitale);
        }
        public string GetNameVerifyTypeExpansion(int? value)
        {
            string name = string.Empty;

            if (value == 126400000) // Base Game
            {
                name = "Base Game";
            }
            if (value == 126400001) // DLC
            {
                name = "DLC";
            }
            if (value == 126400002) // Remastered
            {
                name = "Remastered";
            }
            if (value == 126400003) // Espansione
            {
                name = "Espansione";
            }
            return name;
        }

        public string GetNameVerifyTypeLicenzeSfotware(int? value)
        {
            string name = string.Empty;

            if (value == 126400000) // Perpetua
            {
                name = "Perpetua";
            }
            if (value == 126400001) // Mensile
            {
                name = "Mensile";
            }
            if (value == 126400002) // Annuale
            {
                name = "Annuale";
            }
            if (value == 126400003) // Trial
            {
                name = "Trial";
            }
            if (value == 126400004) // Lifetime
            {
                name = "Lifetime";
            }
            if (value == 126400005) // Enterprise
            {
                name = "Enterprise";
            }
            return name;
        }
    }
}
