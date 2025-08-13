namespace warehouse_api.Models;

public class Arrival
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<ArrivalResource> Resources { get; set; } = new List<ArrivalResource>();
}