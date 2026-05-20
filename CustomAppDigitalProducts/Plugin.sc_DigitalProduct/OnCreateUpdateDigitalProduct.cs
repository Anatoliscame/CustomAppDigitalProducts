using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using Plugin.sc_DigitalProduct.CorePlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnCreateUpdateDigitalProduct : IPlugin
    {
        // Add
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnCreateUpdateDigitalProductLogic bl = new OnCreateUpdateDigitalProductLogic();

                trace.Trace("Start Plugin OnCreateUpdateDigitalProduct");

                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                if (context.Depth > 1)
                {
                    trace.Trace("Plugin interrotto per evitare loop.");
                    return;
                }

                Entity prodottoDigitale =
                            (context.MessageName.ToLower() == "create") ? (Entity)context.InputParameters["Target"] :
                            (context.MessageName.ToLower() == "update") ? Utilities.MergeEntities(context.PreEntityImages["sc_digitalproduct_pre"], (Entity)context.InputParameters["Target"]) : null;

                if (prodottoDigitale != null)
                {
                    //bl.ExecuteLogic(service, prodottoDigitale, context.MessageName, trace);
                }
                else
                {
                    trace?.Trace("prodottoDigitale is null");
                }
                trace?.Trace("End Plugin OnCreateUpdateDigitalProduct");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }

        }
    }
}
