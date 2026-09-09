using System.Security.Claims;
using HouseRentMgmt.Api.Features.Tenants.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Tenants.TenantHandler;

public static class AddTenant
{
    public static void MapAddTenant(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        AddTenantRequestDto addTenantRequestDto,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(AddTenant));

        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Tenant creation rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }
        var newTenant = new Tenant
        {
            Name = addTenantRequestDto.Name,
            Phone = addTenantRequestDto.Phone,
            Email = addTenantRequestDto.Email,
            UserId = userId
        };

        dbContext.Tenant.Add(newTenant);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Tenant_UserId_Phone" })
        {
            logger.LogWarning("Tenant creation rejected: the tenant with the same phone number already exists.");
            return Results.Conflict(new TenantResponseDto(false, "You already have a tenant with this Phone No."));

        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: "FK_Tenant_AspNetUsers_UserId" })
        {
            logger.LogWarning("Tenant creation rejected: the authenticated user's account no longer exists.");
            return Results.Unauthorized();
        }

        var response = new TenantDetailsDto
        (
            Id: newTenant.Id,
            Name: newTenant.Name,
            Phone: newTenant.Phone,
            Email: newTenant.Email,
            CreatedAt: newTenant.CreatedAt
        );
        return Results.Created($"/webservice/v1/api/tenant/{newTenant.Id}", response);
    }
}
