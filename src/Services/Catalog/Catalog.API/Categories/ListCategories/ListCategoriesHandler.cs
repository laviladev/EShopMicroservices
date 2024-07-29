using BuildingBlocks.CQRS;
using Catalog.API.Data.PostgreSQL;
using Catalog.API.Models;

namespace Catalog.API.Categories.ListCategories
{
    // Listar conjunto de categorías
    public record ListCategoriesQuery(string? Name) : IQuery<ListCategoriesQueryResult>;
    public record ListCategoriesQueryResult(List<Category> Categories);

    // Obtener una categoría por id o por name
    public record GetCategoryQuery(string Identification) : IQuery<GetCategoryQueryResult>;
    public record GetCategoryQueryResult(Category Category);


    internal class ListCategoriesQueryHandler :
        IQueryHandler<ListCategoriesQuery, ListCategoriesQueryResult>,
        IQueryHandler<GetCategoryQuery, GetCategoryQueryResult>
    {
        private readonly DataBaseCommands _dataBaseCommands;

        public ListCategoriesQueryHandler(DataBaseCommands dataBaseCommands) {
            _dataBaseCommands = dataBaseCommands;
        }
        public async Task<ListCategoriesQueryResult> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
        {
            var sqlCommand = "SELECT * FROM Categories";
            if (!string.IsNullOrEmpty(query.Name)) {
                sqlCommand += $" WHERE (LOWER(Name) LIKE LOWER('%' || @Name || '%'))";
            }

            var sqlParameters = new Dictionary<string, object> {
                { "@Name", query.Name }
            };

            var result = await _dataBaseCommands.ReadQuery<Category>(sqlCommand, sqlParameters, ["Products"], "categories");

            return new ListCategoriesQueryResult(result);
        }

        public Task<GetCategoryQueryResult> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}