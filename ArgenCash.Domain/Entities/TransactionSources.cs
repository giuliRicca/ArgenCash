namespace ArgenCash.Domain.Entities;

public static class TransactionSources
{
    public const string Manual = "MANUAL";
    public const string AssistantText = "ASSISTANT_TEXT";
    public const string AssistantVoice = "ASSISTANT_VOICE";

    public static TransactionSource ToEnum(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return TransactionSource.Manual;
        }

        return source.Trim().ToUpperInvariant() switch
        {
            Manual => TransactionSource.Manual,
            AssistantText => TransactionSource.AssistantText,
            AssistantVoice => TransactionSource.AssistantVoice,
            _ => throw new ArgumentException("Transaction source must be MANUAL, ASSISTANT_TEXT, or ASSISTANT_VOICE.", nameof(source))
        };
    }

    public static string ToString(TransactionSource source)
    {
        return source switch
        {
            TransactionSource.Manual => Manual,
            TransactionSource.AssistantText => AssistantText,
            TransactionSource.AssistantVoice => AssistantVoice,
            _ => throw new ArgumentException($"Invalid transaction source enum: {source}")
        };
    }
}
