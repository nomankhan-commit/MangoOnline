
using Mango.Services.ShoppingCartAPI.Model;
using Mango.Services.ShoppingCartAPI.Model.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.ShoppingCart.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) { }
        public DbSet<CartHeader> cartHeaders { get; set; }
        public DbSet<CartDetails> cartDetails { get; set; }
        
    }
}
