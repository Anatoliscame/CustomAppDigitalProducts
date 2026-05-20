using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnPostUpdateProductDetailsSyncDigitalProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnPostUpdateProductDetailsSyncDigitalProductLogic bl = new OnPostUpdateProductDetailsSyncDigitalProductLogic();

                trace.Trace("Start Plugin OnPostUpdateProductDetailsSyncDigitalProduct");
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                if (context.Depth > 1)
                {
                    trace.Trace("Plugin interrotto per evitare loop.");
                    return;
                }

                Entity postImage = context.PostEntityImages.Contains("sc_productdetails_post") ? context.PostEntityImages["sc_productdetails_post"] : null;

                if (context.MessageName.ToLower() == "update")
                {
                    bl.ExecuteLogic(service, postImage, trace);
                }
                trace?.Trace("End Plugin OnPostUpdateProductDetailsSyncDigitalProduct");


            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }

        }
    }
}

