using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Dtos;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers;

[Controller]
[Route("api/[controller]")]
public class ProductsController(InventoryContext context) : ControllerBase
{
    private readonly InventoryContext _context = context;

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO product)
    {
        var newProduct = new Product
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        };
        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var stock = await _context.Stock.FirstOrDefaultAsync(s => s.ProductId == id);

        return Ok(new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            Stock = stock == null
                ?null
                : new { stock.Id, stock.ProductId, stock.Quantity }
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.Take(500).ToListAsync();
        return Ok(products);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
