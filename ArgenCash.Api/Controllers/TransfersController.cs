using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgenCash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransfersController : ApiControllerBase
{
    private readonly IAccountService _accountService;

    public TransfersController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var transferGroupId = await _accountService.CreateTransferAsync(userId, request, cancellationToken);
            return Created($"/api/transfers/{transferGroupId}", new { transferGroupId });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

}
