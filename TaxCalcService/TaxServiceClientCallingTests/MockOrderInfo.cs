using System.Collections.Generic;
using Models;

namespace TaxServiceClientCallingTests
{
    public static class MockOrderInfo
    {
        public static Order GetOrderForTest()
        {
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

            return o;
        }
    }
}
