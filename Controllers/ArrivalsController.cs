

using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouse_api.Data;
using warehouse_api.Dtos;
using warehouse_api.Models;

namespace warehouse_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArrivalsController : ControllerBase
{
    private readonly WarehouseContext _context;
    private readonly IValidator<Arrival> _validator;
    private readonly IMapper _mapper;

    public ArrivalsController(WarehouseContext context, IValidator<Arrival> validator, IMapper mapper)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArrivalDto>>> GetArrivals(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] string[]? numbers, 
        [FromQuery] int[]? resourceIds, 
        [FromQuery] int[]? unitIds)
    {
        var query = _context.Arrivals
            .Include(a => a.Resources)
            .ThenInclude(r => r.Resource)
            .Include(a => a.Resources)
            .ThenInclude(r => r.Unit)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Date <= endDate.Value.Date);
        }

        if (numbers != null && numbers.Length > 0)
        {
            query = query.Where(a => numbers.Contains(a.Number));
        }

        if (resourceIds != null && resourceIds.Length > 0)
        {
            query = query.Where(a => a.Resources.Any(r => resourceIds.Contains(r.ResourceId)));
        }

        if (unitIds != null && unitIds.Length > 0)
        {
            query = query.Where(a => a.Resources.Any(r => unitIds.Contains(r.UnitId)));
        }

        var arrivals = await query.ToListAsync();
        return _mapper.Map<List<ArrivalDto>>(arrivals);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArrivalDto>> GetArrival(int id)
    {
        var arrival = await _context.Arrivals
            .Include(a => a.Resources).ThenInclude(r => r.Resource)
            .Include(a => a.Resources).ThenInclude(r => r.Unit)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (arrival == null)
        {
            return NotFound();
        }

        return _mapper.Map<ArrivalDto>(arrival);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutArrival([FromRoute] int id, [FromBody] ArrivalDto arrivalDto)
    {
        if (id != arrivalDto.Id)
        {
            return BadRequest("Идентификаторы поступления в URL и в теле запроса не совпадают.");
        }

        var arrivalInDb = await _context.Arrivals
            .Include(a => a.Resources)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (arrivalInDb == null)
        {
            return NotFound("Документ поступления не найден.");
        }

        var proposedState = _mapper.Map<Arrival>(arrivalDto);

        var balanceCheckResult = await CheckBalance(arrivalInDb, proposedState);
        if (!balanceCheckResult.isValid)
        {
            return BadRequest(balanceCheckResult.errorMessage);
        }

        if (await _context.Arrivals.AnyAsync(a => a.Number == arrivalDto.Number && a.Id != id))
        {
            return Conflict($"Документ поступления с номером '{arrivalDto.Number}' уже существует.");
        }

        _mapper.Map(arrivalDto, arrivalInDb);
        
        var validationResult = await _validator.ValidateAsync(arrivalInDb);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ArrivalExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Arrivals_Number") ?? false)
        {
            return Conflict($"Документ поступления с номером '{arrivalDto.Number}' уже существует.");
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<ArrivalDto>> PostArrival(ArrivalDto arrivalDto)
    {
        var arrival = _mapper.Map<Arrival>(arrivalDto);

        if (await _context.Arrivals.AnyAsync(a => a.Number == arrival.Number))
        {
            return Conflict($"Документ поступления с номером '{arrival.Number}' уже существует.");
        }
        
        var resourceIds = arrival.Resources.Select(r => r.ResourceId).Distinct().ToList();
        var unitIds = arrival.Resources.Select(r => r.UnitId).Distinct().ToList();

        var archivedResources = await _context.Resources
            .Where(r => resourceIds.Contains(r.Id) && r.IsArchived)
            .Select(r => r.Name)
            .ToListAsync();

        if (archivedResources.Any())
        {
            return BadRequest($"Невозможно использовать архивные ресурсы: {string.Join(", ", archivedResources)}");
        }

        var archivedUnits = await _context.Units
            .Where(u => unitIds.Contains(u.Id) && u.IsArchived)
            .Select(u => u.Name)
            .ToListAsync();

        if (archivedUnits.Any())
        {
            return BadRequest($"Невозможно использовать архивные единицы измерения: {string.Join(", ", archivedUnits)}");
        }
        
        var validationResult = await _validator.ValidateAsync(arrival);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        _context.Arrivals.Add(arrival);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArrival), new { id = arrival.Id }, _mapper.Map<ArrivalDto>(arrival));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArrival(int id)
    {
        var arrival = await _context.Arrivals
            .Include(a => a.Resources)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (arrival == null)
        {
            return NotFound("Документ поступления не найден.");
        }

        var balanceCheckResult = await CheckBalance(arrival, null);
        if (!balanceCheckResult.isValid)
        {
            return BadRequest(balanceCheckResult.errorMessage);
        }

        _context.Arrivals.Remove(arrival);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ArrivalExists(int id)
    {
        return _context.Arrivals.Any(e => e.Id == id);
    }

    private async Task<(bool isValid, string errorMessage)> CheckBalance(Arrival oldArrival, Arrival? newArrival)
    {
        var oldItems = oldArrival.Resources.ToDictionary(r => (r.ResourceId, r.UnitId), r => r.Quantity);
        var newItems = newArrival?.Resources.ToDictionary(r => (r.ResourceId, r.UnitId), r => r.Quantity) 
                       ?? new Dictionary<(int, int), int>();

        var allKeys = oldItems.Keys.Union(newItems.Keys);

        foreach (var key in allKeys)
        {
            oldItems.TryGetValue(key, out var oldQuantity);
            newItems.TryGetValue(key, out var newQuantity);

            var delta = newQuantity - oldQuantity;

            if (delta < 0)
            {
                var totalBalanceInDb = await _context.ArrivalResources
                    .Where(ar => ar.ResourceId == key.ResourceId && ar.UnitId == key.UnitId)
                    .SumAsync(ar => ar.Quantity);

                if (totalBalanceInDb + delta < 0)
                {
                    var resource = await _context.Resources.FindAsync(key.ResourceId);
                    var unit = await _context.Units.FindAsync(key.UnitId);
                    return (false, $"Операция не удалась: приведет к отрицательному балансу для ресурса '{resource?.Name}' в единицах '{unit?.Name}'.");
                }
            }
        }

        return (true, string.Empty);
    }
}

