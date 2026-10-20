using System.Data;
using EnterEngine.Api.Contracts;
using EnterEngine.Api.Data;
using EnterEngine.Api.Models;
using EnterEngine.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoriesController : ControllerBase
{
  private readonly VehicleDbContext _db;

  public CategoriesController(VehicleDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
  {
    var categories = await _db.Categories
      .AsNoTracking()
      .OrderBy(x => x.MinWeightKg)
      .ToListAsync(cancellationToken);

    return Ok(categories);
  }

  [HttpPut]
  public async Task<IActionResult> Save(
    SaveCategoriesRequest request,
    CancellationToken cancellationToken)
  {
    var errors = CategoryConfigurationValidator.Validate(request.Categories);

    if (errors.Count > 0)
    {
      foreach (var error in errors)
      {
        ModelState.AddModelError("Categories", error);
      }

      return ValidationProblem(ModelState);
    }

    // Serialize replacement operations so concurrent saves
    // cannot combine rows from two configurations.
    await using var transaction =
      await _db.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken);

    var existing = await _db.Categories
      .ToListAsync(cancellationToken);

    _db.Categories.RemoveRange(existing);

    var replacements = request.Categories.Select(x => new Category
    {
      Name = x.Name.Trim(),
      MinWeightKg = x.MinWeightKg!.Value,
      MaxWeightKg = x.MaxWeightKg,
      IconKey = x.IconKey
    }).ToList();

    _db.Categories.AddRange(replacements);

    await _db.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return NoContent();
  }

}