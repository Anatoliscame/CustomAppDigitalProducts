using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using PluginTestConsole.Wrapper;
using System;
using System.Configuration;


namespace Plugin.sc_DigitalProductTest
{
    class Program
    {
        public static IOrganizationService CrmRepository(string connectionStringName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("CRM ConnectionString empty or null!");

            // Connect to the CRM web service using a connection string.
            ServiceClient conn = new ServiceClient(connectionString);

            return conn;
        }


        static void Main(string[] args)
        {
            // var service = CrmRepository("CRM_CREDENTIALS");

            var connectionString = System.Configuration.ConfigurationManager.AppSettings["TestDigitalProductScameVersion2"];
            CrmServiceClient crmServiceClient = new CrmServiceClient(connectionString);
            // string connectionString = ConfigurationManager.ConnectionStrings["CRM_CREDENTIALS"].ConnectionString;

            //CrmServiceClient crmServiceClient = new CrmServiceClient(connectionString);

            if (!crmServiceClient.IsReady)
            {
                throw new Exception("cannot instantiate service");
            }


            OnPostUpdatePurchaseFinalizeKeysWrapper wrapper = new OnPostUpdatePurchaseFinalizeKeysWrapper();

            string guid = "8430fefd-a54f-f111-bec7-7ced8d2f0304";

            wrapper.Execute(crmServiceClient, guid);

        }
    }
}
