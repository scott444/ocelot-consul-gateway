using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using MinimalApi.Contracts;
using MinimalApi.Database;
using MinimalApi.Entities;
using MinimalApi.Shared;

namespace MinimalApi.Features;

public static class CreateCustomer
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
        }
    }

    internal sealed class Handler(ApplicationDbContext dbContext, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly IValidator<Command> _validator = validator;

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error("CreateCustomer.Validation", validationResult.ToString()));
            }

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedOnUtc = DateTime.UtcNow,
            };

            _dbContext.Add(customer);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return customer.Id;
        }
    }


}
public class CreateCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/customers", async (CreateCustomerRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateCustomer.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }

            return Results.Ok(result.Value);
        }).WithTags(Tags.Customer);
    }
}
