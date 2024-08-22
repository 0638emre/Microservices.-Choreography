using Microsoft.EntityFrameworkCore;

namespace Order.API.Models.Context;

public class OrderAPIDBContext : DbContext
{
    public OrderAPIDBContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    
}