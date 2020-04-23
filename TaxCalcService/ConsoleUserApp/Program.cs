using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;

namespace UserAppConsole
{
    class Program
    {
        const string pingUrl = "https://localhost:44371/taxservice/ping";
        const string taxInfoUrl = "https://localhost:44371/taxservice/taxrateforlocation/{0}";
        const string orderTaxUrl = "https://localhost:44371/taxservice/GetTaxForOrder";

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter zip code: ");
                var zip = Console.ReadLine();
                if (string.IsNullOrEmpty(zip)) break;

                await CallServiceForTaxRatesForZip(zip);
            }

            Console.Write("Press ENTER to post test order...");
            await CallServiceToPostOrder();

            Console.Write("Press ENTER to exit...");
            Console.ReadLine();


        }

        static async Task CallServiceForTaxRatesForZip(string zip)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var taxDataTask = client.GetAsync(string.Format(taxInfoUrl, zip));
                var response = await taxDataTask;
                if (response.IsSuccessStatusCode)
                {
                    var taxInfoResponse = await response.Content.ReadAsStringAsync();

                    Rate tax = JsonConvert.DeserializeObject<Rate>(taxInfoResponse);

                    Console.WriteLine($"The combined tax rate for zip {zip} is {tax.Combined_Rate:P2}");
                }
                else
                {
                    Console.WriteLine($"The TaxService returned a Non-Successful response code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static async Task CallServiceToPostOrder()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Customer c = new Customer("12345", "Jiffy Lube", "Royal Palm Beach", "FL", "33411", "1203 N State Rd 7");
                Product p = new Product("N95-1", "N95 masks", null, 1000);

                var lineItem = new Order.LineItem(p, 300);
                var lineItems = new List<Order.LineItem> {lineItem};

                Location shipFrom = new Location
                {
                    Country = "US",
                    State = "FL",
                    City = "Juno Beach",
                    Street = "790 Juno Ocean Walk #402-403",
                    Zip = "33408"
                };
                var o = new Order(c)
                {
                    ShipFrom = shipFrom,
                    ShipToOverride = c.Location,
                    ShippingCost = 9.95F,
                    ProductLineItems = lineItems
                };

                var postData = JsonConvert.SerializeObject(o);
                HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
                var orderTaxDataTask = client.PostAsync(orderTaxUrl, content);

                var response = await orderTaxDataTask;
                if (response.IsSuccessStatusCode)
                {
                    var taxInfoResponse = await response.Content.ReadAsStringAsync();

                    float tax = JsonConvert.DeserializeObject<float>(taxInfoResponse);

                    Console.WriteLine($"The combined tax for the order is {tax:C}");
                }
                else
                {
                    Console.WriteLine($"The TaxService returned a Non-Successful response code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
