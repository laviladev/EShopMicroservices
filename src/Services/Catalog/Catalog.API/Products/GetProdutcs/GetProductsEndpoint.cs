using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Products.GetProdutcs
{
    public class GetProductsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async ([FromBody] GetProductsRequest? request, ISender sender) => {
                GetProductsRequest query;
                if (request == null) {
                    query = new GetProductsRequest(null, null, null, null, null);
                } else {
                    query = request.Adapt<GetProductsRequest>();
                }
                var result = await sender.Send(query);
                var response = result.Adapt<GetProductsResult>();
                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get products")
            .WithDescription("Get products");
        
            app.MapGet("/products/{id}", async (Guid id, ISender sender) => {
                var query = new GetProductByIdQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetProductByIdQueryResult>();
                return Results.Ok(response);
            })
            .WithName("GetProduct")
            .Produces<GetProductByIdQueryResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get product")
            .WithDescription("Get product");
        }
    }
}