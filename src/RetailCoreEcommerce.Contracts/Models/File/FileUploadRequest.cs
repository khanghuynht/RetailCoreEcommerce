namespace RetailCoreEcommerce.Contracts.Models.File;

public sealed record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Size)
{
    // Ensure the stream is properly disposed of when the FileUploadRequest is no longer needed.
    public async ValueTask DisposeAsync() => await Content.DisposeAsync();
}