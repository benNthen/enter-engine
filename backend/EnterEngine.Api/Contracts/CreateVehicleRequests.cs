using System.ComponentModel.DataAnnotations;

namespace EnterEngine.Api.Contracts;

public class CreateVehicleRequest : IValidatableObject
{
  [Required]
  [StringLength(100)]
  public string OwnerName { get; set; } = string.Empty;

  [Range(1, int.MaxValue)]
  public int ManufacturerId { get; set; }

  public int YearOfManufacture { get; set; }

  public decimal WeightKg { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {

    if (string.IsNullOrWhiteSpace(OwnerName))
    {
      yield return new ValidationResult(
          "Owner name is required.",
          new[] { nameof(OwnerName) });
    }

    // Document this year's range as an application assumption.
    if (YearOfManufacture < 1886 || YearOfManufacture > DateTime.UtcNow.Year)
    {
      yield return new ValidationResult(
          $"Year of manufacture must be between 1886 and {DateTime.Now.Year}.",
          new[] { nameof(YearOfManufacture) });
    }

    if (decimal.Round(WeightKg, 2) != WeightKg)
    {
      yield return new ValidationResult(
        "Weight must have no more than two decimal places.",
        new[] { nameof(WeightKg) });
    }

    if (WeightKg <= 0m || WeightKg > 99_999_999.99m)
    {
      yield return new ValidationResult(
          "Weight must be between 0.01 and 99,999,999.99 kg.",
          new[] { nameof(WeightKg) });
    }
  }
}