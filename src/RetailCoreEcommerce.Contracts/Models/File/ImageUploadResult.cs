namespace RetailCoreEcommerce.Contracts.Models.File;

public sealed record ImageUploadResult(
    string Url,
    string PublicId,
    int Width,
    int Height,
    long Bytes);