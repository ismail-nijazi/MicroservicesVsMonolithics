using Microsoft.EntityFrameworkCore;
using StockService.Models;

namespace StockService.Data;


public class StockServiceContext(DbContextOptions<StockServiceContext> options): DbContext(options){
    public DbSet<Stock> Stock { get; set; }
}