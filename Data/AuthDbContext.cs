using Microsoft.EntityFrameworkCore;

namespace LoginService;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users {get; set;}
    public DbSet<RefreshToken> RefreshTokens { get; set; }
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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(x => x.RefreshTokenId);

            entity.Property(x => x.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.ExpiresAt)
                .IsRequired();

            entity.Property(x => x.RevokedAt)
                .IsRequired(false);

            entity.Property(x => x.ReplacedByTokenHash)
                .HasMaxLength(64);

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
