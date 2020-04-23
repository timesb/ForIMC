namespace Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public float UnitPrice { get; set; }

        public Product(string id, string description, string category, float unitPrice)
        {
            Id = id;
            Description = description;
            Category = category;
            UnitPrice = unitPrice;
        }
    }
}
