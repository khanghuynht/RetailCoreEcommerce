using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Ok(new { isSuccess = true, data = result.Value })
            : BadRequest(new
            {
                isSuccess = false,
                error = new Error(result.Error?.Code ?? "Unknown", result.Error?.Message ?? "Unknown error")
            });
    }

    protected IActionResult FromResult(Result result)
    {
        return result.IsSuccess
            ? Ok()
            : BadRequest(new
            {
                isSuccess = false,
                error = new Error(result.Error?.Code ?? "Unknown", result.Error?.Message ?? "Unknown error")
            });
    }
}