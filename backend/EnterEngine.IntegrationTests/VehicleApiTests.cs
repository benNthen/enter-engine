using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EnterEngine.IntegrationTests;
// Each tests creates a new database, with seeded data migrated. Database is dropped after test ends.
public class VehicleApiTests : IAsyncLifetime
{
  private readonly TestApiFactory _factory = new();
  private HttpClient _client = null!;

  public async Task InitializeAsync()
  {
    await _factory.InitialiseDatabaseAsync();
    _client = _factory.CreateClient();
  }

  public async Task DisposeAsync()
  {
    try
    {
      await _factory.DeleteDatabaseAsync();
    }
    finally
    {
      _client?.Dispose();
      _factory.Dispose();
    }
  }

  private async Task<int> GetManufacturerId()
  {
    var response = await _client.GetAsync("/api/manufacturers");
    response.EnsureSuccessStatusCode();

    var manufacturers =
        await response.Content.ReadFromJsonAsync<JsonElement>();

    return manufacturers[0].GetProperty("id").GetInt32();
  }

  private async Task<int> CreateVehicle(decimal weightKg)
  {
    var manufacturerId = await GetManufacturerId();

    var response = await _client.PostAsJsonAsync(
        "/api/vehicles",
        new
        {
          ownerName = "John Smith",
          manufacturerId,
          yearOfManufacture = 2019,
          weightKg
        });

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body =
        await response.Content.ReadFromJsonAsync<JsonElement>();

    return body.GetProperty("id").GetInt32();
  }

  private async Task<JsonElement> GetVehicle(int id)
  {
    var response = await _client.GetAsync("/api/vehicles");
    response.EnsureSuccessStatusCode();

    var vehicles =
        await response.Content.ReadFromJsonAsync<JsonElement>();

    return vehicles.EnumerateArray()
        .Single(x => x.GetProperty("id").GetInt32() == id);
  }

  private static object Configuration(
      decimal mediumUpper,
      decimal heavyLower) =>
      new
      {
        categories = new object[]
          {
                new
                {
                    name = "Light",
                    minWeightKg = 0m,
                    maxWeightKg = (decimal?)500m,
                    iconKey = "car"
                },
                new
                {
                    name = "Medium",
                    minWeightKg = 500m,
                    maxWeightKg = (decimal?)mediumUpper,
                    iconKey = "van"
                },
                new
                {
                    name = "Heavy",
                    minWeightKg = heavyLower,
                    maxWeightKg = (decimal?)null,
                    iconKey = "truck"
                }
          }
      };

  [Fact]
  public async Task CreatedVehicle_IsReturnedBySeparateRequest()
  {
    var id = await CreateVehicle(1350.75m);

    var vehicle = await GetVehicle(id);

    Assert.Equal(
        "John Smith",
        vehicle.GetProperty("ownerName").GetString());

    Assert.Equal(
        1350.75m,
        vehicle.GetProperty("weightKg").GetDecimal());

    Assert.Equal(
        "Medium",
        vehicle.GetProperty("category").GetString());
  }

  [Fact]
  public async Task CategoryChange_ReclassifiesExistingVehicle()
  {
    var id = await CreateVehicle(2200m);

    var before = await GetVehicle(id);
    Assert.Equal(
        "Medium",
        before.GetProperty("category").GetString());

    var response = await _client.PutAsJsonAsync(
        "/api/categories",
        Configuration(2000m, 2000m));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var after = await GetVehicle(id);

    Assert.Equal(
        "Heavy",
        after.GetProperty("category").GetString());

    Assert.Equal(
        "truck",
        after.GetProperty("categoryIconKey").GetString());

    Assert.Equal(
        2200m,
        after.GetProperty("weightKg").GetDecimal());
  }

  [Theory]
  [InlineData(2000, 2100)] // Gap.
  [InlineData(2100, 2000)] // Overlap.
  public async Task InvalidConfiguration_LeavesSavedDataUnchanged(
      int mediumUpper,
      int heavyLower)
  {
    var before = await _client.GetStringAsync("/api/categories");

    var response = await _client.PutAsJsonAsync(
        "/api/categories",
        Configuration(mediumUpper, heavyLower));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var after = await _client.GetStringAsync("/api/categories");

    Assert.Equal(before, after);
  }

  [Fact]
  public async Task UnknownManufacturer_IsRejectedWithoutSaving()
  {
    var response = await _client.PostAsJsonAsync(
        "/api/vehicles",
        new
        {
          ownerName = "John Smith",
          manufacturerId = int.MaxValue,
          yearOfManufacture = 2019,
          weightKg = 1350m
        });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var vehicles = await _client
        .GetFromJsonAsync<JsonElement>("/api/vehicles");

    Assert.Equal(0, vehicles.GetArrayLength());
  }

  [Fact]
  public async Task InvalidWeight_IsRejectedWithoutSaving()
  {
    var manufacturerId = await GetManufacturerId();

    var response = await _client.PostAsJsonAsync(
        "/api/vehicles",
        new
        {
          ownerName = "John Smith",
          manufacturerId,
          yearOfManufacture = 2019,
          weightKg = 1200.555m
        });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var vehicles = await _client
        .GetFromJsonAsync<JsonElement>("/api/vehicles");

    Assert.Equal(0, vehicles.GetArrayLength());
  }
}