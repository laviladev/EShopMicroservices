using Carter;
using MediatR;

namespace Catalog.API.Products.UpdateProduct
{
    public class UpdateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/products", async (UpdateProductCommand request, ISender sender) => {
                var result = await sender.Send(request);
                return Results.Ok(result);
            })
            .WithName("UpdateProduct")
            .Produces<UpdateProductResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update product")
            .WithDescription("Update product");
        }
    }
}