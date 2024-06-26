using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleAPI", Version = "v1" });
    });
builder.Services.AddHealthChecks();
//builder.Services.AddConsulConfig(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleAPI v1"));
}
//app.UseConsul(builder.Configuration);
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();