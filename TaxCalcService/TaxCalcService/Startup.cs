using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;

namespace TaxCalcService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Below added because of confounding bug in ASP.NET Core 3.0+ that I hit that was
            // causing C# POCO objects to return as empty JSON objects to the calling client
            // See article at https://www.adamrussell.com/asp-net-core-blank-json-return-after-upgrading-to-3-0/
            // and the link to Microsoft's page that advises using their patch to use NewtonSoft instead of System.Text.JSON
            // After this then JSON objects returned properly to client with POCO's values

            services.AddMvc().AddNewtonsoftJson();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //GetCustomSettings();
        }

        private void GetCustomSettings()
        {

            // Read in our Tax Api configuration
            var taxApiProviders = new ConfigurationBuilder().AddJsonFile("taxapiproviders.json").Build();
            var taps = new TaxProviderSettings();
            taxApiProviders.Bind(taps);
            Configuration.Bind("TaxApiProviders", taxApiProviders);
        }
    }
}
