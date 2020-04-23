using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;
using Models.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaxCalcService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxServiceController : ControllerBase, ITaxCalculatorApi
    {
        private IConfiguration _configuration;
        private readonly TaxApiChooser _apiChooser;

        public TaxServiceController(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiChooser = new TaxApiChooser(_configuration);
        }

        // GET: 'It is ALIVE!'
        [HttpGet]
        public string Get()
        {
            return "Connected to TaxService!";
        }

        [HttpGet("TaxRateForLocation/{zip:required}/{country?}/{state?}/{city?}/{street?}")]
        public Rate GetTaxRateForLocation(string zip, string country, string state, string city, string street)
        {
            Rate locationWithRateInfo = null;
            bool failedVerification = false;

            Response.StatusCode = BadRequest().StatusCode;

            if (string.IsNullOrEmpty(zip) || (zip.Length != 5 && zip.Length != 10))
            {
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Zip code is invalid.";
                failedVerification = true;
            }

            // Threw in this online checker just in case bogus zip
            if (!failedVerification)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var s = client.DownloadString($"http://api.zippopotam.us/us/{zip}");
                    }

                    // Of course we can extend this to do actual comparison for Country, state, etc...
                }
                catch (WebException wex)
                {
                    failedVerification = true;
                    Debug.WriteLine(
                        $"Exception thrown when calling zip lookup: {wex.Message}. Response: {wex.Response}");
                    if (wex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var zipLookupReturnStatusCode = ((HttpWebResponse) wex.Response).StatusCode;
                        var errorDescription = ((HttpWebResponse) wex.Response).StatusDescription;
                        Debug.WriteLine($"Status Code : {zipLookupReturnStatusCode}. Error: {errorDescription}");

                        Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Zip code is invalid.";
                    }
                }

                //
            }


            if (!failedVerification)
            {

                ITaxApiCaller taxApi = _apiChooser.GetAppropriateApi();

                locationWithRateInfo =
                    ((ITaxCalculatorApi) taxApi).GetTaxRateForLocation(zip, country, state, city, street);
                Response.StatusCode = Ok().StatusCode;
            }

            return locationWithRateInfo;
        }

        [HttpPost("GetTaxForOrder/")]
        public float CalculateSalesTaxForOrder([FromBody] Order order)
        {
            float totalSalesTaxForOrder = 0;

            try
            {
                if (order.ProductLineItems == null || order.ProductLineItems.Count == 0)
                {
                    // More detailed verification of Order, Customer, Product info can be added here...

                    Response.StatusCode = 400;
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase =
                        "Order contains no product line items.";
                }
                else
                {
                    ITaxApiCaller taxApi = _apiChooser.GetAppropriateApi();

                    totalSalesTaxForOrder = ((ITaxCalculatorApi) taxApi).CalculateSalesTaxForOrder(order);
                }
            }
            catch (InvalidOperationException iox)
            {
                // taxApi client did not like something. Let's check further
                if (iox.Data.Contains("Response"))
                {
                    HttpResponseMessage apiResponse = (HttpResponseMessage)iox.Data["Response"];
                    Response.StatusCode = (int) apiResponse.StatusCode;
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = apiResponse.ReasonPhrase;
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
            }

            return totalSalesTaxForOrder;
        }

        [HttpGet("Ping")]
        public string TaxServicePing()
        {
            var (apiSettings, apisLoaded) = _apiChooser.GetListOfLoadedTaxApis();
            var message = apisLoaded.Count == 0 ? "None" : ("\n\tTax Apis loaded:\n" + string.Join("\n\t", apisLoaded));
            return $"TaxService:\n {apiSettings}\n\n\t{message}";
        }
    }
}
