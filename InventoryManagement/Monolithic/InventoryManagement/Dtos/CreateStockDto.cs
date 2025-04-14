using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Dtos;

public record class CreateStockDto(
    [Required(ErrorMessage= "The products id is required")]
    string ProductId, 
    [Required(ErrorMessage="Quantity is required")] int Quantity
);