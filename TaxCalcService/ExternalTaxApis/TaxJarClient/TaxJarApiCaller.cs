using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Interfaces;
using Newtonsoft.Json;

namespace TaxJarApiClient
{
    public class TaxJarApiCaller : ITaxCalculatorApi, ITaxApiCaller
    {
        #region ITaxApiCaller

        public string Identifier { get; set; }
        public string AuthenticationToken { get; set; }
        public string Version { get; set; }

        #endregion

        #region ITaxCalculatorApi
        // GET https://api.taxjar.com/v2/rates/:zip
        public Rate GetTaxRateForLocation(string zip, string country, string state, string city, string street)
        {
            const string _taxJarUrlTemplateForLocationRates = "https://api.taxjar.com/v2/rates/{0}";

            Rate locationTaxInfo = null;

            try
            {
                var taxJarUrl = string.Format(_taxJarUrlTemplateForLocationRates, zip);
                // Add other parameters in future if needed

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);

                var taxDataTask = client.GetAsync(string.Format(taxJarUrl, zip));
                var waiter = taxDataTask.GetAwaiter();
                var response = waiter.GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var taxInfoResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var ro = JsonConvert.DeserializeObject(taxInfoResponse, typeof(RootObject));
                    var rate = ((RootObject) ro).rate;

                    locationTaxInfo = rate;
                }
                else
                {
                    // Something wrong
                    Debug.WriteLine($"Error returned: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown: {e.Message}\n{e}");
            }

            return locationTaxInfo;
        }

        public float CalculateSalesTaxForOrder([FromBody]Order order)
        {
            const string _taxJarUrlForOrderTaxes = "https://api.taxjar.com/v2/taxes";

            float taxesForOrder;

            try
            {
                // Add other parameters in future if needed

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);

                // Convert and add as Post
                OrderProcessingDTO.OrderToPost otp = new OrderProcessingDTO.OrderToPost(order.ShipFrom, order.ShipToOverride, order);
                var postData = JsonConvert.SerializeObject(otp);
                //
                HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
                var orderTaxDataTask = client.PostAsync(_taxJarUrlForOrderTaxes, content);
                var response = orderTaxDataTask.GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var orderTaxInfoResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    //var or = JsonConvert.DeserializeObject(orderTaxInfoResponse, typeof(OrderProcessingDTO.OrderResponse));
                    //OrderProcessingDTO.OrderResponse r = (OrderProcessingDTO.OrderResponse) or;
                    var orderResponse = (OrderProcessingDTO.OrderResponse)JsonConvert.DeserializeObject(orderTaxInfoResponse, typeof(OrderProcessingDTO.OrderResponse));
                    //var atc = r2.tax.amount_to_collect;
                    
                    //var collectThis = ((OrderProcessingDTO.OrderResponse)or).
                    var collectThis = orderResponse.tax.amount_to_collect;

                    taxesForOrder = collectThis;
                }
                else
                {
                    // Something wrong
                    var message = $"TaxJarApi returned error: {response.StatusCode}. Reason: {response.ReasonPhrase}";
                    Debug.WriteLine(message);
                    var invalidOpEx = new InvalidOperationException(message);
                    invalidOpEx.Data.Add("Response", response);

                    throw invalidOpEx;
                }
            }
            catch (Exception e)
            {
                // Can grab any contextual stuff here - but going to rethrow so controller gets it
                Debug.WriteLine($"Exception thrown: {e.Message}\n{e}");
                throw;
            }

            return taxesForOrder;
        }

        public string TaxServicePing()
        {
            return "External TaxJar service";
        }
        #endregion
    }
}
