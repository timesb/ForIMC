namespace Models.Interfaces
{
    public interface ITaxCalculatorApi
    {
        // GET https://api.taxjar.com/v2/rates/:zip
        Rate GetTaxRateForLocation(string zip, string country, string state, string city, string street);


        // POST https://api.taxjar.com/v2/taxes
        // The requirement was to return the tax amount to collect for an order
        // The response given by TaxJar and other Tax Calculator APIs will be custom to their design
        // so although I created an OrderDTO to transform typical order structure to their specific
        // and included a class structure for their response, the best strategy to fully implement would be
        // to have a data transformation pattern of some type that could be called on, per API service we support
        // I.e., 'Normalize' to our internal structure.
        // That is beyond the scope of this exercise.
        float CalculateSalesTaxForOrder(Order order);

        string TaxServicePing();
    }
}
