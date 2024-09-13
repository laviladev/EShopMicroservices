using BuildingBlocks.CQRS;
using Catalog.API.Data.PostgreSQL;
using Catalog.API.Exceptions;
using Catalog.API.Models;

namespace Catalog.API.Products.GetProdutcs
{
    public record GetProductsResult(List<Product> Products);
    public record GetProductsRequest (string? Name, List<Guid>? Categories, string? Description, decimal? MinPrice, decimal? MaxPrice)
        : IQuery<GetProductsResult>;

    public record GetProductByIdQueryResult(Product Product);
    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResult>;

    public record GetProductsByCategoryResult(List<Product> Products);
    public record GetProductsByCategoryRequest(string Name) : IQuery<GetProductsByCategoryResult>;

    public class GetProductsHandler(DataBaseCommands dataBaseCommands):
        IQueryHandler<GetProductsRequest, GetProductsResult>,
        IQueryHandler<GetProductByIdQuery, GetProductByIdQueryResult>,
        IQueryHandler<GetProductsByCategoryRequest, GetProductsByCategoryResult>
    {
        private readonly DataBaseCommands _dataBaseCommands = dataBaseCommands;

        public async Task<GetProductsResult> Handle(GetProductsRequest request, CancellationToken cancellationToken)
        {
            Dictionary<string, object> namesParameters = [];
            // Consultar productos por nombre, descripción o precio
            string sqlCommand = "SELECT * FROM products WHERE ";
            if (request.Name != null)
            {
                sqlCommand += $"LOWER(name) LIKE LOWER('%' || @Name || '%') AND ";
                namesParameters.Add("@Name", request.Name);
            }
            if (request.Description != null)
            {
                sqlCommand += $"LOWER(description) LIKE LOWER('%' || @Description || '%') AND ";
                namesParameters.Add("@Description", request.Description);
            }
            if (request.MinPrice != null)
            {
                sqlCommand += $"price >= @MinPrice AND ";
                namesParameters.Add("@MinPrice", request.MinPrice);
            }
            if (request.MaxPrice != null)
            {
                sqlCommand += $"price <= @MaxPrice AND ";
                namesParameters.Add("@MaxPrice", request.MaxPrice);
            }
            sqlCommand = sqlCommand.TrimEnd(' ', 'A', 'N', 'D', 'W', 'H', 'E', 'R');

            var products = await _dataBaseCommands.ReadQuery<Product>(sqlCommand, namesParameters, ["Categories"], "products_categories");

            return new GetProductsResult(products);
        }

        public async Task<GetProductByIdQueryResult> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                throw new ProductNotFoundException();
            }

            string sqlCommand = "SELECT * FROM products WHERE id = @Id";
            var sqlParameters = new Dictionary<string, object> {
                { "@Id", request.Id }
            };
            var result = (await _dataBaseCommands.ReadQuery<Product>(sqlCommand, sqlParameters, ["Categories"], "products_categories")).FirstOrDefault() ?? throw new ProductNotFoundException();
            return new GetProductByIdQueryResult(result);
        }

        public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new Exception("Name is required");
            }

            string sqlCommand = @"SELECT p.*
                        FROM products p
                        JOIN products_categories pc ON p.id = pc.product_id
                        JOIN categories c ON c.id = pc.category_id
                        WHERE c.name = @Name;";

            var sqlParameters = new Dictionary<string, object> {
                { "@Name", request.Name }
            };
            var result = await _dataBaseCommands.ReadQuery<Product>(sqlCommand, sqlParameters);
            return new GetProductsByCategoryResult(result);
        }
    }
}