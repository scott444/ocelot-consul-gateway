using Carter;
using MediatR;
using MinimalApi.Contracts;
using MinimalApi.Database;
using MinimalApi.Shared;

namespace MinimalApi.Features;

public static class GetWeather
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public class Query : IRequest<Result<WeatherResponse>>
    {
    }


    internal sealed class Handler(ApplicationDbContext dbContext) : IRequestHandler<Query, Result<WeatherResponse>>
    {

        public async Task<Result<WeatherResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = new WeatherResponse()
            {
                Values = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).ToArray()
            };

            return Result.Success(result);
        }
    }
}

public class GetWeatherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/weather", async (ISender sender) =>
        {
            var query = new GetWeather.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);

        }).WithTags(Tags.Weather);
    }
}