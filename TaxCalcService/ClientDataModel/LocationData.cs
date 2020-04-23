namespace Models
{
    public class Location
    {
        public string Country {get; set; }
        public string State {get; set; }
        public string City {get; set; }
        public string Street {get; set; }
        public string Zip {get; set; }

        public Location(Location location)
        {
            Country = location.Country;
            State = location.State;
            City = location.City;
            Street = location.Street;
            Zip = location.Zip;
        }

        public Location()
        {
        }
    }

    public class Rate : Location
    {
        public string County {get; set; }
        public float Country_Rate {get; set; }
        public float State_Rate {get; set; }
        public float County_Rate {get; set; }
        public float City_Rate {get; set; }
        public float Combined_District_Rate {get; set; }
        public float Combined_Rate {get; set; }
        public bool Freight_Taxable {get; set; }

        public Rate(Location location) : base(location)
        {
        }

        public Rate()
        {
        }
    }

    public class NexusAddress : Location
    {
        public string Id;

        public NexusAddress(Location location, string id) : base(location)
        {
            Id = id;
        }

        public NexusAddress() { }
    }

    public class RootObject
    {
        public Rate rate { get; set; }
    }
}
