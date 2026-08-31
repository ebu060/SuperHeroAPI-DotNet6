using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<SuperHero>>> GetPaged(
            int page = 1,
            int pageSize = 10,
            string? universe = null,
            int? minPower = null,
            string? sortBy = null)
        {
            var query = _context.SuperHeroes.AsQueryable();

            if (!string.IsNullOrEmpty(universe))
                query = query.Where(h => h.Team != null && h.Team.Universe == universe);

            if (minPower.HasValue)
                query = query.Where(h => h.PowerLevel >= minPower.Value);

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

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<SuperHero>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
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
            dbHero.TeamId = request.TeamId;
            dbHero.PowerLevel = request.PowerLevel;
            dbHero.MissionsCompleted = request.MissionsCompleted;

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

    }
}
