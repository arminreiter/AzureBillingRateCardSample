# Azure Billing RateCard API Sample
This is a sample C# console application that uses the Azure Billing RateCard API. It reads the data from the API and creates a CSV file out of it.

**Please find a mored detailed describtion at my blog: [Export Azure RateCard data to CSV with C# and Billing API](https://codehollow.com/2016/12/export-azure-ratecard-data-csv-csharp-billing-api/)**

## Instructions

1. Open and modify the app.config
   * Tenant (required)
   * SubscriptionId (required)
   * OfferDurableId 
   * Currency
   * Locale
   * RegionInfo
   * CsvFilePath
   * The other values will be set in the following steps.
2. Add a new application to active directory: 
   * Open https://portal.azure.com/, navigate to the Azure Active Directory and App Registrations
   * Add a new **native** application
3. Add delegated permissions for Windows Azure Service Management
   * App registrations - Settings - Required Permissions - Add - Windows Azure Service Management API
4. Copy the Application ID and paste it as ClientId in the app.config
   * If you use application authentication, create a new key and copy it to app.config - client secret
5. Give the user/application at least "Reader" rights for the subscription
   * Subscriptions - your subscriptions - Access control (IAM)
6. Build and run the application

# Troubleshooting

Check out the troubleshooting section at my blog: https://codehollow.com/2016/12/export-azure-ratecard-data-csv-csharp-billing-api#troubleshoot
or check the troubleshooting section of this sample: https://github.com/Azure-Samples/billing-dotnet-ratecard-api
