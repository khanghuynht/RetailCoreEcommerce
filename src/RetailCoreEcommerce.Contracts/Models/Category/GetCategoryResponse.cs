namespace RetailCoreEcommerce.Contracts.Models.Category;

public class GetCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int ChildrenCount { get; set; }
    public int ProductsCount { get; set; }
}