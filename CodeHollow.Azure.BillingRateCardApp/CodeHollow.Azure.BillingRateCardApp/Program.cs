using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CodeHollow.Azure.BillingRateCardApp
{
    class Program
    {
        #region settings

        // Azure AD Settings
        static readonly string TENANT = ConfigurationManager.AppSettings["Tenant"];
        static readonly string CLIENTID = ConfigurationManager.AppSettings["ClientId"];
        static readonly string CLIENTSECRET = ConfigurationManager.AppSettings["ClientSecret"];
        static readonly string SUBSCRIPTIONID = ConfigurationManager.AppSettings["SubscriptionId"];
        static readonly string REDIRECTURL = ConfigurationManager.AppSettings["RedirectUrl"];

        static readonly string RESOURCE = "https://management.azure.com/";

        // API Settings
        static readonly string APIVERSION = "2016-08-31-preview"; // "2015-06-01-preview";
        static readonly string OFFERDURABLEID = ConfigurationManager.AppSettings["OfferDurableId"];
        static readonly string CURRENCY = ConfigurationManager.AppSettings["Currency"];
        static readonly string LOCALE = ConfigurationManager.AppSettings["Locale"];
        static readonly string REGIONINFO = ConfigurationManager.AppSettings["RegionInfo"];

        // Application settings
        static readonly string CSVFILEPATH = ConfigurationManager.AppSettings["CsvFilePath"];

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Get OAuth token...");
            var token = GetOAuthTokenFromAAD();
            Console.WriteLine("Token received, read rate card data...");

            var data = GetRateCardData(token);
            if (String.IsNullOrEmpty(data))
            {
                Console.WriteLine("Press key to exit");
                Console.Read();
                return;
            }
            Console.WriteLine("Data received! Parse data and create csv file...");
            var ratecard = JsonConvert.DeserializeObject<RateCard>(data);

            string csv = CreateCsv(ratecard.Meters);

            System.IO.File.WriteAllText(CSVFILEPATH, csv, Encoding.UTF8);

            Console.WriteLine("CSV file successfully created. Press key to exit");
            Console.Read();
        }

        public static string GetOAuthTokenFromAAD()
        {
            AuthenticationResult result;

            if (String.IsNullOrEmpty(CLIENTSECRET)) // if no client secret - authenticate with user...
                result = GetOAuthTokenForUser();
            else                                    // else authenticate with application
                result = GetOAuthTokenForApplication(CLIENTSECRET);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        private static AuthenticationResult GetOAuthTokenForUser()
        {
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TENANT}");
            var authTask = authenticationContext.AcquireTokenAsync(RESOURCE, CLIENTID,
                new Uri(REDIRECTURL), new PlatformParameters(PromptBehavior.Auto));
            authTask.Wait();
            return authTask.Result;
        }

        private static AuthenticationResult GetOAuthTokenForApplication(string clientSecret)
        {
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TENANT}");
            ClientCredential clientCred = new ClientCredential(CLIENTID, clientSecret);
            var authTask = authenticationContext.AcquireTokenAsync(RESOURCE, clientCred);
            authTask.Wait();
            return authTask.Result;
        }

        public static string GetRateCardData(string token)
        {
            string url = $"https://management.azure.com/subscriptions/{SUBSCRIPTIONID}/providers/Microsoft.Commerce/RateCard?api-version={APIVERSION}&$filter=OfferDurableId eq '{OFFERDURABLEID}' and Currency eq '{CURRENCY}' and Locale eq '{LOCALE}' and RegionInfo eq '{REGIONINFO}'";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("An error occurred! That's what I got:");
                Console.WriteLine(response.ToString());
                return string.Empty; ;
            }

            var readTask = response.Content.ReadAsStringAsync();
            readTask.Wait();
            return readTask.Result;
        }

        public static string CreateCsv(List<Meter> meters)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("MeterId;MeterName;MeterCategory;MeterSubCategory;Unit;MeterTags;MeterRegion;MeterRates;EffectiveDate;IncludedQuantity;MeterStatus");

            meters.ForEach(x =>
            {
                string meterRates = string.Join(",", x.MeterRates.Select(y => " [ " + y.Key.ToString() + " : " + y.Value.ToString() + " ]"));
                string meterTags = string.Join(",", x.MeterTags);
                sb.AppendLine($"{x.MeterId};{x.MeterName};{x.MeterCategory};{x.MeterSubCategory};{x.Unit};\"{meterTags}\";{x.MeterRegion};\"{meterRates}\";{x.EffectiveDate};{x.IncludedQuantity};{x.MeterStatus}");
            });

            return sb.ToString();
        }
    }
}
