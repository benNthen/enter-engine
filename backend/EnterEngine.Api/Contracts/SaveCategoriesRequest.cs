using System.ComponentModel.DataAnnotations;

namespace EnterEngine.Api.Contracts;

public class SaveCategoriesRequest
{
  [Required]
  public List<CategoryInput> Categories { get; set; } = new();
}

public class CategoryInput
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = string.Empty;

  [Required]
  public decimal? MinWeightKg { get; set; }
  public decimal? MaxWeightKg { get; set; }

  [Required]
  [StringLength(30)]
  public string IconKey { get; set; } = string.Empty;
}