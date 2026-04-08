namespace ArgenCash.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
