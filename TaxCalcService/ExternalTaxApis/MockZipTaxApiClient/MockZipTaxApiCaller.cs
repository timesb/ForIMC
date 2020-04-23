using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Interfaces;
using Newtonsoft.Json;

namespace MockZipTaxApiClient
{
    public class MockZipTaxApiCaller : ITaxCalculatorApi, ITaxApiCaller
    {

        #region ITaxApiCaller

        public string Identifier { get; set; }
        public string AuthenticationToken { get; set; }
        public string Version { get; set; }

        #endregion

        #region ITaxCalculatorApi
        // GET https://api.zip-tax.com/request/v40?key=1234567890&postalcode=90264
        public Rate GetTaxRateForLocation(string zip, string country, string state, string city, string street)
        {
            const string _taxJarUrlTemplateForLocationRates = "https://api.taxjar.com/v2/rates/zip={0}";

            Rate locationTaxInfo = null;

            try
            {
                var taxJarUrl = string.Format(_taxJarUrlTemplateForLocationRates, zip);
                // Add other parameters in future if needed

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var taxDataTask = client.GetAsync(string.Format(taxJarUrl, zip));
                var waiter = taxDataTask.GetAwaiter();
                var response = waiter.GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var taxInfoResponseWaiter = response.Content.ReadAsStringAsync().GetAwaiter();
                    var taxInfoResponse = taxInfoResponseWaiter.GetResult();

                    locationTaxInfo = JsonConvert.DeserializeObject<Rate>(taxInfoResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return locationTaxInfo;
        }

        public float CalculateSalesTaxForOrder([FromBody]Order order)
        {
            throw new NotImplementedException();
        }

        public string TaxServicePing()
        {
            return "ZipTaxApiCaller is loaded!";
        }
        #endregion
    }
}
