using APBD_D_Cw7.Models;
using Microsoft.Data.SqlClient;

using System.Collections.Generic;



namespace APBD_D_Cw7.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;

    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<IEnumerable<TripDto>> GetTripsAsync()
    {
        var trips = new Dictionary<int, TripDto>();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

       

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                c.Name AS CountryName
            FROM Trip t
            LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.IdTrip";

        await using var command = new SqlCommand(query, connection);
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var idTrip = reader.GetInt32(0);

            if (!trips.ContainsKey(idTrip))
            {
                trips[idTrip] = new TripDto
                {
                    IdTrip = idTrip,
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DateFrom = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                    DateTo = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                    MaxPeople = reader.GetInt32(5),
                    Countries = new List<CountryDto>()
                };

                if (!reader.IsDBNull(6))
                {
                    trips[idTrip].Countries = trips[idTrip].Countries ?? new List<CountryDto>();
                    trips[idTrip].Countries.Add(new CountryDto
                    {
                        Name = reader.GetString(6)
                    });
                }
            }
        }
        return trips.Values;
    }

}