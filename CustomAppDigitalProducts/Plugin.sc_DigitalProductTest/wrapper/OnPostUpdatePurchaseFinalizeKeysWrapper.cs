
using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct;
using Plugin.sc_DigitalProduct.Entities;
using System;


namespace PluginTestConsole.Wrapper
{
    class OnPostUpdatePurchaseFinalizeKeysWrapper
    {
        public void Execute(IOrganizationService service, string guid)
        {

            Entity target = GetTarget(service, guid);

            ITracingService tracingService = service as ITracingService;

            OnPostUpdatePurchaseFinalizeKeys plugin = new OnPostUpdatePurchaseFinalizeKeys();

            //plugin.Execute(service, target, tracingService);

        }

        private Entity GetTarget(IOrganizationService service, string guid)
        {
            var targetEntity = service.Retrieve(DigitalProduct.LogicalName, new Guid(guid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            return targetEntity;
        }
    }
}
