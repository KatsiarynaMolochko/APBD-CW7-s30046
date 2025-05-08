using APBD_D_Cw7.Models;
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
    //zwraca listę wycieczek, na które zapisany jest klient o podanym id
    // jeśli klient nie istnieje zwraca 404.
    // jeśli klient nie ma żadnych wycieczek zwraca pustą listę
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id)
    {
        var trips = await _dbService.GetTripsForClientAsync(id);

        if (trips == null)
            return NotFound($"Client with id {id} does not exist.");
        
        return Ok(trips);
    }
    //dodaje nowego klienta na podstawie danych przesłanych w ciele żądania (JSON)
    //jeśli dane są niepoprawne  zwraca 400 BadRequest
    //jeśli sukces — zwraca 200 OK z ID nowego klienta
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newId = await _dbService.AddClientAsync(dto);
        return Ok(new { id = newId });
    }
    
    // rejestruje klienta na wycieczkę (jeśli obie strony istnieją i są miejsca)
    //zwraca komunikaty błędów lub status sukcesu
   
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
    {
        var result = await _dbService.RegisterClientToTripAsync(id, tripId);

        if (result == null)
            return Ok("Klient został zapisany na wycieczkę.");

        return BadRequest(result); 
    }
    
    //usuwa rejestrację klienta z wycieczki, jeśli istnieje
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientTrip(int id, int tripId)
    {
        var result = await _dbService.DeleteClientTripAsync(id, tripId);

        if (result == null)
            return Ok("Rejestracja została usunięta.");

        return NotFound(result);
    }

}