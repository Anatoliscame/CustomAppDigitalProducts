using FunctionApp.sc_DigitalProducts.BusinessLogic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;

namespace FunctionApp.sc_DigitalProducts;

public class ReleaseExpiredCartKeysTimer
{
    private readonly ILogger _logger;

    public ReleaseExpiredCartKeysTimer(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ReleaseExpiredCartKeysTimer>();
    }

    [Function("ReleaseExpiredCartKeysTimer")]
    public void Run([TimerTrigger("%TriggerTimerDigitalProducts%")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        ServiceClient serviceClient = null;

        var damManager = new ReleaseExpiredCartKeysTimerBL(_logger);

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("CRM_CustomeAppScame");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("CRM_CustomeAppScame non configurata.");
                return;
            }

            serviceClient = new ServiceClient(connectionString);

            if (!serviceClient.IsReady)
            {
                _logger.LogError("Errore connessione Dataverse: {error}", serviceClient.LastError);
                return;
            }

            IOrganizationService service = serviceClient;

            var stringFrequency = Environment.GetEnvironmentVariable("Frequency");
            var intTopQuery = Environment.GetEnvironmentVariable("TopQuery");

            _logger.LogInformation("Connessione Dataverse riuscita.");

            // Qui dopo userai:
            // service.RetrieveMultiple(...)
            // service.Update(...)
            // service.Create(...)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante esecuzione ReleaseExpiredCartKeysTimer.");
        }
        finally
        {
            serviceClient?.Dispose();
        }
    }
}