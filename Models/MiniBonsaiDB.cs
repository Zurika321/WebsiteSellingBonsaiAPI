using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
public class MiniBonsaiDBAPI : DbContext
{
    public MiniBonsaiDBAPI(DbContextOptions<MiniBonsaiDBAPI> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Bonsai)
            .WithMany(b => b.Favourites)
            .HasForeignKey(f => f.BONSAI_ID);
    }


    public DbSet<Bonsai> Bonsais { get; set; }
    public DbSet<BonsaiType> Types { get; set; }
    public DbSet<GeneralMeaning> GeneralMeaning { get; set; }
    public DbSet<Style> Styles { get; set; }
    public DbSet<WebsiteSellingBonsaiAPI.Models.AdminUser> AdminUser { get; set; }
    public DbSet<Banner> banners { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartDetail> CartDetails { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Favourite> Favourites { get; set; }
    public DbSet<Feature> Features { get; set; }
}
