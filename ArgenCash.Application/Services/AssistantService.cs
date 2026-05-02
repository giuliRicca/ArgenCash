using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public partial class AssistantService : IAssistantService
{
    private static readonly string[] TransferWords = ["transferi", "transferí", "pase", "pasé", "mande", "mandé"];
    private static readonly string[] RecurringWords = ["todos los meses", "cada mes", "mensual", "recurrente"];
    private static readonly string[] InstallmentWords = ["cuota", "cuotas"];
    private static readonly string[] IncomeWords = ["cobre", "cobré", "sueldo", "depositaron", "ingreso", "entre", "entró", "entro", "honorarios", "freelance"];
    private static readonly string[] ExpenseWords = ["gaste", "gasté", "pague", "pagué", "compre", "compré", "compra", "pago"];

    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILearnedCategoryMappingRepository _learnedCategoryMappingRepository;
    private readonly IAssistantPreferencesRepository _assistantPreferencesRepository;
    private readonly IAccountService _accountService;

    public AssistantService(
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        ILearnedCategoryMappingRepository learnedCategoryMappingRepository,
        IAssistantPreferencesRepository assistantPreferencesRepository,
        IAccountService accountService)
    {
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _learnedCategoryMappingRepository = learnedCategoryMappingRepository;
        _assistantPreferencesRepository = assistantPreferencesRepository;
        _accountService = accountService;
    }

    public async Task<AssistantDraftResponse> CreateTransactionDraftAsync(Guid userId, TransactionDraftRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var text = request.Text.Trim();
        if (string.IsNullOrWhiteSpace(text) && request.PreviousDraft is null)
        {
            return Unsupported("Decime qué gasto o ingreso querés registrar.");
        }

        var normalizedText = NormalizeText(text);
        var accounts = (await _accountRepository.GetAllAsync(userId, cancellationToken)).ToList();
        var categories = await _categoryRepository.GetVisibleForUserAsync(userId, cancellationToken);
        var draft = CloneDraft(request.PreviousDraft) ?? new AssistantTransactionDraftDto
        {
            Currency = "ARS",
            WasCurrencyDefaulted = true,
            TransactionDate = DateTime.UtcNow.Date.AddHours(12)
        };

        if (ContainsAny(normalizedText, RecurringWords))
        {
            return Unsupported("Los pagos recurrentes todavía no están soportados desde el asistente.");
        }

        if (ContainsAny(normalizedText, InstallmentWords))
        {
            return Unsupported("Las cuotas todavía no están soportadas desde el asistente. Podés registrar el gasto completo de una vez.");
        }

        if (ContainsAny(normalizedText, TransferWords))
        {
            return Unsupported("Las transferencias todavía no están soportadas desde el asistente.");
        }

        if (normalizedText.Contains("pague la visa") || normalizedText.Contains("pagué la visa"))
        {
            return Unsupported("Registrar pagos de resumen todavía no está soportado desde el asistente. Puedo registrar gastos hechos con tarjeta.");
        }

        ApplyType(draft, normalizedText);
        ApplyAmountAndCurrency(draft, normalizedText);
        ApplyDate(draft, normalizedText);
        ApplyAccount(draft, normalizedText, accounts);
        ApplyDescriptionAndLearningKey(draft, normalizedText);
        await ApplyCategoryAsync(userId, draft, categories, cancellationToken);

        var followUp = BuildFollowUp(draft, accounts, categories);
        var warnings = await BuildWarningsAsync(userId, draft, accounts, cancellationToken);

        return new AssistantDraftResponse
        {
            State = followUp is null ? "ready_to_confirm" : "needs_followup",
            Draft = draft,
            FollowUp = followUp,
            Warnings = warnings
        };
    }

    public async Task<AssistantChatResponse> HandleChatAsync(Guid userId, AssistantChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var text = request.Text.Trim();
        if (string.IsNullOrWhiteSpace(text) && request.PreviousDraft is null)
        {
            return ChatUnsupported("Decime qué gasto o ingreso querés registrar.");
        }

        if (!LooksLikeTransactionIntent(text, request.PreviousDraft))
        {
            return ChatUnsupported("Todavía no puedo responder preguntas de finanzas. Por ahora puedo registrar gastos e ingresos.");
        }

        var normalizedText = NormalizeText(text);
        if (ContainsAny(normalizedText, RecurringWords))
        {
            return ChatUnsupported("Los pagos recurrentes todavía no están soportados desde el asistente.");
        }

        if (ContainsAny(normalizedText, InstallmentWords))
        {
            return ChatUnsupported("Las cuotas todavía no están soportadas desde el asistente. Podés registrar el gasto completo de una vez.");
        }

        if (ContainsAny(normalizedText, TransferWords))
        {
            return ChatUnsupported("Las transferencias todavía no están soportadas desde el asistente.");
        }

        if (normalizedText.Contains("pague la visa") || normalizedText.Contains("pagué la visa"))
        {
            return ChatUnsupported("Registrar pagos de resumen todavía no está soportado desde el asistente. Puedo registrar gastos hechos con tarjeta.");
        }

        var accounts = (await _accountRepository.GetAllAsync(userId, cancellationToken)).ToList();
        var categories = await _categoryRepository.GetVisibleForUserAsync(userId, cancellationToken);
        var draft = CloneDraft(request.PreviousDraft) ?? new AssistantTransactionDraftDto
        {
            Currency = "ARS",
            WasCurrencyDefaulted = true,
            TransactionDate = DateTime.UtcNow.Date.AddHours(12)
        };

        ApplyTypeForChat(draft, normalizedText);
        ApplyAmountAndCurrency(draft, normalizedText);
        ApplyDate(draft, normalizedText);
        ApplyAccount(draft, normalizedText, accounts);
        ApplyDescriptionAndLearningKey(draft, normalizedText);
        await ApplyCategoryAsync(userId, draft, categories, cancellationToken);
        await ApplyDefaultAccountAsync(userId, draft, cancellationToken);

        var followUp = BuildChatFollowUp(draft, accounts);
        if (followUp is not null)
        {
            return new AssistantChatResponse
            {
                Type = AssistantChatResponseTypes.NeedsFollowUp,
                Message = followUp.Question,
                Draft = draft,
                FollowUp = followUp
            };
        }

        var warnings = await BuildWarningsAsync(userId, draft, accounts, cancellationToken);
        if (warnings.Any(warning => warning.Contains("duplicado", StringComparison.OrdinalIgnoreCase)) &&
            !string.Equals(request.Action, AssistantChatActions.SaveAnyway, StringComparison.OrdinalIgnoreCase))
        {
            return new AssistantChatResponse
            {
                Type = AssistantChatResponseTypes.DuplicateWarning,
                Message = "Posible duplicado. ¿Querés guardarlo igual?",
                Draft = draft,
                TransactionPreview = BuildPreview(draft, accounts, categories),
                Warnings = warnings
            };
        }

        var transactionId = await _accountService.CreateTransactionAsync(
            userId,
            new CreateTransactionRequest
            {
                AccountId = draft.AccountId!.Value,
                Amount = draft.Amount!.Value,
                Currency = draft.Currency,
                TransactionType = draft.TransactionType!,
                Description = draft.Description,
                CategoryId = draft.CategoryId,
                TransactionDate = draft.TransactionDate,
                Source = TransactionSources.AssistantText,
                IgnoreDuplicateWarning = true,
                AssistantRawInput = text,
            },
            cancellationToken);

        return new AssistantChatResponse
        {
            Type = AssistantChatResponseTypes.TransactionSaved,
            Message = "Transacción guardada.",
            Transaction = BuildSavedTransaction(transactionId, draft, accounts, categories),
            Warnings = warnings
        };
    }

    private static AssistantChatResponse ChatUnsupported(string message)
    {
        return new AssistantChatResponse
        {
            Type = AssistantChatResponseTypes.Unsupported,
            Message = message
        };
    }

    private static AssistantDraftResponse Unsupported(string message)
    {
        return new AssistantDraftResponse
        {
            State = "unsupported",
            Message = message
        };
    }

    private static AssistantTransactionDraftDto? CloneDraft(AssistantTransactionDraftDto? draft)
    {
        if (draft is null)
        {
            return null;
        }

        return new AssistantTransactionDraftDto
        {
            AccountId = draft.AccountId,
            Amount = draft.Amount,
            Currency = draft.Currency,
            WasCurrencyDefaulted = draft.WasCurrencyDefaulted,
            TransactionType = draft.TransactionType,
            Description = draft.Description,
            CategoryId = draft.CategoryId,
            CategorySkipped = draft.CategorySkipped,
            TransactionDate = draft.TransactionDate,
            LearningKey = draft.LearningKey,
            SuggestedCategoryId = draft.SuggestedCategoryId
        };
    }

    private static void ApplyType(AssistantTransactionDraftDto draft, string text)
    {
        if (ContainsAny(text, IncomeWords))
        {
            draft.TransactionType = TransactionTypes.Income;
            return;
        }

        if (ContainsAny(text, ExpenseWords))
        {
            draft.TransactionType = TransactionTypes.Expense;
        }
    }

    private static void ApplyTypeForChat(AssistantTransactionDraftDto draft, string text)
    {
        if (text.TrimStart().StartsWith('+') || ContainsAny(text, IncomeWords))
        {
            draft.TransactionType = TransactionTypes.Income;
            return;
        }

        if (text.TrimStart().StartsWith('-') || ContainsAny(text, ExpenseWords))
        {
            draft.TransactionType = TransactionTypes.Expense;
            return;
        }

        draft.TransactionType ??= TransactionTypes.Expense;
    }

    private async Task ApplyDefaultAccountAsync(Guid userId, AssistantTransactionDraftDto draft, CancellationToken cancellationToken)
    {
        if (draft.AccountId.HasValue || string.IsNullOrWhiteSpace(draft.TransactionType))
        {
            return;
        }

        var preferences = await _assistantPreferencesRepository.GetByUserIdAsync(userId, cancellationToken);
        var transactionType = TransactionTypes.ToEnum(draft.TransactionType);
        draft.AccountId = preferences?.GetDefaultAccountId(transactionType);
    }

    private static void ApplyAmountAndCurrency(AssistantTransactionDraftDto draft, string text)
    {
        var match = AmountRegex().Match(text);
        if (!match.Success)
        {
            return;
        }

        var rawAmount = match.Groups["amount"].Value;
        var multiplierWord = match.Groups["multiplier"].Value;
        var currencyWord = match.Groups["currency"].Value;

        if (!TryParseArgentineAmount(rawAmount, out var amount))
        {
            return;
        }

        if (multiplierWord is "k" or "lucas" or "luca" or "mil")
        {
            amount *= 1000;
        }
        else if (multiplierWord is "palo" or "palos")
        {
            amount *= 1_000_000;
        }

        draft.Amount = amount;

        if (currencyWord is "dolar" or "dolares" or "usd")
        {
            draft.Currency = "USD";
            draft.WasCurrencyDefaulted = false;
        }
        else if (currencyWord is "peso" or "pesos" or "ars" or "mango" or "mangos")
        {
            draft.Currency = "ARS";
            draft.WasCurrencyDefaulted = false;
        }
        else if (string.IsNullOrWhiteSpace(draft.Currency))
        {
            draft.Currency = "ARS";
            draft.WasCurrencyDefaulted = true;
        }
    }

    private static bool TryParseArgentineAmount(string value, out decimal amount)
    {
        var normalized = value.Trim();
        if (normalized.Contains('.') && normalized.Contains(','))
        {
            normalized = normalized.Replace(".", string.Empty).Replace(',', '.');
        }
        else if (normalized.Contains(','))
        {
            normalized = normalized.Replace(',', '.');
        }
        else if (normalized.Contains('.'))
        {
            var parts = normalized.Split('.');
            if (parts.Length > 1 && parts[^1].Length == 3)
            {
                normalized = normalized.Replace(".", string.Empty);
            }
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    private static void ApplyDate(AssistantTransactionDraftDto draft, string text)
    {
        var today = DateTime.UtcNow.Date.AddHours(12);
        if (text.Contains("ayer"))
        {
            draft.TransactionDate = today.AddDays(-1);
            return;
        }

        var match = DateRegex().Match(text);
        if (match.Success)
        {
            var day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);
            var month = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
            var year = match.Groups["year"].Success ? int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture) : DateTime.UtcNow.Year;
            draft.TransactionDate = new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Utc);
            return;
        }

        draft.TransactionDate ??= today;
    }

    private static void ApplyAccount(AssistantTransactionDraftDto draft, string text, IReadOnlyList<AccountBalanceSnapshot> accounts)
    {
        var exact = accounts.FirstOrDefault(account => NormalizeText(account.Name).Split(' ', StringSplitOptions.RemoveEmptyEntries).All(text.Contains));
        if (exact is not null)
        {
            draft.AccountId = exact.Id;
            return;
        }

        var alias = accounts.FirstOrDefault(account =>
        {
            var name = NormalizeText(account.Name);
            return (text.Contains("mp") || text.Contains("mercado pago") || text.Contains("mercadopago")) && name.Contains("mercado") ||
                   text.Contains("efectivo") && (name.Contains("efectivo") || name.Contains("cash")) ||
                   text.Contains("visa") && name.Contains("visa") ||
                   text.Contains("master") && name.Contains("master");
        });

        if (alias is not null)
        {
            draft.AccountId = alias.Id;
        }
    }

    private static void ApplyDescriptionAndLearningKey(AssistantTransactionDraftDto draft, string text)
    {
        var phrase = ExtractPhrase(text);
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return;
        }

        draft.LearningKey = NormalizeLearningKey(phrase);
        draft.Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(draft.LearningKey);
    }

    private async Task ApplyCategoryAsync(Guid userId, AssistantTransactionDraftDto draft, IReadOnlyList<Category> categories, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(draft.TransactionType) || string.IsNullOrWhiteSpace(draft.LearningKey))
        {
            return;
        }

        var transactionType = TransactionTypes.ToEnum(draft.TransactionType);
        var learned = await _learnedCategoryMappingRepository.GetAsync(userId, draft.LearningKey, transactionType, cancellationToken);
        if (learned is not null)
        {
            draft.CategoryId = learned.CategoryId;
            draft.SuggestedCategoryId = learned.CategoryId;
            return;
        }

        var category = categories
            .Where(category => category.Type == transactionType)
            .FirstOrDefault(category => CategoryMatches(category.Name, draft.LearningKey));

        if (category is not null)
        {
            draft.CategoryId = category.Id;
            draft.SuggestedCategoryId = category.Id;
        }
    }

    private static AssistantFollowUpDto? BuildFollowUp(AssistantTransactionDraftDto draft, IReadOnlyList<AccountBalanceSnapshot> accounts, IReadOnlyList<Category> categories)
    {
        if (!draft.Amount.HasValue || draft.Amount <= 0)
        {
            return new AssistantFollowUpDto { Field = "amount", Question = "¿Cuál fue el monto?" };
        }

        if (string.IsNullOrWhiteSpace(draft.TransactionType))
        {
            return new AssistantFollowUpDto
            {
                Field = "transactionType",
                Question = "¿Es ingreso o gasto?",
                Options = [new() { Label = "Gasto", Value = TransactionTypes.Expense }, new() { Label = "Ingreso", Value = TransactionTypes.Income }]
            };
        }

        if (!draft.AccountId.HasValue)
        {
            return new AssistantFollowUpDto
            {
                Field = "accountId",
                Question = "¿Qué cuenta usaste?",
                Options = accounts.Take(6).Select(account => new AssistantFollowUpOptionDto { Label = account.Name, Value = account.Id.ToString() }).ToList()
            };
        }

        if (!draft.CategoryId.HasValue && !draft.CategorySkipped)
        {
            var type = TransactionTypes.ToEnum(draft.TransactionType);
            var options = categories
                .Where(category => category.Type == type)
                .Take(6)
                .Select(category => new AssistantFollowUpOptionDto { Label = category.Name, Value = category.Id.ToString() })
                .ToList();
            options.Add(new AssistantFollowUpOptionDto { Label = "Sin categorizar", Value = null });

            return new AssistantFollowUpDto
            {
                Field = "categoryId",
                Question = "No estoy seguro de la categoría. ¿Cuál querés usar?",
                Options = options
            };
        }

        return null;
    }

    private static AssistantFollowUpDto? BuildChatFollowUp(AssistantTransactionDraftDto draft, IReadOnlyList<AccountBalanceSnapshot> accounts)
    {
        if (!draft.Amount.HasValue || draft.Amount <= 0)
        {
            return new AssistantFollowUpDto { Field = "amount", Question = "¿Cuál fue el monto?" };
        }

        if (!draft.AccountId.HasValue)
        {
            return new AssistantFollowUpDto
            {
                Field = "accountId",
                Question = "¿Qué cuenta usaste?",
                Options = accounts.Take(6).Select(account => new AssistantFollowUpOptionDto { Label = account.Name, Value = account.Id.ToString() }).ToList()
            };
        }

        return null;
    }

    private static AssistantTransactionPreviewDto BuildPreview(
        AssistantTransactionDraftDto draft,
        IReadOnlyList<AccountBalanceSnapshot> accounts,
        IReadOnlyList<Category> categories)
    {
        var account = accounts.First(account => account.Id == draft.AccountId);
        var category = draft.CategoryId.HasValue ? categories.FirstOrDefault(category => category.Id == draft.CategoryId.Value) : null;

        return new AssistantTransactionPreviewDto
        {
            AccountId = account.Id,
            AccountName = account.Name,
            Amount = draft.Amount!.Value,
            Currency = draft.Currency,
            TransactionType = draft.TransactionType!,
            Description = draft.Description,
            CategoryId = draft.CategoryId,
            CategoryName = category?.Name,
            TransactionDate = draft.TransactionDate!.Value
        };
    }

    private static AssistantSavedTransactionDto BuildSavedTransaction(
        Guid transactionId,
        AssistantTransactionDraftDto draft,
        IReadOnlyList<AccountBalanceSnapshot> accounts,
        IReadOnlyList<Category> categories)
    {
        var preview = BuildPreview(draft, accounts, categories);
        return new AssistantSavedTransactionDto
        {
            TransactionId = transactionId,
            AccountId = preview.AccountId,
            AccountName = preview.AccountName,
            Amount = preview.Amount,
            Currency = preview.Currency,
            TransactionType = preview.TransactionType,
            Description = preview.Description,
            CategoryId = preview.CategoryId,
            CategoryName = preview.CategoryName,
            TransactionDate = preview.TransactionDate,
            Source = TransactionSources.AssistantText
        };
    }

    private async Task<List<string>> BuildWarningsAsync(Guid userId, AssistantTransactionDraftDto draft, IReadOnlyList<AccountBalanceSnapshot> accounts, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var account = draft.AccountId.HasValue ? accounts.FirstOrDefault(account => account.Id == draft.AccountId.Value) : null;
        if (account is not null && account.CurrencyCode == "USD" && draft.Currency == "ARS" && draft.WasCurrencyDefaulted)
        {
            warnings.Add("Asumí pesos argentinos porque no mencionaste dólares.");
        }

        if (draft.AccountId.HasValue && draft.Amount.HasValue && !string.IsNullOrWhiteSpace(draft.TransactionType) && draft.TransactionDate.HasValue)
        {
            var date = draft.TransactionDate.Value.Date;
            var hasDuplicate = await _accountRepository.HasDuplicateTransactionAsync(
                userId,
                draft.AccountId.Value,
                draft.Amount.Value,
                TransactionTypes.ToEnum(draft.TransactionType),
                draft.CategoryId,
                DateTime.SpecifyKind(date, DateTimeKind.Utc),
                DateTime.SpecifyKind(date.AddDays(1), DateTimeKind.Utc),
                cancellationToken);

            if (hasDuplicate)
            {
                warnings.Add("Posible duplicado: ya tenés una transacción igual para esa cuenta, fecha, monto, tipo y categoría.");
            }
        }

        return warnings;
    }

    private static string ExtractPhrase(string text)
    {
        var cleaned = AmountRegex().Replace(text, string.Empty);
        var markers = new[] { " en ", " de ", " por ", " para " };
        foreach (var marker in markers)
        {
            var index = cleaned.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0)
            {
                cleaned = cleaned[(index + marker.Length)..];
                break;
            }
        }

        cleaned = Regex.Replace(cleaned, "\\b(con|desde|usando|cuenta|tarjeta|mercado|pago|mp|visa|master|efectivo|hoy|ayer)\\b", " ", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();
        return cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }

    private static string NormalizeLearningKey(string value)
    {
        var normalized = NormalizeText(value);
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? normalized;
    }

    private static bool CategoryMatches(string categoryName, string key)
    {
        var category = NormalizeText(categoryName);
        return category.Contains(key) ||
               key.Contains("super") && (category.Contains("food") || category.Contains("grocer") || category.Contains("comida")) ||
               key.Contains("cafe") && (category.Contains("food") || category.Contains("comida")) ||
               key.Contains("sueldo") && category.Contains("salary") ||
               key.Contains("honorario") && category.Contains("freelance");
    }

    private static bool ContainsAny(string text, IEnumerable<string> words)
    {
        return words.Any(text.Contains);
    }

    private static bool LooksLikeTransactionIntent(string text, AssistantTransactionDraftDto? previousDraft)
    {
        if (previousDraft is not null)
        {
            return true;
        }

        var normalizedText = NormalizeText(text);
        return AmountRegex().IsMatch(normalizedText) ||
               normalizedText.TrimStart().StartsWith('+') ||
               normalizedText.TrimStart().StartsWith('-') ||
               ContainsAny(normalizedText, IncomeWords) ||
               ContainsAny(normalizedText, ExpenseWords) ||
               !LooksLikeQuestion(normalizedText);
    }

    private static bool LooksLikeQuestion(string text)
    {
        return text.Contains('?') ||
               text.StartsWith("cuanto ") ||
               text.StartsWith("cuanta ") ||
               text.StartsWith("cuantos ") ||
               text.StartsWith("cuantas ") ||
               text.StartsWith("que ") ||
               text.StartsWith("como ") ||
               text.StartsWith("donde ") ||
               text.StartsWith("cuando ") ||
               text.StartsWith("por que ");
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"(?<amount>\d+(?:[\.,]\d{1,3})?(?:[\.,]\d{3})*)(?:\s*(?<multiplier>k|mil|lucas|luca|palos|palo))?(?:\s*(?<currency>pesos?|ars|mangos?|dolares?|dolar|usd))?", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"\b(?<day>\d{1,2})\/(?<month>\d{1,2})(?:\/(?<year>\d{4}))?\b")]
    private static partial Regex DateRegex();
}
