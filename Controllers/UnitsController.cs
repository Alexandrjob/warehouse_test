using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouse_api.Data;
using warehouse_api.Dtos;
using warehouse_api.Models;

namespace warehouse_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnitsController : ControllerBase
{
    private readonly WarehouseContext _context;
    private readonly IMapper _mapper;

    public UnitsController(WarehouseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits([FromQuery] string? name, [FromQuery] bool? isArchived)
    {
        var query = _context.Units.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(u => u.Name.Contains(name));
        }

        if (isArchived.HasValue)
        {
            query = query.Where(u => u.IsArchived == isArchived.Value);
        }

        var units = await query.ToListAsync();
        return _mapper.Map<List<UnitDto>>(units);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UnitDto>> GetUnit(int id)
    {
        var unit = await _context.Units.FindAsync(id);

        if (unit == null)
        {
            return NotFound();
        }

        return _mapper.Map<UnitDto>(unit);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUnit([FromRoute] int id, [FromBody] UnitDto unitDto)
    {
        if (id != unitDto.Id)
        {
            return BadRequest();
        }

        var unitToUpdate = await _context.Units.FindAsync(id);
        if (unitToUpdate == null)
        {
            return NotFound();
        }

        if (unitToUpdate.IsArchived)
        {
            return BadRequest("Невозможно изменить заархивированную единицу измерения.");
        }

        if (await _context.Units.AnyAsync(u => u.Name == unitDto.Name && u.Id != id))
        {
            return Conflict($"Единица измерения с именем '{unitDto.Name}' уже существует.");
        }

        _mapper.Map(unitDto, unitToUpdate);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UnitExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<UnitDto>> PostUnit(UnitDto unitDto)
    {
        var unit = _mapper.Map<Unit>(unitDto);
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUnit", new { id = unit.Id }, _mapper.Map<UnitDto>(unit));
    }

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchiveUnit(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
        {
            return NotFound();
        }

        if (unit.IsArchived)
        {
            return BadRequest("Единица измерения уже находится в архиве.");
        }

        var isUsed = await _context.ArrivalResources.AnyAsync(ar => ar.UnitId == id);
        if (isUsed)
        {
            return BadRequest("Невозможно заархивировать используемую единицу измерения.");
        }

        unit.IsArchived = true;
        _context.Entry(unit).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UnitExists(int id)
    {
        return _context.Units.Any(e => e.Id == id);
    }
}
