using FunctionApp.sc_DigitalProducts.BusinessLogic;
using FunctionApp.sc_DigitalProducts.Entities;
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
        var _purchases = new ReleaseExpiredCartKeysTimerBL(_logger);

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("TestDigitalProductScameVersion2");

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

            Guid SystemUserId = Guid.Empty;
            Guid CaseId = Guid.Empty;

            // In attesa
            var purchasesArray = _purchases.GetPurchaseList(service, Convert.ToInt32(intTopQuery), 126400001);

            if (purchasesArray.Count > 0)
            {
                foreach (Entity purchase in purchasesArray)
                {
                    if (purchase == null) { continue; }

                    if (!purchase.Contains(Purchase.ExpirationDate))
                    {
                        _logger.LogWarning("Purchase {purchaseId} senza ExpirationDate. Record ignorato.", purchase.Id);
                        continue;
                    }
                    DateTime nowUtc = DateTime.UtcNow;
                    DateTime expirationDate = purchase.GetAttributeValue<DateTime>(Purchase.ExpirationDate);
                    TimeSpan remainingTime = expirationDate - nowUtc;

                    _logger.LogInformation("Purchase {purchaseId} - ExpirationDate: {expirationDate}, NowUtc: {nowUtc}, RemainingTime: {remainingTime}", purchase.Id, expirationDate, nowUtc, remainingTime);
                     
                    if (remainingTime <= TimeSpan.Zero)
                    {
                        _purchases.ExpireUpdatePurchase(service, purchase, null);
                        
                        _logger.LogInformation("Purchase {purchaseId} scaduto. IsExpired impostato a true.", purchase.Id);
                    }
                    else
                    {
                        DateTime newExpirationDate = expirationDate.Add(remainingTime);
                        _purchases.ExpireUpdatePurchase(service, purchase, newExpirationDate);
                        _logger.LogInformation("Purchase {purchaseId} non ancora scaduto. Tempo rimanente: {remainingTime}", purchase.Id, remainingTime);
                    }

                    Thread.Sleep(Convert.ToInt32(stringFrequency));
                }
            }
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