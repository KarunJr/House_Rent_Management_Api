using HouseRentMgmt.Api.Data;
using HouseRentMgmt.Api.Endpoints;
using HouseRentMgmt.Api.Extensions;
using HouseRentMgmt.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthenticationService(builder.Configuration);
builder.Services.AddCorsService();
builder.Services.AddTokenService();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NeonDbConnection"))
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("NextJsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();

