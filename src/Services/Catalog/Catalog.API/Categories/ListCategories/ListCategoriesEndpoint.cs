using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Categories.ListCategories
{

    public class ListCategoriesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapGet("/categories", async ([FromBody] ListCategoriesQuery? request, ISender sender) => {
                ListCategoriesQuery query;
                if (request == null) {
                    query = new ListCategoriesQuery(null);
                } else {
                    query = request.Adapt<ListCategoriesQuery>();
                }
                var result = await sender.Send(query);

                var response = result.Adapt<ListCategoriesQueryResult>();

                return Results.Ok(response);
            })
            .WithName("ListCategories")
            .Produces<ListCategoriesQueryResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("List categories by name");
        }
    }
}