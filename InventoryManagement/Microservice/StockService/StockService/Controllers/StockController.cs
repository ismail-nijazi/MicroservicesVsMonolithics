
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.Dtos;
using StockService.Models;

namespace StockService.Controllers;

[Controller]
[Route("api/[controller]")]
public class StockController: ControllerBase{
    private readonly StockServiceContext _context;

    public StockController(StockServiceContext context){
        _context = context;
    }

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

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> getStockByProductId(string productId)
    {
        var stock = await _context.Stock.FirstOrDefaultAsync(s => s.ProductId == productId);
        if (stock == null)
        {
            return NotFound();
        }
        return Ok(stock);
    }
}