using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
public class MiniBonsaiDBAPI : DbContext
{
    public MiniBonsaiDBAPI(DbContextOptions<MiniBonsaiDBAPI> options)
        : base(options)
    {
    }

    public DbSet<Bonsai> Bonsais { get; set; }
    public DbSet<BonsaiType> Types { get; set; }
    public DbSet<GeneralMeaning> GeneralMeaning { get; set; }
    public DbSet<Style> Styles { get; set; }
    public DbSet<WebsiteSellingBonsaiAPI.Models.AdminUser> AdminUser { get; set; } = default!;
    public DbSet<Banner> banners { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartDetail> CartDetails { get; set; }
    public DbSet<Review> Reviews { get; set; }
}
