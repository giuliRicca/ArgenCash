using ArgenCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgenCash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController : ApiControllerBase
{
    private readonly IAccountService _accountService;

    public TransactionsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var transactionId = await _accountService.CreateTransactionAsync(userId, request);
            return Created("", new { id = transactionId });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var transactions = await _accountService.GetRecentTransactionsAsync(userId, limit, cancellationToken);
        return Ok(transactions);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _accountService.DeleteTransactionAsync(id, userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _accountService.UpdateTransactionAsync(id, userId, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

}
