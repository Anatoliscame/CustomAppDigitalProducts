using Microsoft.Xrm.Sdk;
using Plugin.sc_DigitalProduct.BusinessLogicPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.sc_DigitalProduct
{
    public class OnPreUpdateDigitalProductVerifyColumns : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OnPreUpdateDigitalProductVerifyColumnsLogic bl = new OnPreUpdateDigitalProductVerifyColumnsLogic();

                trace.Trace("Start Plugin OnPreUpdateDigitalProductVerifyColumns");
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (trace == null)
                    throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

                if (context.Depth > 1)
                {
                    trace.Trace("Plugin interrotto per evitare loop.");
                    return;
                }
                // Entity target = (Entity)context.InputParameters["Target"];

                if (context.MessageName.ToLower() == "update")
                {
                    Entity preImage = context.PreEntityImages.Contains("sc_digitalproduct_pre") ? context.PreEntityImages["sc_digitalproduct_pre"] : null;
                  
                    bl.ExecuteLogic(service, trace, preImage);
                }
                else
                {
                    return;  // Esci subito dal metodo, non serve continuare
                }
                    trace?.Trace("End Plugin OnPreUpdateDigitalProductVerifyColumns");


            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}

