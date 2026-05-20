using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnPostCreatePurchaseOrderLineProcessDigitalProductPurchase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnPostCreatePurchaseOrderLineProcessDigitalProductPurchaseLogic bl = new OnPostCreatePurchaseOrderLineProcessDigitalProductPurchaseLogic();

                trace.Trace("Start Plugin OnPostCreatePurchaseOrderLineProcessDigitalProductPurchase");
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                var target = new Entity();

                if (context.MessageName.ToLower() == "create") 
                {
                    target = (Entity)context.InputParameters["Target"];
                    bl.ExecuteLogic(service, target, trace);

                    trace?.Trace("End Plugin OnPostCreatePurchaseOrderLineProcessDigitalProductPurchase");
                }

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}

