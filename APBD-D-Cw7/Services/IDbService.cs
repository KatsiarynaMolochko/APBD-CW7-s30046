using APBD_D_Cw7.Models;

namespace APBD_D_Cw7.Services;

public interface IDbService
{
    Task<IEnumerable<TripDto>> GetTripsAsync();
    Task<IEnumerable<ClientTripDto>> GetTripsForClientAsync(int id);
    Task<int> AddClientAsync(CreateClientDto clientDto);
    Task<string?> RegisterClientToTripAsync(int clientId, int tripId);
    Task<string?> DeleteClientTripAsync(int clientId, int tripId);


    
}