using Carter;
using MediatR;

namespace Catalog.API.Products.DeactivateProduct
{
    public class DeactivateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/products/deactivate/{id}", async (Guid id, ISender sender) => {
                var result = await sender.Send(new DeactivateProductCommand(id));
                return Results.Ok(result);
            })
            .WithName("DeactivateProduct")
            .Produces<DeactivateProductResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Deactivate product")
            .WithDescription("Deactivate product");
        }
    }
}