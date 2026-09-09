using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Tenants.TenantHandler;

public static class EditTenant
{
    public static void MapEditTenant(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        Guid id,
        EditTenantRequestDto editTenantRequestDto,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(EditTenant));

        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Tenant update rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var tenant = await dbContext.Tenant.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (tenant is null)
        {
            logger.LogWarning("Tenant update rejected: the tenant was not found for the authenticated user.");
            return Results.NotFound(new TenantResponseDto(false, "Tenant not found."));
        }

        tenant.Name = editTenantRequestDto.Name;
        tenant.Phone = editTenantRequestDto.Phone;
        tenant.Email = editTenantRequestDto.Email;
        tenant.UpdatedAt = DateTime.UtcNow;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Tenant update failed: the expected tenant row was not updated; it may have been deleted before saving.");
            return Results.Conflict(new TenantResponseDto(false, "The tenant could not be updated. It may have been deleted. Reload and try again."));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Tenant_UserId_Phone"
        })
        {
            logger.LogWarning("Tenant update rejected: a tenant with this phone number already exists for this user.");
            return Results.Conflict(new TenantResponseDto(false, "You already have a tenant with this phone number."));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Tenant {TenantId} edited",id);
        }
        var response = new TenantDetailsDto(
            Id: tenant.Id,
            Name: tenant.Name,
            Phone: tenant.Phone,
            Email: tenant.Email,
            CreatedAt: tenant.CreatedAt);

        return Results.Ok(response);
    }
}
