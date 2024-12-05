using BuildingBlocks.CQRS;
using Catalog.API.Data.PostgreSQL;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using FluentValidation;

namespace Catalog.API.Products.DeactivateProduct
{
  public record DeactivateProductResult(bool IsSuccess);
  public record DeactivateProductCommand(Guid Id) : ICommand<DeactivateProductResult>;

      public class CreateProductCommandValidator : AbstractValidator<DeactivateProductCommand> {
        public CreateProductCommandValidator() {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product Id is required");
        }
    }
  public class DeactivateProductHandler(DataBaseCommands dataBaseCommands) : ICommandHandler<DeactivateProductCommand, DeactivateProductResult> {
    private readonly DataBaseCommands _dataBaseCommands = dataBaseCommands;

        public async Task<DeactivateProductResult> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            // Validar que el producto exista
            string getSqlCommand = "SELECT id FROM products WHERE id = @Id";
            var getSqlParameters = new Dictionary<string, object> {
                { "@Id", request.Id }
            };
            var product = (await _dataBaseCommands.ReadQuery<Product>(getSqlCommand, getSqlParameters)).FirstOrDefault() ?? throw new ProductNotFoundException();

            // Desactivar el producto
            string updateSqlCommand = "UPDATE products SET active = false WHERE id = @Id";
            var updateSqlParameters = new Dictionary<string, object> {
                { "@Id", request.Id }
            };
            return await _dataBaseCommands.WriteCommand(updateSqlCommand, updateSqlParameters).ContinueWith(x => new DeactivateProductResult(true));
        }
    }

}