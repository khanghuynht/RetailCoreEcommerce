using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Category;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;

namespace RetailCoreEcommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateCategoryAsync(CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

            // Assume maximum depth of category hierarchy is 2 (root -> subcategory)
            // temporary hardcoded, can be moved to config if needed
            const int maxDepth = 2;

            if (request.ParentId.HasValue)
            {
                var parent = await categoryRepo.FindFirstAsync(
                    x => x.Id == request.ParentId.Value,
                    tracking: false,
                    cancellationToken: cancellationToken);

                // Validate parent category exists and does not exceed maximum depth
                if (parent is null)
                {
                    return Result.Failure(new Error(
                        "Category.ParentNotFound",
                        $"Parent category with ID {request.ParentId} not found"));
                }

                if (parent.ParentId is not null)
                {
                    return Result.Failure(new Error(
                        "Category.MaxDepthExceeded",
                        $"Cannot create category: Maximum depth of {maxDepth} levels reached"));
                }
            }

            // Check for duplicate category name under the same parent
            var nameExists = await categoryRepo.AnyAsync(
                x => x.Name == request.Name && x.ParentId == request.ParentId,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure(new Error(
                    "Category.DuplicateName",
                    $"Category with name '{request.Name}' already exists under the same parent"));
            }

            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                ParentId = request.ParentId
            };

            categoryRepo.Add(category);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("CategoryService.CreateCategoryAsync",
                $"Error creating category: {ex.Message}"));
        }
    }

    public async Task<Result> UpdateCategoryAsync(UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

            const int maxDepth = 2;

            var category = await categoryRepo.FindByIdAsync(
                request.Id,
                tracking: true,
                cancellationToken: cancellationToken);

            if (category is null)
            {
                return Result.Failure(new Error(
                    "Category.NotFound",
                    $"Category with ID {request.Id} not found"));
            }

            // cannot be its own parent
            if (request.ParentId == category.Id)
            {
                return Result.Failure(new Error(
                    "Category.InvalidParent",
                    "Category cannot be its own parent"));
            }

            if (request.ParentId.HasValue)
            {
                var parent = await categoryRepo.FindFirstAsync(
                    x => x.Id == request.ParentId.Value,
                    tracking: false,
                    cancellationToken: cancellationToken);

                if (parent is null)
                {
                    return Result.Failure(new Error(
                        "Category.ParentNotFound",
                        $"Parent category with ID {request.ParentId} not found"));
                }

                if (parent.ParentId is not null)
                {
                    return Result.Failure(new Error(
                        "Category.MaxDepthExceeded",
                        $"Cannot update category: Maximum depth of {maxDepth} levels reached"));
                }

                if (await WouldCreateCircularReferenceAsync(category.Id, request.ParentId.Value, cancellationToken))
                {
                    return Result.Failure(new Error(
                        "Category.CircularReference",
                        "Cannot create circular reference in category hierarchy"));
                }
            }

            // duplicate name under same parent (exclude self)
            var nameExists = await categoryRepo.AnyAsync(
                x => x.Id != category.Id
                     && x.Name == request.Name
                     && x.ParentId == request.ParentId,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure(new Error(
                    "Category.DuplicateName",
                    $"Category with name '{request.Name}' already exists under the same parent"));
            }

            category.Name = request.Name?.Trim() ?? category.Name;
            category.Description = request.Description ?? category.Description;
            category.ParentId = request.ParentId;

            categoryRepo.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(
                "CategoryService.UpdateCategoryAsync",
                $"Error updating category: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

            var category = await categoryRepo.FindByIdAsync(
                id,
                tracking: true,
                cancellationToken: cancellationToken,
                x => x.Children,
                x => x.Products);

            if (category is null)
            {
                return Result.Failure(new Error(
                    "Category.NotFound",
                    $"Category with ID {id} not found"));
            }

            if (category.Children.Any())
            {
                return Result.Failure(new Error(
                    "Category.HasChildren",
                    "Cannot delete category that has child categories"));
            }

            if (category.Products.Any())
            {
                return Result.Failure(new Error(
                    "Category.HasProducts",
                    "Cannot delete category that has associated products"));
            }

            categoryRepo.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(
                "CategoryService.DeleteCategoryAsync",
                $"Error deleting category: {ex.Message}"));
        }
    }

    public async Task<Result<GetCategoryResponse>> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();
            var category = await categoryRepo.FindFirstAsync(
                x => x.Id == id,
                tracking: false,
                cancellationToken: cancellationToken,
                x => x.Parent!,
                x => x.Children);

            if (category is null)
            {
                return Result.Failure<GetCategoryResponse>(new Error(
                    "Category.NotFound",
                    $"Category with ID {id} not found"));
            }

            var response = new GetCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                ParentName = category.Parent?.Name,
                ChildrenCount = category.Children.Count,
            };
            
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<GetCategoryResponse>(new Error(
                "CategoryService.GetCategoryByIdAsync",
                $"Error retrieving category: {ex.Message}"));
        }
    }

    public async Task<Result<PaginationResult<GetAllCategoryResponse>>> GetAllCategoriesAsync(
        GetAllCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

            var pagedResult = await categoryRepo.GetPagedAsync(
                predicate: x =>
                    (request.Name == null || x.Name.Contains(request.Name)) &&
                    (request.ParentId == null || x.ParentId == request.ParentId) &&
                    (request.IsRootOnly == null || !request.IsRootOnly.Value || x.ParentId == null),
                orderBy: q => q.OrderBy(x => x.CreatedAt),
                pagination: request, // PaginationParams is the base
                cancellationToken: cancellationToken);

            var responses = pagedResult.Items.Select(category => new GetAllCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
            });

            var result = new PaginationResult<GetAllCategoryResponse>(
                responses,
                pagedResult.TotalItems,
                pagedResult.PageNumber,
                pagedResult.PageSize);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginationResult<GetAllCategoryResponse>>(new Error(
                "CategoryService.GetAllCategoriesAsync",
                $"Error retrieving categories: {ex.Message}"));
        }
    }


    private async Task<bool> WouldCreateCircularReferenceAsync(
        Guid categoryId,
        Guid newParentId,
        CancellationToken cancellationToken = default)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();
        Guid? currentParentId = newParentId;
        while (currentParentId is not null)
        {
            if (currentParentId == categoryId)
            {
                return true;
            }

            var parent = await categoryRepo.FindByIdAsync(
                currentParentId.Value,
                tracking: false,
                cancellationToken: cancellationToken);
            if (parent is null)
            {
                break;
            }

            currentParentId = parent.ParentId;
        }

        return false;
    }
}