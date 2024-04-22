using Asp.Versioning;
using Asp.Versioning.Builder;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Database;
using MinimalApi.Extensions;
using MinimalApi.Model;
using Serilog;
using Serilog.Events;
using Serilog.Templates.Themes;
using SerilogTracing;
using SerilogTracing.Expressions;
using System.Reflection;

// https://nblumhardt.com/2024/04/serilog-net8-0-minimal/
// https://www.meziantou.net/creating-ico-files-from-multiple-images-in-dotnet.htm
// https://code-maze.com/code-maze-weekly-210/
// https://medium.com/@malarsharmila/introduction-to-net-aspire-a-beginners-guide-to-getting-started-7887bfb1d13a
// https://medium.com/@malarsharmila/minimal-apis-with-filters-in-net-188afffce40a
// https://code-maze.com/aspnetcore-api-versioning/

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.WithProperty("Application", "Example")
    .WriteTo.Console(Formatters.CreateConsoleTextFormatter(theme: TemplateTheme.Code))
    //.WriteTo.Seq(
    //    Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341",
    //    apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY"))
    .CreateLogger();

using var listener = new ActivityListenerConfiguration()
    //.Instrument.AspNetCoreRequests()
    .TraceToSharedLogger();

Log.Information("Starting up");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddSerilog();
    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
    builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

    //builder.Services.AddControllers();
    //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
    var assembly = typeof(Program).Assembly;

    builder.Services.AddCarter();
    builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

    builder.Services.AddValidatorsFromAssembly(assembly);

    await using WebApplication app = builder.Build();

    ApiVersionSet apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1, 0))
        .ReportApiVersions()
        .Build();

    RouteGroupBuilder versionedGroup = app
        .MapGroup("api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    app.MapEndpoints(versionedGroup);

    app.MapGet("/", () => "Hello World!");

    app.MapCarter();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //app.UseAuthorization();    

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}