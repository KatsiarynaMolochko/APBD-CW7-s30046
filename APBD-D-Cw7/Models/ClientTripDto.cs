using System.Runtime.InteropServices.JavaScript;

namespace APBD_D_Cw7.Models;


public class ClientTripDto
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DateFrom { get; set; }
    public string DateTo { get; set; }
    public int MaxPeople { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}
