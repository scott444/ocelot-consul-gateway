namespace MinimalApi.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public static class Tags
{
    public const string Users = "Users";
}