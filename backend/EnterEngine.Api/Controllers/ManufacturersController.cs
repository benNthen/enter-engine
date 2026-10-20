using EnterEngine.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturersController : ControllerBase
{
  private readonly VehicleDbContext _db;

  public ManufacturersController(VehicleDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
  {
    var manufacturers = await _db.Manufacturers
      .AsNoTracking()
      .OrderBy(x => x.Id)
      .Select(x => new { x.Id, x.Name })
      .ToListAsync(cancellationToken);

    return Ok(manufacturers);
  }
}