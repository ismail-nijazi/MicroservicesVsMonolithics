using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductServiceContext(DbContextOptions<ProductServiceContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}