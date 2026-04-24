namespace RetailCoreEcommerce.Contracts.Shared;

public sealed record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Size);