using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Constants;
using RetailCoreEcommerce.Contracts.Models.User;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/users")]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("customers")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] GetPagedUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.GetPagedCustomersAsync(request, cancellationToken);
        return FromResult(result);
    }
}