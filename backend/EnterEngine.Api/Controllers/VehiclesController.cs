using EnterEngine.Api.Contracts;
using EnterEngine.Api.Data;
using EnterEngine.Api.Models;
using EnterEngine.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
  private readonly VehicleDbContext _db;

  public VehiclesController(VehicleDbContext db)
  {
    _db = db;
  }

  [HttpPost]
  public async Task<IActionResult> Create(
    CreateVehicleRequest request,
    CancellationToken cancellationToken)
  {
    var manufacturerExists = await _db.Manufacturers.AnyAsync(
      x => x.Id == request.ManufacturerId,
      cancellationToken);

    if (!manufacturerExists)
    {
      ModelState.AddModelError(
        nameof(request.ManufacturerId),
        "Select a valid manufacturer.");

      return ValidationProblem(ModelState);
    }

    var vehicle = new Vehicle
    {
      OwnerName = request.OwnerName.Trim(),
      WeightKg = request.WeightKg,
      YearOfManufacture = request.YearOfManufacture,
      ManufacturerId = request.ManufacturerId
    };

    _db.Vehicles.Add(vehicle);
    await _db.SaveChangesAsync(cancellationToken);

    return StatusCode(StatusCodes.Status201Created, new
    {
      vehicle.Id
    });

  }

  [HttpGet]
  [HttpGet]
  public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
  {
    var categories = await _db.Categories
        .AsNoTracking()
        .OrderBy(x => x.MinWeightKg)
        .ToListAsync(cancellationToken);

    var vehicles = await _db.Vehicles
        .AsNoTracking()
        .OrderByDescending(x => x.Id)
        .Select(x => new
        {
          x.Id,
          x.OwnerName,
          Manufacturer = x.Manufacturer.Name,
          x.YearOfManufacture,
          x.WeightKg
        })
        .ToListAsync(cancellationToken);

    var response = vehicles.Select(vehicle =>
    {
      var category = VehicleCategoryCalculator.Resolve(
          vehicle.WeightKg,
          categories);

      return new
      {
        vehicle.Id,
        vehicle.OwnerName,
        vehicle.Manufacturer,
        vehicle.YearOfManufacture,
        vehicle.WeightKg,
        Category = category.Name,
        CategoryIconKey = category.IconKey
      };
    }).ToList();

    return Ok(response);
  }
}
