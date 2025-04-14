using System.ComponentModel.DataAnnotations;

namespace StockService.Dtos;

public record class CreateStockDto(
    [Required(ErrorMessage= "The products id is required")]
    string ProductId, 
    [Required(ErrorMessage="Quantity is required")] int Quantity
);