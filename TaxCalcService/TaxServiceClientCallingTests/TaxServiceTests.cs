using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Models;
using TaxCalcService.Controllers;

namespace TaxServiceClientCallingTests
{
    public class Tests
    {
        const string PINGURL = "https://localhost:44371/taxservice/ping";
        const string TAXiNFOURL = "https://localhost:44371/taxservice/taxrateforlocation/{0}";
        const string ORDERTAXURL = "https://localhost:44371/taxservice/GetTaxForOrder";

        private const float PBCTAXRATE = 0.07F;

        private TaxServiceController _taxServicecontroller;
        private IConfiguration _configuration;
        private TaxProviderSettings _apiSettings;

        [SetUp]
        public void Setup()
        {
            var taxApiProviders = new ConfigurationBuilder().AddJsonFile("taxapiproviders.json").Build();
            _apiSettings = new TaxProviderSettings();
            taxApiProviders.Bind(_apiSettings);
            _configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
            _configuration.Bind("TaxApiProviders", taxApiProviders);
            _taxServicecontroller = new TaxServiceController(_configuration)
            {
                ControllerContext = {HttpContext = new DefaultHttpContext()}
            };

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Test]
        public void PingService()
        {
            var pingResponse = _taxServicecontroller.TaxServicePing();
            StringAssert.StartsWith("TaxService:", pingResponse);
        }

        [Test]
        public void TestAlive()
        {
            var aliveResponse = _taxServicecontroller.Get();
            StringAssert.Contains("Connected to TaxService!", aliveResponse);
        }

        [Test]
        public void TestZipParameterVerificationForFailure()
        {
            var zipResponse = _taxServicecontroller.GetTaxRateForLocation(null, null, null, null, null);

            Assert.AreEqual(_taxServicecontroller.Response.StatusCode, 400);

            Assert.AreEqual(null, zipResponse);
        }

        [Test]
        public void TestTaxesForLocation()
        {
            var ratesResponse = _taxServicecontroller.GetTaxRateForLocation("33408", null, null, null, null);

            Assert.AreEqual(ratesResponse.Combined_Rate, PBCTAXRATE);
        }

        [Test]
        public void TestMinReqsCheckForOrderTax()
        {
            var testOrder = MockOrderInfo.GetOrderForTest();
            testOrder.ShipFrom.Country = null;
            testOrder.ShipFrom.Zip = "";
            testOrder.Customer.Location.Country = null;
            testOrder.Customer.Location.Zip = null;
            testOrder.ShipToOverride.Country = null;
            testOrder.ShipToOverride.Zip = null;
            var orderTaxResponse = _taxServicecontroller.CalculateSalesTaxForOrder(testOrder);

            Assert.AreEqual(orderTaxResponse, 0);

            Assert.AreNotEqual(_taxServicecontroller.Response.StatusCode, 200);
        }

        [Test]
        public void TestHappyPathForOrderTaxAmount()
        {
            var testOrder = MockOrderInfo.GetOrderForTest();
            var orderTaxResponse = _taxServicecontroller.CalculateSalesTaxForOrder(testOrder);

            Assert.AreEqual(orderTaxResponse, 21000.70F);

            Assert.AreEqual(_taxServicecontroller.Response.StatusCode, 200);

        }
    }
}