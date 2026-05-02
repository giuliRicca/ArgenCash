using ArgenCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ArgenCash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AssistantController : ApiControllerBase
{
    private readonly IAssistantService _assistantService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public AssistantController(IAssistantService assistantService, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _assistantService = assistantService;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    [HttpPost("transaction-draft")]
    public async Task<IActionResult> CreateTransactionDraft([FromBody] TransactionDraftRequest request, CancellationToken cancellationToken)
    {
        if (!IsAssistantEnabled())
        {
            return NotFound();
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (!TryConsumeRateLimit(userId, out var message))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { detail = message });
        }

        var response = await _assistantService.CreateTransactionDraftAsync(userId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request, CancellationToken cancellationToken)
    {
        if (!IsAssistantEnabled())
        {
            return NotFound();
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (!TryConsumeRateLimit(userId, out var message))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { detail = message });
        }

        var response = await _assistantService.HandleChatAsync(userId, request, cancellationToken);
        return Ok(response);
    }

    private bool IsAssistantEnabled()
    {
        return _configuration.GetValue("Assistant:Enabled", false);
    }

    private bool TryConsumeRateLimit(Guid userId, out string message)
    {
        var now = DateTimeOffset.UtcNow;
        var minuteKey = $"assistant-rate:{userId}:minute:{now:yyyyMMddHHmm}";
        var dayKey = $"assistant-rate:{userId}:day:{now:yyyyMMdd}";

        var minuteCount = _memoryCache.Get<int>(minuteKey);
        if (minuteCount >= 5)
        {
            message = "Llegaste al límite por minuto de IA. Podés cargar la transacción manualmente.";
            return false;
        }

        var dayCount = _memoryCache.Get<int>(dayKey);
        if (dayCount >= 30)
        {
            message = "Llegaste al límite diario de IA. Podés cargar la transacción manualmente.";
            return false;
        }

        _memoryCache.Set(minuteKey, minuteCount + 1, now.AddMinutes(1));
        _memoryCache.Set(dayKey, dayCount + 1, now.Date.AddDays(1));
        message = string.Empty;
        return true;
    }
}
