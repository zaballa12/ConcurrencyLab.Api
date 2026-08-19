namespace ConcurrencyLab.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Stock { get; set; }

    public Guid Version { get; set; }
}
