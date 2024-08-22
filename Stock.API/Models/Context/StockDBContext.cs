using Microsoft.EntityFrameworkCore;

namespace Stock.API.Models.Context;

public class StockDBContext : DbContext
{
    public StockDBContext(DbContextOptions<StockDBContext> options) :base(options)
    {
        
    }
    
    public DbSet<Stock> Stocks { get; set; }
}