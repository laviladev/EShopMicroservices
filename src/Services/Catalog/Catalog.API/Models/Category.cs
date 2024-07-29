namespace Catalog.API.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;

        public List<Product> Products { get; set; } = [];
    }
}