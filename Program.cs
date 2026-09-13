using HouseRentMgmt.Api.Features;
using HouseRentMgmt.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.AddCustomLogging();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAllFeatureServices();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();


var api = app.MapGroup("/webservice/v1/api");

api.MapAllApplicationEndPoints();

app.Run();
