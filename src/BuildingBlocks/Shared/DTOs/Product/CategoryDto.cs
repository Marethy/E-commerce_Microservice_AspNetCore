using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<ProductDto>? Products { get; set; }
        // Hierarchy properties
        public Guid? ParentId { get; set; }
        public int Level { get; set; }
        public bool HasChildren { get; set; }
        public ICollection<CategoryDto>? Children { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
