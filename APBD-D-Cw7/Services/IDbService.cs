using APBD_D_Cw7.Models;

namespace APBD_D_Cw7.Services;

public interface IDbService
{
    Task<IEnumerable<TripDto>> GetTripsAsync();
}