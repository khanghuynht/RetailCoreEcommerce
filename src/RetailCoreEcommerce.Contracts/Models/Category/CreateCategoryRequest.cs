using System.ComponentModel.DataAnnotations;

namespace RetailCoreEcommerce.Contracts.Models.Category;

public class CreateCategoryRequest
{
    [Required] [MaxLength(100)] public string Name { get; set; } = null!;

    public Guid? ParentId { get; set; }
    public string? Description { get; set; }
}