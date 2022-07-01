using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Temprature.DataAccess;
public class TempratureDbContext :DbContext
{
    public TempratureDbContext() { }
    public TempratureDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Temprature> Temprature { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SnakeCaseIdentityTableNames(modelBuilder);  
    }

    private static void SnakeCaseIdentityTableNames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Temprature>(b => { b.ToTable("temprature"); });
    }
}
 