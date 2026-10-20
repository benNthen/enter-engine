namespace EnterEngine.Api.Models;

public class Category
{
  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public decimal MinWeightKg { get; set; }

  // Null means there is no upper limit.
  public decimal? MaxWeightKg { get; set; }

  public string IconKey { get; set; } = "car";
}