using System.Runtime.InteropServices.JavaScript;

namespace APBD_D_Cw7.Models;

public class TripDto
{
    public int IdTrip { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public String DateFrom { get; set; }
    public String DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDto> Countries { get; set; }
    
}
public class CountryDto
{
    public string Name { get; set; }
}