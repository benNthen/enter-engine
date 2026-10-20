using EnterEngine.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Vehicles")
        ?? throw new InvalidOperationException(
            "Connection string 'Vehicles' was not configured.")));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Run();

public partial class Program
{
}
