using EnterEngine.Api.Models;
using EnterEngine.Api.Services;
using Xunit;

namespace EnterEngine.UnitTests;

public class VehicleCategoryTests
{
  private static List<Category> DefaultCategories() =>
      new()
      {
            new()
            {
                Name = "Light",
                MinWeightKg = 0m,
                MaxWeightKg = 500m,
                IconKey = "car"
            },
            new()
            {
                Name = "Medium",
                MinWeightKg = 500m,
                MaxWeightKg = 2500m,
                IconKey = "van"
            },
            new()
            {
                Name = "Heavy",
                MinWeightKg = 2500m,
                MaxWeightKg = null,
                IconKey = "truck"
            }
      };

  [Theory]
  [InlineData("0.01", "Light")]
  [InlineData("499.99", "Light")]
  [InlineData("500.00", "Medium")]
  [InlineData("500.01", "Medium")]
  [InlineData("2499.99", "Medium")]
  [InlineData("2500.00", "Heavy")]
  [InlineData("2500.01", "Heavy")]
  [InlineData("99999999.99", "Heavy")]
  public void Resolve_ReturnsExpectedCategory(
      string weight,
      string expectedCategory)
  {
    var weightKg = decimal.Parse(
        weight,
        System.Globalization.CultureInfo.InvariantCulture);

    var result = VehicleCategoryCalculator.Resolve(
        weightKg,
        DefaultCategories());

    Assert.Equal(expectedCategory, result.Name);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Resolve_RejectsNonPositiveWeight(int weight)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
        VehicleCategoryCalculator.Resolve(
            weight,
            DefaultCategories()));
  }
}