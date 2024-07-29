using BuildingBlocks.CQRS;
using Catalog.API.Data.PostgreSQL;
using Catalog.API.Models;

namespace Catalog.API.Products.CreateProduct
{
    public record CreateProductCommand(string Name, List<string> Categories, string Description, string ImageFile, decimal Price)
        : ICommand<CreateProductResult>;
    public record CreateProductResult(Guid Id);


    internal class CreateProductCommandHandler(DataBaseCommands dataBaseCommands) : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        private readonly DataBaseCommands _dataBaseCommands = dataBaseCommands;

        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            // Validar categorías
            var parameterNames = new List<string>();
            Dictionary<string, object> namesParameters = [];
            for (int i = 0; i < command.Categories.Count; i++)
            {
                parameterNames.Add($"@name{i}");
                namesParameters.Add($"name{i}", command.Categories[i]);
            }

            string sqlCommand1 = $"SELECT * FROM categories WHERE Name IN ({string.Join(", ", parameterNames)})";

            var coincidences = await _dataBaseCommands.ReadQuery<Category>(sqlCommand1, namesParameters, []);

            if (coincidences.Count != command.Categories.Count)
            {
                throw new Exception("One or more categories do not exist");
            }

            var product = new Product {
                Name = command.Name,
                Description = command.Description,
                ImageFile = command.ImageFile,
                Price = command.Price,
                Id = Guid.NewGuid()
            };
            // Obtener el tipo de la clase Product
            Type productType = product.GetType();
            // Obtener todas las propiedades de la clase Product
            // Obtener los nombres de las propiedades como un array de strings
            string[] propertyNames = productType.GetProperties().Select(p => p.Name).ToArray().Where(p => p != "Categories").ToArray();
            var propertyValues = productType.GetProperties().ToDictionary(p => p.Name, p => p.GetValue(product));

            // Construir la consulta SQL dinámica
            var columns = string.Join(", ", propertyNames);
            var parameters = string.Join(", ", propertyNames.Select(p => $"@{p.ToLower()}"));
            var sqlCommand2 = $"INSERT INTO products ({columns}) VALUES ({parameters})";

            // Construir el diccionario de parámetros para la consulta SQL
            var sqlParameters = propertyNames.ToDictionary(
                p => $"@{p.ToLower()}",
                p => propertyValues[p] ?? DBNull.Value
            );

            var result = await _dataBaseCommands.WriteCommand(sqlCommand2, sqlParameters);

            if (!result) return new CreateProductResult(Guid.Empty);

            // Crear relación entre las categorias y el producto usando sus ids
            Guid[] categoryIds = coincidences.Select(c => c.Id).ToArray();
            string sqlCommand3 = "INSERT INTO products_categories (product_id, category_id) VALUES ";

            var values = new List<string>();
            var sqlParameters2 = new Dictionary<string, object>
            {
                { "@productId", product.Id }
            };

            for (int i = 0; i < categoryIds.Length; i++)
            {
                string parameterName = $"@categoryId{i}";
                values.Add($"(@productId, {parameterName})");
                sqlParameters2.Add(parameterName, categoryIds[i]);
            }

            sqlCommand3 += string.Join(", ", values);
            await _dataBaseCommands.WriteCommand(sqlCommand3, sqlParameters2);

            return new CreateProductResult(product.Id);
        }
    }
}