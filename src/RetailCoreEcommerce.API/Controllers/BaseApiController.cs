using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new Result<T>(value: result.Value, isSuccess: true, error: null));
        }

        var errorCode = result.Error?.Code ?? "Unknown";
        var errorMessage = result.Error?.Message ?? "Unknown error";
        return BadRequest(new Result(isSuccess: false, error: new Error(errorCode, errorMessage)));
    }

    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(new Result(isSuccess: true, error: null));
        }

        var errorCode = result.Error?.Code ?? "Unknown";
        var errorMessage = result.Error?.Message ?? "Unknown error";
        return BadRequest(new Result(isSuccess: false, error: new Error(errorCode, errorMessage)));
    }
}