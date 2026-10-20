using EnterEngine.Api.Contracts;

namespace EnterEngine.Api.Services;

// In charge of validating the category configuration provided by the user.
public static class CategoryConfigurationValidator
{
  private const decimal MaximumWeight = 99_999_999.99m;

  private static readonly HashSet<string> AllowedIcons =
    new(StringComparer.Ordinal)
    {
      "car",
      "van",
      "truck"
    };

  public static List<string> Validate(
    IReadOnlyList<CategoryInput> categories)
  {
    var errors = new List<string>();

    if (categories.Count == 0)
    {
      errors.Add("At least one category must be provided.");
      return errors;
    }

    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var category in categories)
    {
      var name = category.Name?.Trim() ?? "";

      if (name.Length == 0 || name.Length > 50)
      {
        errors.Add(
                    "Every category needs a name of 1–50 characters.");
      }

      if (!names.Add(name))
      {
        errors.Add($"Category name '{name}' is duplicated.");
      }

      if (!AllowedIcons.Contains(category.IconKey ?? ""))
      {
        errors.Add($"Select a valid icon for '{name}'.");
      }

      if (category.MinWeightKg is not decimal minimum)
      {
        errors.Add($"Enter a lower boundary for '{name}'.");
        continue;
      }

      if (!IsValidBoundary(minimum))
      {
        errors.Add(
            $"The lower boundary for '{name}' must be between " +
            "0 and 99,999,999.99 with up to two decimal places.");
      }

      if (category.MaxWeightKg is decimal maximum)
      {
        if (!IsValidBoundary(maximum))
        {
          errors.Add(
              $"The upper boundary for '{name}' must be between " +
              "0 and 99,999,999.99 with up to two decimal places.");
        }

        if (maximum <= minimum)
        {
          errors.Add(
              $"The upper boundary for '{name}' must exceed " +
              "its lower boundary.");
        }
      }
    }

    // Only compare ranges after their individual values are valid.
    if (errors.Count > 0)
    {
      return errors;
    }

    var ordered = categories
        .OrderBy(x => x.MinWeightKg!.Value)
        .ToList();

    if (ordered[0].MinWeightKg != 0m)
    {
      errors.Add("The first category must start at 0 kg.");
    }

    for (var index = 0; index < ordered.Count - 1; index++)
    {
      var current = ordered[index];
      var next = ordered[index + 1];

      if (current.MaxWeightKg is not decimal upper)
      {
        errors.Add(
            "Only the final category can have no upper limit.");

        continue;
      }

      var nextLower = next.MinWeightKg!.Value;

      if (upper < nextLower)
      {
        errors.Add(
            $"There is a gap between '{current.Name}' " +
            $"and '{next.Name}'.");
      }
      else if (upper > nextLower)
      {
        errors.Add(
            $"'{current.Name}' overlaps '{next.Name}'.");
      }
    }

    if (ordered[^1].MaxWeightKg is not null)
    {
      errors.Add(
          "The final category must have no upper limit.");
    }

    return errors;

  }

  // Prevents overlapping or gaps in the category ranges, and ensures that the values are within the allowed range and precision.
  private static bool IsValidBoundary(decimal value)
  {
    return value >= 0m
        && value <= MaximumWeight
        && decimal.Round(value, 2) == value;
  }
}