namespace RetailCoreEcommerce.StoreFront.Models;

/// <summary>
/// Mirrors the backend's Error value object.
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Wrapper that matches the backend's { isSuccess, data, error } envelope.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
}
