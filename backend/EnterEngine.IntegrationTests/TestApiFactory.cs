using EnterEngine.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterEngine.IntegrationTests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
  private readonly string _connectionString;

  public TestApiFactory()
  {
    var baseConnection = Environment.GetEnvironmentVariable(
        "ENTERENGINE_TEST_SQL");

    if (string.IsNullOrWhiteSpace(baseConnection))
    {
      throw new InvalidOperationException(
          "Set ENTERENGINE_TEST_SQL before running integration tests.");
    }

    var builder = new SqlConnectionStringBuilder(baseConnection)
    {
      InitialCatalog =
            $"EnterEngine_Tests_{Guid.NewGuid():N}"
    };

    _connectionString = builder.ConnectionString;
  }

  protected override void ConfigureWebHost(
      IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      // Replace the application's DbContext configuration.
      services.RemoveAll<VehicleDbContext>();
      services.RemoveAll<DbContextOptions<VehicleDbContext>>();
      services.RemoveAll<
              IDbContextOptionsConfiguration<VehicleDbContext>>();

      services.AddDbContext<VehicleDbContext>(options =>
              options.UseSqlServer(_connectionString));
    });
  }

  public async Task InitialiseDatabaseAsync()
  {
    using var scope = Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<VehicleDbContext>();

    // Applies real migrations, including initial category data.
    await db.Database.MigrateAsync();
  }

  public async Task DeleteDatabaseAsync()
  {
    using var scope = Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<VehicleDbContext>();

    await db.Database.EnsureDeletedAsync();
  }
}