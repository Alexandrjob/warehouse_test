namespace warehouse_api.Models;

public class ArrivalResource
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public int Quantity { get; set; }
}