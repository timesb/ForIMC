using System.Collections.Generic;

namespace Models
{
    public class Order
    {

        public Customer Customer { get; set; }
        public float ShippingCost { get; set; }
        public List<LineItem> ProductLineItems { get; set; }

        public Location ShipFrom { get; set; }
        public Location ShipToOverride { get; set; }


        public Order(Customer customer)
        {
            Customer = customer;
        }

        public Order()
        {
        }

        public class LineItem
        {
            public string Id { get; set; }
            public string Product_Tax_Code { get; set; }
            public float Unit_Price { get; set; }
            public int Quantity { get; set; }

            public LineItem(Product product, int quantity)
            {
                Id = product.Id;
                Product_Tax_Code = product.Category;
                Quantity = quantity;
                Unit_Price = product.UnitPrice;
            }

            public LineItem()
            {
            }
        }
    }

}
