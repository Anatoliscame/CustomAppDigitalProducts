using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnPreDeletePurchaseReleaseKeys : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnPreDeletePurchaseReleaseKeysLogic bl = new OnPreDeletePurchaseReleaseKeysLogic();

                trace.Trace("Start Plugin OnPreDeletePurchaseReleaseKeys");

                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");


                Entity preImage = context.PreEntityImages.Contains("sc_purchase_pre") ? context.PreEntityImages["sc_purchase_pre"] : null;

                if (context.MessageName.ToLower() == "delete")
                {
                    bl.ExecuteLogic(service, preImage, trace);
                }
                trace?.Trace("End Plugin OnPreDeletePurchaseReleaseKeys");

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
