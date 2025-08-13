using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouse_api.Data;
using warehouse_api.Dtos;
using warehouse_api.Models;

namespace warehouse_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourcesController : ControllerBase
{
    private readonly WarehouseContext _context;
    private readonly IMapper _mapper;

    public ResourcesController(WarehouseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources([FromQuery] string? name, [FromQuery] bool? isArchived)
    {
        var query = _context.Resources.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(r => r.Name.Contains(name));
        }

        if (isArchived.HasValue)
        {
            query = query.Where(r => r.IsArchived == isArchived.Value);
        }

        var resources = await query.ToListAsync();
        return _mapper.Map<List<ResourceDto>>(resources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        return _mapper.Map<ResourceDto>(resource);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutResource([FromRoute] int id, [FromBody] ResourceDto resourceDto)
    {
        if (id != resourceDto.Id)
        {
            return BadRequest();
        }

        var resourceToUpdate = await _context.Resources.FindAsync(id);
        if (resourceToUpdate == null)
        {
            return NotFound();
        }

        if (resourceToUpdate.IsArchived)
        {
            return BadRequest("Невозможно изменить заархивированный ресурс.");
        }

        if (await _context.Resources.AnyAsync(r => r.Name == resourceDto.Name && r.Id != id))
        {
            return Conflict($"Ресурс с именем '{resourceDto.Name}' уже существует.");
        }

        _mapper.Map(resourceDto, resourceToUpdate);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceExists(id))
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
    public async Task<ActionResult<ResourceDto>> PostResource(ResourceDto resourceDto)
    {
        var resource = _mapper.Map<Resource>(resourceDto);

        if (await _context.Resources.AnyAsync(r => r.Name == resource.Name))
        {
            return Conflict($"Ресурс с именем '{resource.Name}' уже существует.");
        }

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetResource", new { id = resource.Id }, _mapper.Map<ResourceDto>(resource));
    }

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchiveResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        if (resource.IsArchived)
        {
            return BadRequest("Ресурс уже находится в архиве.");
        }

        var isUsed = await _context.ArrivalResources.AnyAsync(ar => ar.ResourceId == id);
        if (isUsed)
        {
            return BadRequest("Невозможно заархивировать используемый ресурс.");
        }

        resource.IsArchived = true;
        _context.Entry(resource).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ResourceExists(int id)
    {
        return _context.Resources.Any(e => e.Id == id);
    }
}
