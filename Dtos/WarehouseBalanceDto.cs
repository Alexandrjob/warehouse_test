namespace warehouse_api.Dtos;

public class WarehouseBalanceDto
{
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
