using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            appBuilder.UseDeveloperExceptionPage();
        }

        appBuilder.UseSerilogRequestLogging();
        appBuilder.UseHttpsRedirection();

        appBuilder.UseHangfireDashboard();

        appBuilder.UseRouting();
        appBuilder.UseAuthorization();

        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}