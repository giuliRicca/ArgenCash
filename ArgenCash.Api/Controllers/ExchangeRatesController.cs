using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpPost("manual")]
    public async Task<IActionResult> CreateManualRate([FromBody] CreateExchangeRateRequest request)
    {
        try
        {
            var rateId = await _exchangeRateService.CreateManualRateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = rateId }, new { id = rateId });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate exchange rate.",
                Detail = "An exchange rate of this type already exists for this exact timestamp.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var exchangeRate = await _exchangeRateService.GetByIdAsync(id);

        return exchangeRate is null ? NotFound() : Ok(exchangeRate);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] string baseCurrency = "USD", [FromQuery] string targetCurrency = "ARS")
    {
        try
        {
            var exchangeRate = await _exchangeRateService.GetLatestAsync(baseCurrency, targetCurrency);
            return exchangeRate is null ? NotFound() : Ok(exchangeRate);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

    [HttpGet("live")]
    public async Task<IActionResult> GetLive([FromQuery] string baseCurrency = "USD", [FromQuery] string targetCurrency = "ARS", [FromQuery] ExchangeRateType rateType = ExchangeRateType.Official, CancellationToken cancellationToken = default)
    {
        try
        {
            var exchangeRate = await _exchangeRateService.GetLiveRateAsync(baseCurrency, targetCurrency, rateType, cancellationToken);
            return Ok(exchangeRate);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Exchange-rate provider failed.", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
        catch (HttpRequestException ex)
        {
            return Problem(title: "Exchange-rate provider failed.", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpGet("live/batch")]
    public async Task<IActionResult> GetLiveBatch(
        [FromQuery] string baseCurrency = "USD",
        [FromQuery] string targetCurrency = "ARS",
        [FromQuery] ExchangeRateType[]? rateTypes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestedRateTypes = rateTypes is { Length: > 0 }
                ? rateTypes
                : [ExchangeRateType.Official, ExchangeRateType.Blue, ExchangeRateType.Ccl, ExchangeRateType.Crypto];

            var rates = await _exchangeRateService.GetLiveRatesAsync(baseCurrency, targetCurrency, requestedRateTypes, cancellationToken);
            return Ok(rates);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Exchange-rate provider failed.", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
        catch (HttpRequestException ex)
        {
            return Problem(title: "Exchange-rate provider failed.", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
