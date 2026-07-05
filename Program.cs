using HouseRentMgmt.Api.Features;
using HouseRentMgmt.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAllFeatureServices();

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


var api = app.MapGroup("/webservice/v1/api");

api.MapAllApplicationEndPoints();

app.Run();

