namespace RetailCoreEcommerce.Contracts.Models.Category;

public class GetPagedCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}