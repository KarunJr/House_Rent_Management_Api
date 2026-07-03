using HouseRentMgmt.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Data;

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
    }
}
