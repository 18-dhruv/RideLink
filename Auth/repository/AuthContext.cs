using Auth.model;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;

public class AuthContext : DbContext
{
   DbSet<User> Users{get;set;}
   DbSet<RefreshToken> RefreshTokens{get;set;}
   public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
        .HasIndex(e=>e.Email)
        .IsUnique();
        modelBuilder.Entity<RefreshToken>()
        .HasOne<User>()
        .WithMany()
        .HasForeignKey(e=>e.UserId)
        .OnDelete(deleteBehavior:DeleteBehavior.Cascade);
    }
}