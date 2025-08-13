namespace warehouse_api.Dtos;

public class UnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}