namespace Models
{
    public class Customer
    {
        public string CustomerId;
        public string Name;
        public Location Location;
        public string ExemptionCode;

        public Customer(string id, string name, string city, string state, string zip, string street = null,
            string country = "US", string exemption = "non_exempt")
        {
            CustomerId = id;
            Name = name;

            Location = new Location()
            {
                Country = country,
                State = state,
                City = city,
                Street = street,
                Zip = zip
            };
        }

        public Customer()
        {
        }
    }
}
