using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Plugin.sc_DigitalProduct.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.CorePlugins
{
    public static class Utilities
    {
        public static Dictionary<string, CountryInfo> GetDeserializeCountryConfig(IOrganizationService service, ITracingService trace, string key)
        {
            try
            {
                var priveConfig = GetPrivateConfig(service, key);
                if (priveConfig == null) { trace?.Trace($"PriveConfig non trovata."); return null; }
                string description = priveConfig.GetAttributeValue<string>("sc_description");

                // Verifica il valore di acn_description, se esiste o no
                if (string.IsNullOrWhiteSpace(description))
                {
                    trace?.Trace($"Configurazione con chiave '{key}' non trovata o vuota.");
                    return null;
                }
                // Deserializza Json in oggetto
                return JsonConvert.DeserializeObject<CountryConfig>(description)?.Countries;
            }
            catch (Exception ex)
            {
                trace?.Trace("Formato JSON non valido in acn_description: " + ex.Message);
                return null;
            }
        }
        public static Entity GetPrivateConfig(IOrganizationService service, string key)
        {
            QueryExpression queryMessage = new QueryExpression();
            queryMessage.EntityName = "sc_privateconfiguration";
            queryMessage.ColumnSet.AddColumns("sc_description", "sc_value");
            queryMessage.Criteria.AddCondition("sc_key", ConditionOperator.Equal, key);

            var resultQuery = service.RetrieveMultiple(queryMessage);

            if (resultQuery.Entities.Count == 0)
            {
                return null;
            }

            return resultQuery.Entities[0];
        }

        public static bool CheckCountryPrivateConfig(IOrganizationService service, Dictionary<string, CountryInfo> countryConfig, Guid countryTo)
        {
            var foundCountryInConfig = countryConfig.FirstOrDefault(c => c.Value.Id == countryTo.ToString());

            if (foundCountryInConfig.Key != null)
            {
                return true;
            }
            return false;
        }

        public static Entity MergeEntities(Entity preImage, Entity target)
        {
            foreach (var attr in preImage.Attributes)
            {
                if (!target.Contains(attr.Key))
                {
                    target[attr.Key] = attr.Value;
                }
            }
            return target;
        }

        public static string GeneraCodice(int lunghezza = 6)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return "SCAME_CODICE-" + new string(Enumerable.Repeat(chars, 6)
                                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static List<string> GetPrivateConfigurationValueSplit(string priveConfigTo)
        {
            List<string> priveConfigTo_Array = priveConfigTo.Split(',')
               .Select(u => u.Trim())//per rimuovere gli spazi vuoti sia all'inizio e alla fine
               .Select(u => u.Trim('\'', '"'))
               .Where(u => !string.IsNullOrWhiteSpace(u)) // elimina eventuali elementi vuoti
               .ToList();
            return priveConfigTo_Array;
        }

        public static string GetNameCodiceForProdottoDigitalePrivateConfig(IOrganizationService service, string key)
        {
            QueryExpression queryMessage = new QueryExpression();
            queryMessage.EntityName = "sc_privateconfiguration";
            queryMessage.ColumnSet.AddColumns("sc_description", "sc_value");
            queryMessage.Criteria.AddCondition("sc_key", ConditionOperator.Equal, key);

            var resultQuery = service.RetrieveMultiple(queryMessage);

            if (resultQuery.Entities.Count == 0)
            {
                return string.Empty;
            }
            return resultQuery.Entities[0].GetAttributeValue<string>("sc_value");
        }
    }
}
