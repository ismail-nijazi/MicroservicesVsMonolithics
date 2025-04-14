namespace InventoryManagement.Models;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string Description { get; set; } = String.Empty;
    public decimal Price { get; set; }
}
