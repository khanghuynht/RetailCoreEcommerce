namespace RetailCoreEcommerce.Contracts.Shared;

public sealed record ImageUploadResult(
    string Url,
    string PublicId,
    int Width,
    int Height,
    long Bytes);