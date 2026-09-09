using HouseRentMgmt.Api.Features.Auth.Entities;
using HouseRentMgmt.Api.Features.Leases.Entities;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Features.Tenants.Entities;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // CRITICAL: Always call the base method first, otherwise Identity configuration will break!
        base.OnModelCreating(builder);

        // This tells Postgres to automatically generate standard UUIDs if they are null
        builder.Entity<ApplicationUser>()
            .Property(u => u.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        builder.Entity<EmailVerificationCode>()
            .Property(u => u.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Entity<EmailVerificationCode>()
            .HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmailVerificationCode>()
            .HasIndex(e => e.UserId)
            .IsUnique();

        builder.Entity<Room>()
            .Property(r => r.BaseRentAmount)
            .HasPrecision(10, 2);

        builder.Entity<Lease>()
            .Property(l => l.MonthlyRent)
            .HasPrecision(10, 2);

        builder.Entity<Lease>()
            .HasOne(l => l.Room)
            .WithMany(r => r.Leases)
            .HasForeignKey(l => l.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lease>()
            .HasOne(l => l.Tenant)
            .WithMany(t => t.Leases)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Lease>()
            .HasIndex(l => l.RoomId)
            .IsUnique()
            .HasDatabaseName("unique_active_lease")
            .HasFilter("\"IsActive\"= TRUE");

        builder.Entity<Room>()
            .HasOne(r => r.User)
            .WithMany(u => u.Rooms)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Room>()
            .HasIndex(r => new {r.UserId, r.RoomName})
            .IsUnique();

        builder.Entity<Tenant>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tenants)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Tenant>()
            .HasIndex(t => new {t.UserId, t.Phone})
            .IsUnique();
    }

    public DbSet<EmailVerificationCode> EmailVerificationCode => Set<EmailVerificationCode>();

    public DbSet<Room> Room => Set<Room>();
    public DbSet<Tenant> Tenant => Set<Tenant>();
    public DbSet<Lease> Lease => Set<Lease>();

}
