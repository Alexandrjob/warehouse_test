namespace warehouse_api.Models;

public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}
