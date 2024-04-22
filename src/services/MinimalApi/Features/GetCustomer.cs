using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Contracts;
using MinimalApi.Database;
using MinimalApi.Shared;

namespace MinimalApi.Features;


public static class GetCustomer
{
    public class Query : IRequest<Result<CustomerResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler(ApplicationDbContext dbContext) : IRequestHandler<Query, Result<CustomerResponse>>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Result<CustomerResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customerResponse = await _dbContext
                .Customers
                .AsNoTracking()
                .Where(customer => customer.Id == request.Id)
                .Select(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    CreatedOnUtc = customer.CreatedOnUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (customerResponse is null)
            {
                return Result.Failure<CustomerResponse>(new Error(
                    "GetCustomer.Null",
                    "The customer with the specified ID was not found"));
            }

            return customerResponse;
        }
    }
}

public class GetCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/customers/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetCustomer.Query { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        }).WithTags(Tags.Customer);
    }
}
