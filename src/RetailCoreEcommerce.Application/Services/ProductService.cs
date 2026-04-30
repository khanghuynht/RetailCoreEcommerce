using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Product;
using RetailCoreEcommerce.Contracts.Models.ProductImage;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

            // Validate category exists
            var category = await categoryRepo.AnyAsync(
                x => x.Id == request.CategoryId,
                cancellationToken: cancellationToken);

            if (category is false)
                return Result.Failure(new Error("Product.CategoryNotFound",
                    $"Category with ID {request.CategoryId} not found."));

            // Ensure SKU is unique
            var skuExists = await productRepo.AnyAsync(
                x => x.SKU == request.SKU, cancellationToken);

            if (skuExists)
                return Result.Failure(new Error("Product.DuplicateSKU",
                    $"Product with SKU '{request.SKU}' already exists."));

            var product = new Product
            {
                CategoryId = request.CategoryId,
                Title = request.Title.Trim(),
                SKU = request.SKU.Trim().ToUpperInvariant(),
                Name = request.Name.Trim(),
                OriginalPrice = request.OriginalPrice,
                SalePrice = request.SalePrice,
                Description = request.Description,
                Length = request.Length,
                Width = request.Width,
                Height = request.Height,
                IsActive = false,
                Inventory = new Inventory
                {
                    StockQuantity = request.InitialStock,
                    ReservedQuantity = 0,
                    SoldQuantity = 0
                }
            };

            productRepo.Add(product);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.CreateProductAsync",
                $"Error creating product: {ex.Message}"));
        }
    }

    public async Task<Result> UpdateProductAsync(
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();

            var product = await productRepo.FindByIdAsync(
                request.Id, tracking: true, cancellationToken: cancellationToken);

            if (product is null)
                return Result.Failure(new Error("Product.NotFound",
                    $"Product with ID {request.Id} not found."));

            // Validate new category if changed
            if (request.CategoryId.HasValue && request.CategoryId != product.CategoryId)
            {
                var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

                var category = await categoryRepo.FindByIdAsync(
                    request.CategoryId.Value, tracking: false, cancellationToken: cancellationToken);

                if (category is null)
                    return Result.Failure(new Error("Product.CategoryNotFound",
                        $"Category with ID {request.CategoryId} not found."));

                product.CategoryId = request.CategoryId.Value;
            }

            product.Title = request.Title?.Trim() ?? product.Title;
            product.Name = request.Name?.Trim() ?? product.Name;
            product.OriginalPrice = request.OriginalPrice ?? product.OriginalPrice;
            product.SalePrice = request.SalePrice ?? product.SalePrice;
            product.Description = request.Description ?? product.Description;
            product.Length = request.Length ?? product.Length;
            product.Width = request.Width ?? product.Width;
            product.Height = request.Height ?? product.Height;
            product.IsActive = request.IsActive ?? product.IsActive;

            productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.UpdateProductAsync",
                $"Error updating product: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();

            var product = await productRepo.FindByIdAsync(
                id, tracking: true, cancellationToken: cancellationToken);

            if (product is null)
                return Result.Failure(new Error("Product.NotFound",
                    $"Product with ID {id} not found."));

            productRepo.Delete(product);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductService.DeleteProductAsync",
                $"Error deleting product: {ex.Message}"));
        }
    }


    public async Task<Result<GetProductResponse>> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();

            var product = await productRepo.FindFirstAsync(
                x => x.Id == id,
                tracking: false,
                cancellationToken: cancellationToken,
                x => x.Category,
                x => x.Inventory,
                x => x.ProductImages!);

            if (product is null)
                return Result.Failure<GetProductResponse>(new Error("Product.NotFound",
                    $"Product with ID {id} not found."));

            var response = new GetProductResponse
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Title = product.Title,
                SKU = product.SKU,
                Name = product.Name,
                OriginalPrice = product.OriginalPrice,
                SalePrice = product.SalePrice,
                Description = product.Description,
                ThumbnailUrl = product.ThumbnailUrl,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                IsActive = product.IsActive,
                StockQuantity = product.Inventory?.StockQuantity ?? 0,
                ReservedQuantity = product.Inventory?.ReservedQuantity ?? 0,
                SoldQuantity = product.Inventory?.SoldQuantity ?? 0,
                Images = product.ProductImages?
                    .OrderBy(x => x.Position)
                    .Select(x => new ProductImageResponse
                    {
                        Id = x.Id,
                        ImageUrl = x.ImageUrl,
                        Name = x.Name,
                        Position = x.Position
                    })
                    .ToList() ?? []
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<GetProductResponse>(new Error("ProductService.GetProductByIdAsync",
                $"Error retrieving product: {ex.Message}"));
        }
    }

    public async Task<Result<PaginationResult<GetAllProductResponse>>> GetPagedProductsAsync(
        GetProductRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();

            var pagedResult = await productRepo.GetPagedAsync(
                predicate: x =>
                    (request.Name == null || x.Name.Contains(request.Name)) &&
                    (request.CategoryId == null || x.CategoryId == request.CategoryId) &&
                    (request.IsActive == null || x.IsActive == request.IsActive),
                orderBy: q => q.OrderBy(x => x.CreatedAt),
                pagination: request,
                cancellationToken: cancellationToken);

            var responses = pagedResult.Items.Select(p => new GetAllProductResponse
            {
                Id = p.Id,
                Title = p.Title,
                SKU = p.SKU,
                OriginalPrice = p.OriginalPrice,
                SalePrice = p.SalePrice,
                ThumbnailUrl = p.ThumbnailUrl,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId
            });

            return Result.Success(new PaginationResult<GetAllProductResponse>(
                responses,
                pagedResult.TotalItems,
                pagedResult.PageNumber,
                pagedResult.PageSize));
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginationResult<GetAllProductResponse>>(new Error(
                "ProductService.GetAllProductsAsync",
                $"Error retrieving products: {ex.Message}"));
        }
    }
}