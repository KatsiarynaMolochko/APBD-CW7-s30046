using APBD_D_Cw7.Models;
using Microsoft.Data.SqlClient;

using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;


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

    public async Task<IEnumerable<ClientTripDto>> GetTripsForClientAsync(int id)
    {
        var trips = new List<ClientTripDto>();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var checkClientQuery = "SELECT 1 FROM Client WHERE IdClient = @id";
        await using (var checkCmd = new SqlCommand(checkClientQuery, connection))
        {
            checkCmd.Parameters.AddWithValue("@id", id);
            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null)
            {
                return null;
            }
        }
        
        var query = @"
        SELECT 
            t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
            ct.RegisteredAt, ct.PaymentDate
        FROM Client_Trip ct
        JOIN Trip t ON ct.IdTrip = t.IdTrip
        WHERE ct.IdClient = @id";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trips.Add(new ClientTripDto
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2), 
                DateFrom = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                DateTo = reader.GetDateTime(4).ToString("yyyy-MM-dd"), 
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
            });
        }

        return trips;
    }

    public async Task<int> AddClientAsync(CreateClientDto clientDto)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var query = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
        command.Parameters.AddWithValue("@LastName", clientDto.LastName);
        command.Parameters.AddWithValue("@Email", clientDto.Email);
        command.Parameters.AddWithValue("@Telephone", (object?)clientDto.Telephone ?? DBNull.Value);
        command.Parameters.AddWithValue("@Pesel", clientDto.Pesel);

        var insertedId = (int)await command.ExecuteScalarAsync();
        return insertedId;

    }

    public async Task<string?> RegisterClientToTripAsync(int clientId, int tripId)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        try
    {
        //sprawdzenie czy klient isttnieje
        var checkClient = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @id", connection, (SqlTransaction)transaction);
        checkClient.Parameters.AddWithValue("@id", clientId);
        var clientExists = await checkClient.ExecuteScalarAsync();
        if (clientExists == null)
            return "Klient nie istnieje";

        //sprawdzenie czy wycieczka istnieje
        var checkTrip = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @id", connection, (SqlTransaction)transaction);
        checkTrip.Parameters.AddWithValue("@id", tripId);
        var maxPeopleObj = await checkTrip.ExecuteScalarAsync();
        if (maxPeopleObj == null)
            return "Wycieczka nie istnieje";

        var maxPeople = (int)maxPeopleObj;

        //liczba aktualnych zapisów
        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId", connection, (SqlTransaction)transaction);
        countCmd.Parameters.AddWithValue("@tripId", tripId);
        var count = (int)await countCmd.ExecuteScalarAsync();

        if (count >= maxPeople)
            return "Osiągnięto maksymalną liczbę uczestników";

        //srawdzenie czy już zapisany
        var existsCmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid", connection, (SqlTransaction)transaction);
        existsCmd.Parameters.AddWithValue("@cid", clientId);
        existsCmd.Parameters.AddWithValue("@tid", tripId);
        var already = await existsCmd.ExecuteScalarAsync();
        if (already != null)
            return "Klient już zapisany na tę wycieczkę";

        //wstawienie nowgo rekordu
        var insertCmd = new SqlCommand(@"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES (@cid, @tid, @date, NULL)", connection, (SqlTransaction)transaction);

        insertCmd.Parameters.AddWithValue("@cid", clientId);
        insertCmd.Parameters.AddWithValue("@tid", tripId);
        insertCmd.Parameters.AddWithValue("@date", int.Parse(DateTime.Now.ToString("yyyyMMdd"))); // np. 20250508

        await insertCmd.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
        return null; 
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }

    }

    public async Task<string?> DeleteClientTripAsync(int clientId, int tripId)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var checkQuery =  @"SELECT 1 FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid";
        var checkCmd = new SqlCommand(checkQuery, connection);
        checkCmd.Parameters.AddWithValue("@cid", clientId);
        checkCmd.Parameters.AddWithValue("@tid", tripId);
        
        var exists = await checkCmd.ExecuteScalarAsync();
        if (exists == null)
            return "Rejestracja nie istnieje";
        
        var deleteCmd = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid", connection);
        deleteCmd.Parameters.AddWithValue("@cid", clientId);
        deleteCmd.Parameters.AddWithValue("@tid", tripId);
        
        await deleteCmd.ExecuteNonQueryAsync();
        return null;
    }


}