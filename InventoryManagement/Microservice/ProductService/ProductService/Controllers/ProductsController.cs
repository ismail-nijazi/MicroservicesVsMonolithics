namespace ProductService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;
using System.Net.Http;
using System.Text.Json;


[Controller]
[Route("api/[controller]")]
public class ProductsController(ProductServiceContext context, HttpClient httpClient, IConfiguration config) : ControllerBase
{
    private readonly ProductServiceContext _context = context;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _stockServiceBaseUrl = config["StockService:BaseUrl"];

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

        var stockUrl = $"{_stockServiceBaseUrl}/api/stock/product/{id}";

        object stock = new { Quantity = "Unavailable" };

        try
        {
            var response = await _httpClient.GetAsync(stockUrl);
            if (response.IsSuccessStatusCode)
            {
                using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = jsonDoc.RootElement;

                stock = new
                {
                    Id = root.GetProperty("id").GetString(),
                    ProductId = root.GetProperty("productId").GetString(),
                    Quantity = root.GetProperty("quantity").GetInt32()
                };
            }
        }
        catch
        {
            // Ignore errors, stock = "Unavailable"
        }

        return Ok(new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            Stock = stock
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
