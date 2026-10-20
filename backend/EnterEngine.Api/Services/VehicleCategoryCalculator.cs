using EnterEngine.Api.Models;

namespace EnterEngine.Api.Services;

public static class VehicleCategoryCalculator
{
  // requires exactly one matching category. If an invalid configuration 
  // somehow reaches the database, the application fails visibly instead of silently choosing an arbitrary category.
  public static Category Resolve(
      decimal weightKg,
      IReadOnlyList<Category> categories)
  {
    if (weightKg <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(weightKg));
    }

    return categories.Single(category =>
        weightKg >= category.MinWeightKg
        && (
            category.MaxWeightKg is null
            || weightKg < category.MaxWeightKg.Value
        ));
  }
}