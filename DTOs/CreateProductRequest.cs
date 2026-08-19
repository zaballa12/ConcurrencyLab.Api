namespace ConcurrencyLab.Api.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public int Stock { get; set; }
}