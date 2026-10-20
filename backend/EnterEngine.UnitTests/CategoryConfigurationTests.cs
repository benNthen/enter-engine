using EnterEngine.Api.Contracts;
using EnterEngine.Api.Services;
using Xunit;

namespace EnterEngine.UnitTests;

public class CategoryConfigurationTests
{
  private static List<CategoryInput> ValidConfiguration() =>
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

  [Fact]
  public void Validate_AcceptsCompleteAdjacentRanges()
  {
    var errors = CategoryConfigurationValidator.Validate(
        ValidConfiguration());

    Assert.Empty(errors);
  }

  [Theory]
  [InlineData(600)] // Gap after Light ends at 500.
  [InlineData(400)] // Overlap with Light.
  public void Validate_RejectsGapOrOverlap(int mediumMinimum)
  {
    var categories = ValidConfiguration();
    categories[1].MinWeightKg = mediumMinimum;

    var errors = CategoryConfigurationValidator.Validate(categories);

    Assert.NotEmpty(errors);
  }

  [Fact]
  public void Validate_RejectsMissingLowestRange()
  {
    var categories = ValidConfiguration();
    categories[0].MinWeightKg = 1m;

    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(categories));
  }

  [Fact]
  public void Validate_RejectsFiniteFinalRange()
  {
    var categories = ValidConfiguration();
    categories[2].MaxWeightKg = 10_000m;

    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(categories));
  }

  [Fact]
  public void Validate_RejectsUnlimitedNonFinalRange()
  {
    var categories = ValidConfiguration();
    categories[0].MaxWeightKg = null;

    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(categories));
  }

  [Theory]
  [InlineData(500, 500)]
  [InlineData(600, 500)]
  public void Validate_RejectsEmptyOrReversedRange(
      int minimum,
      int maximum)
  {
    var categories = ValidConfiguration();
    categories[1].MinWeightKg = minimum;
    categories[1].MaxWeightKg = maximum;

    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(categories));
  }

  [Fact]
  public void Validate_RejectsExcessDecimalPlaces()
  {
    var categories = ValidConfiguration();
    categories[0].MaxWeightKg = 500.555m;
    categories[1].MinWeightKg = 500.555m;

    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(categories));
  }

  [Fact]
  public void Validate_RejectsEmptyConfiguration()
  {
    Assert.NotEmpty(
        CategoryConfigurationValidator.Validate(
            new List<CategoryInput>()));
  }

  [Fact]
  public void Validate_AcceptsOneUnlimitedCategory()
  {
    var categories = new List<CategoryInput>
        {
            new()
            {
                Name = "All vehicles",
                MinWeightKg = 0m,
                MaxWeightKg = null,
                IconKey = "car"
            }
        };

    Assert.Empty(
        CategoryConfigurationValidator.Validate(categories));
  }
}