using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SuperHeroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperHeroController : ControllerBase
    {
        private readonly DataContext _context;

        public SuperHeroController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> Get()
        {
            return Ok(await _context.SuperHeroes.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SuperHero>> Get(int id)
        {
            var hero = await _context.SuperHeroes.FindAsync(id);
            if (hero == null)
                return BadRequest("Hero not found.");
            return Ok(hero);
        }

        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> AddHero(SuperHero hero)
        {
            _context.SuperHeroes.Add(hero);
            await _context.SaveChangesAsync();

            return Ok(await _context.SuperHeroes.ToListAsync());
        }

        [HttpPut]
        public async Task<ActionResult<List<SuperHero>>> UpdateHero(SuperHero request)
        {
            var dbHero = await _context.SuperHeroes.FindAsync(request.Id);
            if (dbHero == null)
                return BadRequest("Hero not found.");

            dbHero.Name = request.Name;
            dbHero.FirstName = request.FirstName;
            dbHero.LastName = request.LastName;
            dbHero.Place = request.Place;

            await _context.SaveChangesAsync();

            return Ok(await _context.SuperHeroes.ToListAsync());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<List<SuperHero>>> Delete(int id)
        {
            var dbHero = await _context.SuperHeroes.FindAsync(id);
            if (dbHero == null)
                return BadRequest("Hero not found.");

            _context.SuperHeroes.Remove(dbHero);
            await _context.SaveChangesAsync();

            return Ok(await _context.SuperHeroes.ToListAsync());
        }

        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetPaged(
            int page = 1,
            int pageSize = 10,
            string? universe = null,
            int? minPower = null,
            string? sortBy = null)
        {
            var query = _context.SuperHeroes.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(universe))
            {
                query = query.Where(h => h.Team != null && h.Team.Universe == universe);
            }

            if (minPower.HasValue)
            {
                query = query.Where(h => h.PowerLevel >= minPower.Value);
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "power_desc":
                    query = query.OrderByDescending(h => h.PowerLevel);
                    break;
                case "power_asc":
                    query = query.OrderBy(h => h.PowerLevel);
                    break;
                default:
                    query = query.OrderBy(h => h.Id);
                    break;
            }

            // Calculate pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply paging
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items = items,
                totalCount = totalCount,
                pageNumber = page,
                totalPages = totalPages
            });
        }
    }
}
