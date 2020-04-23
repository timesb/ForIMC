using System.Collections.Generic;

namespace Models
{
    public class TaxApiProvider
    {
        public string Name { get; set; }
        public string AssemblyFQN { get; set; }
        public string ClrType { get; set; }
        public string GetUrl { get; set; }
        public string PostUrl { get; set; }
        public string ApiToken { get; set; }

        // For diagnostics
        public override string ToString()
        {
            var output = $"\t\tTaxApiProvider: {Name}:\n" +
                         $"\t\t\tAssemblyFQN (Fully Qualified Name: {AssemblyFQN}\n" +
                         $"\t\t\tCLR Type: {ClrType}\n" +
                         $"\t\t\tGetUrl: {GetUrl}\n" +
                         $"\t\t\tPostUrl: {PostUrl}\n" +
                         $"\t\t\tApiToken: {ApiToken}\n";
            return output;
        }
    }

    public class TaxProviderSettings
    {
        public string DefaultTaxApi { get; set; }

        public List<TaxApiProvider> TaxApiProviders { get; set; }

        public TaxProviderSettings()
        {
            TaxApiProviders = new List<TaxApiProvider>();
        }

        // For diagnostics
        public override string ToString()
        {
            var output = "\tTaxProvider Settings:\n" +
                         $"\t\tDefault Tax Api: {DefaultTaxApi}\n" +
                         $"\t\tTaxApiProviders:\n" +
                         string.Join("\n", TaxApiProviders);
            return output;
        }
    }


}
