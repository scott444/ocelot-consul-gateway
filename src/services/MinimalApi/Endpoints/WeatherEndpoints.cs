using MediatR;

namespace MinimalApi.Endpoints;

public class WeatherEndpoints : IEndpoint
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("weather", async (ISender sender, CancellationToken cancellationToken) =>
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }).WithTags(Tags.Users);
    }
}
