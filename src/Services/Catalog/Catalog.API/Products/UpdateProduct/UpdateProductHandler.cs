using BuildingBlocks.CQRS;
using Catalog.API.Data.PostgreSQL;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using FluentValidation;

namespace Catalog.API.Products.UpdateProduct
{

    public record UpdateProductResult(bool IsSuccess);
    public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, string ImageFile, List<Guid>? Categories)
    : ICommand<UpdateProductResult>;

    public class CreateProductCommandValidator : AbstractValidator<UpdateProductCommand> {
        public CreateProductCommandValidator() {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product Id is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
                .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.ImageFile).NotEmpty().WithMessage("ImageFile is required");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            RuleFor(x => x.Price).NotEmpty().WithMessage("Price is required");
        }
    }
    public class UpdateProductHandler(DataBaseCommands dataBaseCommands): ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        private readonly DataBaseCommands _dataBaseCommands = dataBaseCommands;

        public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            string getSqlCommand = "SELECT * FROM products WHERE id = @Id";
            var getSqlParameters = new Dictionary<string, object> {
                { "@Id", request.Id }
            };
            var currentProduct = (await _dataBaseCommands.ReadQuery<Product>(getSqlCommand, getSqlParameters, ["Categories"], "products_categories"))
                .FirstOrDefault() ?? throw new ProductNotFoundException();

            // Obtener las categorías agregadas y eliminadas
            List<Guid> categoriesToAddIds = [];
            List<Guid> categoriesToRemoveIds = [];
            if (request.Categories != null) {
                if (request.Categories.Count > 0) {
                    var currentCategories = currentProduct.Categories?.Select(x => x.Id).ToList();
                    if (currentCategories != null) {
                        categoriesToAddIds = request.Categories.FindAll(x => !currentCategories.Contains(x));
                        categoriesToRemoveIds = currentCategories.FindAll(x => !request.Categories.Contains(x));
                    }
                } else {
                    categoriesToRemoveIds = currentProduct.Categories?.Select(x => x.Id).ToList() ?? [];
                }
                // Agregar categorías
                if (categoriesToAddIds.Count > 0) {
                    string addCategoriesSqlCommand = "INSERT INTO products_categories (product_id, category_id) VALUES";
                    var insertCategoriesSqlParameters = new Dictionary<string, object> {
                        { "@ProductIdd", request.Id },
                    };
                    for (int i = 0; i < categoriesToAddIds.Count; i++) {
                        addCategoriesSqlCommand += $" (@ProductIdd, @CategoryId{i}),";
                        insertCategoriesSqlParameters.Add($"@CategoryId{i}", categoriesToAddIds[i]);
                    }
                    addCategoriesSqlCommand = addCategoriesSqlCommand.TrimEnd(',');
                    bool resultInserCategories = await _dataBaseCommands.WriteCommand(addCategoriesSqlCommand, insertCategoriesSqlParameters);
                    if (!resultInserCategories) throw new Exception("Error al intentar agregar categorias al producto");
                }
                // Eliminar categorías
                if (categoriesToRemoveIds.Count > 0) {
                    // Crear los placeholders para los parámetros de las categorías
                    var categoryPlaceholders = string.Join(", ", categoriesToRemoveIds.Select((id, index) => $"@CategoryId{index}"));
                    
                    // Crear la consulta SQL
                    string removeCategoriesSqlCommand = $"DELETE FROM products_categories WHERE product_id = @ProductIdd AND category_id IN ({categoryPlaceholders})";
                    
                    // Crear los parámetros
                    var removeCategoriesSqlParameters = new Dictionary<string, object> {
                        { "@ProductIdd", request.Id }
                    };

                    // Añadir los parámetros para las categorías
                    for (int i = 0; i < categoriesToRemoveIds.Count; i++) {
                        removeCategoriesSqlParameters.Add($"@CategoryId{i}", categoriesToRemoveIds[i]);
                    }

                    // Ejecutar el comando
                    bool resultRemoveCategories = await _dataBaseCommands.WriteCommand(removeCategoriesSqlCommand, removeCategoriesSqlParameters);
                    
                    if (!resultRemoveCategories) throw new Exception("Error al intentar eliminar categorias del producto");
                }
            }
            string updateSqlCommand = "UPDATE products SET name = @Name, description = @Description, price = @Price, imageFile = @ImageFile WHERE id = @Id";
            Dictionary<string, object> sqlParameters = new()
            {
                { "@Name", request.Name },
                { "@Description", request.Description },
                { "@Price", request.Price },
                { "@ImageFile", request.ImageFile },
                { "@Id", request.Id }
            };
            var result = await _dataBaseCommands.WriteCommand(updateSqlCommand, sqlParameters);
            return new UpdateProductResult(result);
        }
    }
}