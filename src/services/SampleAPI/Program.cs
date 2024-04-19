using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Consul;
using Microsoft.Extensions.DependencyInjection;
using SampleAPI;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using System.IO;

namespace SampleApi;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging((builderContext, logging) =>
            {
                logging.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
            });
}

//var builder = WebApplication.CreateBuilder(args);
    
//builder.Services.AddControllers();
//builder.Services.AddSwaggerGen(c =>
//    {
//        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleAPI", Version = "v1" });
//    });
//builder.Services.AddHealthChecks();
//builder.Services.AddConsulConfig(builder.Configuration);

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//    {
//        app.UseDeveloperExceptionPage();
//        app.UseSwagger();
//        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleAPI v1"));
//    }
//    app.UseConsul(builder.Configuration);
//    app.UseHttpsRedirection();
//    app.UseRouting();
//    app.UseAuthorization();
//    app.MapHealthChecks("/health");
//    app.MapControllers();
//    app.Run();