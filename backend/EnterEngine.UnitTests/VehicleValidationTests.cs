using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EnterEngine.Api.Contracts;
using Xunit;

namespace EnterEngine.UnitTests;

public class VehicleValidationTests
{
  private static CreateVehicleRequest ValidRequest() =>
      new()
      {
        OwnerName = "John Smith",
        ManufacturerId = 2,
        YearOfManufacture = 2019,
        WeightKg = 1350.75m
      };

  private static bool IsValid(CreateVehicleRequest request)
  {
    var results = new List<ValidationResult>();

    return Validator.TryValidateObject(
        request,
        new ValidationContext(request),
        results,
        validateAllProperties: true);
  }

  [Fact]
  public void ValidVehicle_PassesValidation()
  {
    Assert.True(IsValid(ValidRequest()));
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void MissingOwner_IsRejected(string owner)
  {
    var request = ValidRequest();
    request.OwnerName = owner;

    Assert.False(IsValid(request));
  }

  [Theory]
  [InlineData("0")]
  [InlineData("-1")]
  [InlineData("1200.555")]
  [InlineData("100000000")]
  public void InvalidWeight_IsRejected(string weight)
  {
    var request = ValidRequest();
    request.WeightKg = decimal.Parse(
        weight,
        System.Globalization.CultureInfo.InvariantCulture);

    Assert.False(IsValid(request));

    var results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(
        request,
        new ValidationContext(request),
        results,
        validateAllProperties: true);

    Assert.False(isValid);
    Assert.Contains(
        results,
        result => result.MemberNames.Contains(
            nameof(CreateVehicleRequest.WeightKg)));
  }

  [Theory]
  [InlineData("0.01")]
  [InlineData("1200")]
  [InlineData("1200.5")]
  [InlineData("1200.55")]
  public void ValidWeight_IsAccepted(string weight)
  {
    var request = ValidRequest();
    request.WeightKg = decimal.Parse(
        weight,
        CultureInfo.InvariantCulture);

    Assert.True(IsValid(request));
  }

  [Fact]
  public void FutureYear_IsRejected()
  {
    var request = ValidRequest();
    request.YearOfManufacture = DateTime.UtcNow.Year + 1;

    Assert.False(IsValid(request));
  }

  [Fact]
  public void YearBeforeSupportedRange_IsRejected()
  {
    var request = ValidRequest();
    request.YearOfManufacture = 1885;

    Assert.False(IsValid(request));
  }
}