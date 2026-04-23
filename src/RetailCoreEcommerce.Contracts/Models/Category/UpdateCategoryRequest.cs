using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RetailCoreEcommerce.Contracts.Models.Category;

public class UpdateCategoryRequest
{
    [JsonIgnore] public Guid Id { get; set; }
    [MaxLength(100)] public string? Name { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public string? Description { get; set; }
}