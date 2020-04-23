using System.Collections.Generic;
using Models;

namespace TaxJarApiClient
{
    // Data Transformation class to more easily move from our model to TaxJar's specific send/return types
    // The requirement was to return the tax amount to collect for an order
    // The response given by TaxJar and other Tax Calculator APIs will be custom to their design
    // so although I created an OrderDTO to transform typical order structure to their specific
    // and included a class structure for their response, the best strategy to fully implement would be
    // to have a data transformation pattern of some type that could be called on, per API service we support
    // I.e., 'Normalize' to our internal structure.
    // That is beyond the scope of this exercise.

    public class OrderProcessingDTO
    {
        // Strange - TaxJar does NOT like if we give it normal Case_Sensitive serialized objects.
        // It returns 406 - 'Not Acceptable'. It wants all lower_case json names.
        // So NewtonSoft serializes C# objects matching case.
        // The only option I found was to make JsonConvert convert Lower_Case to lower_Case. not lower_case as needed
        // thus why this entire file is lower_case exactly as in their API docs.
        public class OrderToPost
        {
            public string from_country;
            public string from_zip;
            public string from_state;
            public string from_city;
            public string from_street;
            public string to_country;
            public string to_zip;
            public string to_state;
            public string to_city;
            public string to_street;
            public float amount;
            public float shipping;
            public List<NexusAddress> nexus_addresses;
#if OPTIONAL
            public string Customer_Id;
#endif

            public List<lineitem> line_items;

            public OrderToPost()
            {
                line_items = new List<lineitem>();
            }

            public OrderToPost(Location fulfillmentLocation, Location shipToOverride,  Order inboundOrder)
            {
                from_zip = fulfillmentLocation.Zip;
                from_country = fulfillmentLocation.Country;
                from_state = fulfillmentLocation.State;
                from_city = fulfillmentLocation.City;
                from_street = fulfillmentLocation.Street;

                nexus_addresses = new List<NexusAddress>();
                if (shipToOverride != null)
                {
                    to_country = shipToOverride.Country;
                    to_state = shipToOverride.State;
                    to_city = shipToOverride.City;
                    to_street = shipToOverride.Street;
                    to_zip = shipToOverride.Zip;

                    var nxa = new NexusAddress(shipToOverride, "STO");
                    nexus_addresses.Add(nxa);
                }
                else
                {
                    // Default to Customer's address for ShipTo location
                    to_country = inboundOrder.Customer.Location.Country;
                    to_state = inboundOrder.Customer.Location.State;
                    to_city = inboundOrder.Customer.Location.City;
                    to_street = inboundOrder.Customer.Location.Street;
                    to_zip = inboundOrder.Customer.Location.Zip;
                    var nxa = new NexusAddress(inboundOrder.Customer.Location, "MAIN");
                    nexus_addresses.Add(nxa);
                }

                shipping = inboundOrder.ShippingCost;

                line_items = new List<lineitem>();
                foreach (var line in inboundOrder.ProductLineItems)
                {
                    lineitem l = new lineitem
                    {
                        id = line.Id,
                        product_tax_code = line.Product_Tax_Code,
                        quantity = line.Quantity,
                        unit_price = line.Unit_Price,
                        discount = 0
                    };
                    //l.product_tax_code = "20010";

                    line_items.Add(l);
                }
            }

        }

        public class lineitem
        {
            public string id;
            public string product_tax_code;
            public float unit_price;
            public int quantity;
            public float discount;
        }

        public class OrderResponse
        {
            public class LineItem
            {
                public float city_amount { get; set; }
                public float city_tax_rate { get; set; }
                public float city_taxable_amount { get; set; }
                public float combined_tax_rate { get; set; }
                public float county_amount { get; set; }
                public float county_tax_rate { get; set; }
                public float county_taxable_amount { get; set; }
                public string id { get; set; }
                public float special_district_amount { get; set; }
                public float special_district_taxable_amount { get; set; }
                public float special_tax_rate { get; set; }
                public float state_amount { get; set; }
                public float state_sales_tax_rate { get; set; }
                public float state_taxable_amount { get; set; }
                public float tax_collectable { get; set; }
                public float taxable_amount { get; set; }
            }

            public class Shipping
            {
                public float city_amount { get; set; }
                public float city_tax_rate { get; set; }
                public float city_taxable_amount { get; set; }
                public float combined_tax_rate { get; set; }
                public float county_amount { get; set; }
                public float county_tax_rate { get; set; }
                public float county_taxable_amount { get; set; }
                public float special_district_amount { get; set; }
                public float special_tax_rate { get; set; }
                public float special_taxable_amount { get; set; }
                public float state_amount { get; set; }
                public float state_sales_tax_rate { get; set; }
                public float state_taxable_amount { get; set; }
                public float tax_collectable { get; set; }
                public float taxable_amount { get; set; }
            }

            public class Breakdown
            {
                public float city_tax_collectable { get; set; }
                public float city_tax_rate { get; set; }
                public float city_taxable_amount { get; set; }
                public float combined_tax_rate { get; set; }
                public float county_tax_collectable { get; set; }
                public float county_tax_rate { get; set; }
                public float county_taxable_amount { get; set; }
                public List<LineItem> line_items { get; set; }
                public Shipping shipping { get; set; }
                public float special_district_tax_collectable { get; set; }
                public float special_district_taxable_amount { get; set; }
                public float special_tax_rate { get; set; }
                public float state_tax_collectable { get; set; }
                public float state_tax_rate { get; set; }
                public float state_taxable_amount { get; set; }
                public float tax_collectable { get; set; }
                public float taxable_amount { get; set; }
            }

            public class Jurisdictions
            {
                public string city { get; set; }
                public string country { get; set; }
                public string county { get; set; }
                public string state { get; set; }
            }

            public class Tax
            {
                public float amount_to_collect { get; set; }
                public Breakdown breakdown { get; set; }
                public bool freight_taxable { get; set; }
                public bool has_nexus { get; set; }
                public Jurisdictions jurisdictions { get; set; }
                public float order_total_amount { get; set; }
                public float rate { get; set; }
                public float shipping { get; set; }
                public string tax_source { get; set; }
                public float taxable_amount { get; set; }
            }

            public Tax tax { get; set; }
        }
    }
}
