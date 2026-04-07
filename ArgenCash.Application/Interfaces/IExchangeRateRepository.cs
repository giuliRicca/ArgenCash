using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task AddAsync(ExchangeRate exchangeRate);
    Task<ExchangeRate?> GetByIdAsync(Guid id);
    Task<ExchangeRate?> GetLatestAsync(string baseCurrency, string targetCurrency);
    Task SaveChangesAsync();
}
