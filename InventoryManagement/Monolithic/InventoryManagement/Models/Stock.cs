namespace InventoryManagement.Models;

public class Stock{
    public string Id { get; set; }  = Guid.NewGuid().ToString();
    public required string ProductId { get; set; }
    public int Quantity { get; set; } = 0;
}