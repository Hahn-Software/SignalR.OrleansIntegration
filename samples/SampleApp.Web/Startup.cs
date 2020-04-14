using System.Threading.Tasks;
using AspNetCore.SignalR.OrleansIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using SampleApp.Abstractions;
using SampleApp.Web.Hubs;

namespace SampleApp.Web
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
            services.AddRazorPages();

            var clusterClient = new ClientBuilder()
                .UseLocalhostClustering()
                .UseSignalR()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChatGrain).Assembly).WithReferences())
                .Build();

            var retries = 0;
            clusterClient.Connect(async ex =>
            {
                if (retries >= 5)
                    return false;

                await Task.Delay(1000);

                retries++;

                return true;
            }).Wait();

            services.AddSingleton(clusterClient);

            services.AddSignalR()
                .AddOrleans<ChatHub>()
                .AddOrleans<AnotherHub>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}