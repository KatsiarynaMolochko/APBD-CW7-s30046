using APBD_D_Cw7.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_D_Cw7.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ClientsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id)
    {
        var trips = await _dbService.GetTripsForClientAsync(id);

        if (trips == null)
            return NotFound($"Client with id {id} does not exist.");
        
        return Ok(trips);
    }

}