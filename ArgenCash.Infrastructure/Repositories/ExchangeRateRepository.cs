using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ArgenCashDbContext _context;

    public ExchangeRateRepository(ArgenCashDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExchangeRate exchangeRate)
    {
        await _context.ExchangeRates.AddAsync(exchangeRate);
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        return await _context.ExchangeRates
            .AsNoTracking()
            .SingleOrDefaultAsync(exchangeRate => exchangeRate.Id == id);
    }

    public async Task<ExchangeRate?> GetLatestAsync(string baseCurrency, string targetCurrency)
    {
        var normalizedBaseCurrency = baseCurrency.Trim().ToUpperInvariant();
        var normalizedTargetCurrency = targetCurrency.Trim().ToUpperInvariant();

        return await _context.ExchangeRates
            .AsNoTracking()
            .Where(exchangeRate =>
                exchangeRate.BaseCurrency == normalizedBaseCurrency &&
                exchangeRate.TargetCurrency == normalizedTargetCurrency)
            .OrderByDescending(exchangeRate => exchangeRate.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
