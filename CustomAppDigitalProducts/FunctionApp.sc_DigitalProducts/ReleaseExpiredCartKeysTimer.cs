using FunctionApp.sc_DigitalProducts.BusinessLogic;
using FunctionApp.sc_DigitalProducts.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var intTopQueryPurchase = Environment.GetEnvironmentVariable("TopQueryPurchase");
            var intTopQueryPurchaseOL = Environment.GetEnvironmentVariable("TopQueryPurchaseOL");

            _logger.LogInformation("Connessione Dataverse riuscita.");

            // In attesa
            var purchasesArray = _purchases.GetPurchaseList(service, Convert.ToInt32(intTopQueryPurchase), 126400001);

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
                    DateTime currentExpirationDate = purchase.GetAttributeValue<DateTime>(Purchase.ExpirationDate);

                    var purchaseOrderLines = _purchases.GetPurchaseOrderLineList(service, Convert.ToInt32(intTopQueryPurchaseOL), purchase.Id);
                    if (purchaseOrderLines.Count == 0)
                    {
                        if (currentExpirationDate <= nowUtc)
                        {
                            _purchases.ExpireUpdatePurchase(service, purchase, null);

                            _logger.LogInformation("Purchase {purchaseId} scaduto. IsExpired impostato a true.", purchase.Id);
                        }
                        continue;
                    }

                    //purchaseOrderLines = purchaseOrderLines.Skip(1).ToList();
                   // if (purchaseOrderLines.Count == 0) { continue; }

                    bool purchaseExpired = false;
                    bool expirationDateUpdated = false;


                    foreach (Entity purchaseOrderLine in purchaseOrderLines)
                    {

                        DateTime createdOn = purchaseOrderLine.GetAttributeValue<DateTime>(PurchaseOrderLine.CreatedOn);

                        _logger.LogInformation("PurchaseOrderLine {purchaseOrderLineId} da gestire nella Function App. CreatedOn: {createdOn}", purchaseOrderLine.Id, createdOn);
                        //string purchaseName = purchase.GetAttributeValue<string>(Purchase.Name) ?? purchase.Id.ToString();
                        //DateTime expirationDate = purchase.GetAttributeValue<DateTime>(Purchase.ExpirationDate);
                        TimeSpan remainingTime = currentExpirationDate - createdOn;

                        _logger.LogInformation("Purchase {purchaseId} - CurrentExpirationDate: {expirationDate}, PurchaseOrderLineCreatedOn: {createdOn}, RemainingTime: {remainingTime}", purchase.Id, currentExpirationDate, createdOn, remainingTime);

                        if (remainingTime <= TimeSpan.Zero)
                        {
                            _purchases.ExpireUpdatePurchase(service, purchase, null);

                            _logger.LogInformation("Purchase {purchaseId} scaduto. IsExpired impostato a true.", purchase.Id);

                            purchaseExpired = true;
                            break;
                        }
                        
                        _purchases.updatePurchaseOLIsExpiration(service, purchaseOrderLine);

                        TimeSpan elapsedTime = (nowUtc - remainingTime) - createdOn;

                        currentExpirationDate = currentExpirationDate.Add(remainingTime);
                        expirationDateUpdated = true;
                    }
                    if (purchaseExpired) { continue; }

                    if (currentExpirationDate <= nowUtc)
                    {
                        _purchases.ExpireUpdatePurchase(service, purchase, null);
                        _logger.LogInformation("Purchase {purchaseId} scaduto. IsExpired impostato a true.", purchase.Id);
                        continue;
                    }

                    if (expirationDateUpdated)
                    {
                        _purchases.ExpireUpdatePurchase(service, purchase, currentExpirationDate);
                        _logger.LogInformation("Purchase {purchaseId} non ancora scaduto. Nuova ExpirationDate calcolata: {newExpirationDate}", purchase.Id,currentExpirationDate);
                    }

                    //_purchases.ExpireUpdatePurchase(service, purchase, newExpirationDate);
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