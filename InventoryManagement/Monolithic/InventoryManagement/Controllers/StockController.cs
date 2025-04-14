
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Dtos;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers;

[Controller]
[Route("api/[controller]")]
public class StockController(InventoryContext context) : ControllerBase{
    private readonly InventoryContext _context = context;

    [HttpGet]
    public async Task<IActionResult> getStocks(){
        var stocks = await _context.Stock.ToArrayAsync();
        return Ok(stocks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> getStock(string id){
        var stock = await _context.Stock.FindAsync(id);
        if(stock == null){
            return NotFound();
        }
        return Ok(stock);
    }

    [HttpPost]
    public async Task<IActionResult> createStock([FromBody] CreateStockDto stock){
        var newStock = new Stock{
            ProductId = stock.ProductId,
            Quantity = stock.Quantity
        };
        _context.Stock.Add(newStock);
        await _context.SaveChangesAsync();  
        return CreatedAtAction(nameof(getStock), new {id = newStock.Id}, newStock);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteStock(string id){
        var stock = await _context.Stock.FindAsync(id);
        if(stock == null){
            return NotFound();
        }
        _context.Stock.Remove(stock);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}