using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using Plugin.sc_DigitalProduct.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnCreateUpdateStatusPurchaseRegisterPurchaseHistory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnCreateUpdateStatusPurchaseRegisterPurchaseHistoryLogic _bl = new OnCreateUpdateStatusPurchaseRegisterPurchaseHistoryLogic();

                trace.Trace("Start Plugin OnCreateUpdateDigitalProduct");

                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                if (context.Depth > 1)
                {
                    trace.Trace("Plugin interrotto per evitare loop.");
                    return;
                }
                if (context.MessageName.ToLower() == "update")
                {

                    Entity postImage = context.PostEntityImages.Contains("sc_purchase_post") ? context.PostEntityImages["sc_purchase_post"] : null;
                    if (postImage == null)
                    {
                        trace?.Trace("postImage non presente.");
                        throw new InvalidPluginExecutionException("postImage non presente.");
                    }
                    Entity preImage = context.PreEntityImages.Contains("sc_purchase_pre") ? context.PreEntityImages["sc_purchase_pre"] : null;
                    if (preImage == null)
                    {
                        trace?.Trace("PreImage non presente.");
                        throw new InvalidPluginExecutionException("PreImage non presente.");
                    }

                    _bl.ExecuteStatusPurchaseRegister(service, postImage, preImage,trace, Purchase.ModifiedBy);
                }

                trace.Trace("End Plugin OnCreateUpdateKeStatusRegisterTracking");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
