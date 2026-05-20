using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct.BusinessLogicPlugins
{
    public class OnPreUpdateDigitalProductVerifyColumnsLogic
    {
        public void ExecuteLogic(IOrganizationService service, ITracingService trace, Entity PreImage)
        {
            //Entity target =  (messageName.ToLower() == "update") ? PostImage : null;
            //if (PostImage == null) { trace?.Trace("Non hai apportato le modifiche su DigitalProduct POST"); return; }
            if (PreImage == null) { trace?.Trace("Non e' disponibile DigitalProduct PRE"); return; }

            ExecuteDigitalProduct(service, trace, PreImage);

        }
        public void ExecuteDigitalProduct(IOrganizationService service, ITracingService trace, Entity preImage)
        {
            var parentDigitProdLookupToPre = preImage.GetAttributeValue<EntityReference>(DigitalProduct.ParentDigitalProductId);

            if (parentDigitProdLookupToPre != null)
            {
                trace?.Trace("Esiste gia un padre DigitalProduct correlato e non puoi piu rimuovere da espansione, PRE");
                throw new InvalidPluginExecutionException("Esiste gia un padre DigitalProduct correlato e non puoi piu rimuovere da espansione, PRE");
            }
        }
    }
}
