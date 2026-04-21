using ArgenCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgenCash.Api.Controllers;

/// <summary>
/// Processes due credit card statement settlements.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CreditSettlementsController(ICreditCardSettlementService creditCardSettlementService) : ApiControllerBase
{
    private readonly ICreditCardSettlementService _creditCardSettlementService =
        creditCardSettlementService ?? throw new ArgumentNullException(nameof(creditCardSettlementService));

    /// <summary>
    /// Processes due settlements for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processing summary.</returns>
    [HttpPost("process-due")]
    public async Task<IActionResult> ProcessDue(CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _creditCardSettlementService.ProcessDueSettlementsAsync(userId, cancellationToken);
        return Ok(result);
    }
}
