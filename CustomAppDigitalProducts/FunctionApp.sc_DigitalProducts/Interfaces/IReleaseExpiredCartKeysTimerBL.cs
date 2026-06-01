using FunctionApp.sc_DigitalProducts.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp.sc_DigitalProducts.Interfaces
{
    public interface IReleaseExpiredCartKeysTimerBL
    {
        public List<DigitalProduct> GetDigitalProductList(int IntTopQueryDigitalProduct);
        public List<Entity> GetPurchaseList(IOrganizationService service, int topCount, int statuspurchase);
        public List<Entity> GetPurchaseOrderLineList(IOrganizationService service, int topCount, Guid purchaseId);
        public void ExpireUpdatePurchase(IOrganizationService service, Entity purchase, DateTime? newExpirationDate);

    }
}
 