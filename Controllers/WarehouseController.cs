using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouse_api.Data;
using warehouse_api.Dtos;

namespace warehouse_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly WarehouseContext _context;

    public WarehouseController(WarehouseContext context)
    {
        _context = context;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<IEnumerable<WarehouseBalanceDto>>> GetBalance([FromQuery] int[]? resourceIds, [FromQuery] int[]? unitIds)
    {
        var query = _context.ArrivalResources.AsQueryable();

        if (resourceIds != null && resourceIds.Length > 0)
        {
            query = query.Where(ar => resourceIds.Contains(ar.ResourceId));
        }

        if (unitIds != null && unitIds.Length > 0)
        {
            query = query.Where(ar => unitIds.Contains(ar.UnitId));
        }

        var balance = await query
            .GroupBy(ar => new { ar.ResourceId, ar.UnitId })
            .Select(g => new WarehouseBalanceDto
            {
                ResourceId = g.Key.ResourceId,
                UnitId = g.Key.UnitId,
                ResourceName = g.First().Resource.Name,
                UnitName = g.First().Unit.Name,
                Quantity = g.Sum(ar => ar.Quantity)
            })
            .ToListAsync();

        return balance;
    }
}