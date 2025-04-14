using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Dtos;

public record class CreateProductDTO(
    [Required(ErrorMessage= "The Name is required")]
    [StringLength(30, ErrorMessage ="Name can't be more than 30 characters")] 
    string Name, 
    [Required(ErrorMessage = "The Description is required")]
    [StringLength(300, ErrorMessage ="Description can't be more than 300 characters")] 
    string Description, 
    [Required] 
    decimal Price
);

