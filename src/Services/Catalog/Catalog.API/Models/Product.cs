namespace Catalog.API.Models;

    public class Product
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = default!;
        
        public string Description { get; set; } = default!;
        
        public List<Category>? Categories { get; set; } = [];
        
        public string ImageFile { get; set; } = default!;
        
        public decimal Price { get; set; }

        public bool Active { get; set; } = true;
    }
