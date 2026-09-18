using Microsoft.EntityFrameworkCore;

namespace LoginService;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.UserId);

            entity.Property(u => u.UserId)
                .ValueGeneratedOnAdd();

            entity.Property(u => u.UserName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnType("varchar(255)")
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasColumnType("varchar(500)")
                .IsRequired();
            
            entity.Property(u => u.Role)
                .HasMaxLength(20)
                .IsRequired();    

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_Users_Email");
        });
    }
}
