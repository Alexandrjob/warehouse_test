namespace warehouse_api.Dtos;

public class ArrivalDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<ArrivalResourceDto> Resources { get; set; } = new();
}