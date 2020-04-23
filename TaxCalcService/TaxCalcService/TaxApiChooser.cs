using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Models;
using Models.Interfaces;

namespace TaxCalcService
{
    public class TaxApiChooser
    {
        /// <summary>
        /// TaxApiChooser: Provides dynamic lookup and instantiation of TaxApiCaller assemblies so that we
        /// don't have to recompile and redeploy the entire TaxCalcService. We just deploy the new assemblies,
        /// update the appsettings.json and bounce the service. This also allows a key to be provided
        /// if a specific customer needs to use a specific external Tax Api. Services are cached once instantiated.
        /// </summary>
        private static IConfiguration _configuration;

        private static TaxProviderSettings _apiSettings;

        private static ConcurrentDictionary<string, ITaxApiCaller> _taxApiLibraries;

        public TaxApiChooser(IConfiguration configuration)
        {
            if (_configuration == null)
            {
                _configuration = configuration;
                _taxApiLibraries = new ConcurrentDictionary<string, ITaxApiCaller>();
                GetCustomSettings();
            }
        }

        public (string, List<string>) GetListOfLoadedTaxApis()
        {
            return (_apiSettings.ToString(), _taxApiLibraries.Keys.ToList());
        }

        public ITaxApiCaller GetAppropriateApi(string explicitKey = null)
        {
            ITaxApiCaller apiCaller = null;

            var keyOfApiToLoad = string.IsNullOrEmpty(explicitKey) ? _apiSettings.DefaultTaxApi : explicitKey;

            try
            {
                if (!_taxApiLibraries.ContainsKey(keyOfApiToLoad))
                {
                    TaxApiProvider provider = GetExplicitApi(keyOfApiToLoad);
                    var assemblyTypeNameOfApi = provider.AssemblyFQN;
                    string assemblyFileName = provider.Name + ".dll";
                    string assemblyPath = AppDomain.CurrentDomain.BaseDirectory + provider.Name + "Client.dll";
                    Assembly apiAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    var exported = apiAssembly.ExportedTypes.FirstOrDefault(t => t.Name == provider.ClrType);

                    if (exported == null)
                    {
                        throw new InvalidOperationException("Configuration of tax api clients is not valid.");
                    }

                    Type taxApiType = apiAssembly.GetType(exported.FullName ?? throw new InvalidOperationException());

                    object taxApiInstance = Activator.CreateInstance(taxApiType);

                    apiCaller = taxApiInstance as ITaxApiCaller;

                    if (apiCaller == null)
                    {
                        throw new InvalidOperationException($"Could not instantiate tax api library: '{keyOfApiToLoad}'");
                    }
                    apiCaller.AuthenticationToken = provider.ApiToken;

                    _taxApiLibraries.TryAdd(keyOfApiToLoad, apiCaller);
                }
                else
                {
                    apiCaller = _taxApiLibraries[keyOfApiToLoad];
                }
            }
            catch (Exception e)
            {
                // Do some logging here
                Debug.WriteLine($"Exception thrown when instantiating taxApi libraries: '{keyOfApiToLoad}'. Exception: {e}");
                throw;
            }

            return apiCaller;
        }

        private TaxApiProvider GetExplicitApi(string key)
        {
            // Use this strategy in the future to return which external tax api to call based
            // on customer (or can be any criteria

            // right now, we can use this to load a default one based on configuration
            var value = _apiSettings.TaxApiProviders.First(p => p.Name == key);

            return value;

        }

        private void GetCustomSettings()
        {
            // Read in our Tax Api configuration
            var taxApiProviders = new ConfigurationBuilder().AddJsonFile("taxapiproviders.json").Build();
            _apiSettings = new TaxProviderSettings();
            taxApiProviders.Bind(_apiSettings);
            _configuration.Bind("TaxApiProviders", taxApiProviders);
        }

    }
}
