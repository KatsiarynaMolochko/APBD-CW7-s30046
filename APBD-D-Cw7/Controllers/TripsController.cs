using APBD_D_Cw7.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_D_Cw7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly IDbService _dbService;

    public TripsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _dbService.GetTripsAsync();
        return Ok(trips);
    }
}