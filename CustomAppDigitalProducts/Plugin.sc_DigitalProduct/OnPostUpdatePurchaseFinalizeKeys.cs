using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnPostUpdatePurchaseFinalizeKeys : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnPostUpdatePurchaseFinalizeKeysLogic bl = new OnPostUpdatePurchaseFinalizeKeysLogic();

                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                trace.Trace("Start Plugin OnPostUpdatePurchaseSetKeyDigitalProductPurchase");

                var target = new Entity();

                if (context.MessageName.ToLower() == "update")
                {
                    target = context.PostEntityImages.Contains("sc_purchase_post") ? context.PostEntityImages["sc_purchase_post"] : null;

                    bl.ExecuteLogic(service, target, trace);
                }

                trace.Trace("End Plugin OnPostUpdatePurchaseSetKeyDigitalProductPurchase");

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    } 
}

