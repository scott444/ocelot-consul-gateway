using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace SampleAPI;

public class Startup(IConfiguration configuration)
{
    private const string SERVICE_NAME = "SampleService.OpenApi";

    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddConsul(Configuration.GetServiceConfig());
        services.AddHttpContextAccessor();

        services.AddControllers();
        services.AddCors();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleAPI", Version = "v1" });
        });
        services.AddHealthChecks();

        services.AddRouting(options => options.LowercaseUrls = true);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleAPI v1"));
        }
        else
        {
            app.UseHsts();
        }

        app.UseRouting();



        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("", async context =>
            {
                await context.Response.WriteAsync(SERVICE_NAME);
            });
            endpoints.MapHealthChecks("/healthcheck");
            endpoints.MapControllers();
        });
    }
}