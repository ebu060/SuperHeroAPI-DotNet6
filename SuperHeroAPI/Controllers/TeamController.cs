using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SuperHeroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly DataContext _context;

        public TeamController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> Get()
        {
            var teams = await _context.Teams
                .Select(t => new
                {
                    Id = t.Id,
                    Name = t.Name,
                    Universe = t.Universe,
                    Description = t.Description,
                    ActiveHeroCount = t.Heroes.Count(h => h.TeamId == t.Id)
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpPost]
        public async Task<ActionResult<List<Team>>> Create(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return Ok(await _context.Teams.ToListAsync());
        }

        [HttpPost("{teamId}/add-hero/{heroId}")]
        public async Task<ActionResult<List<SuperHero>>> AddHeroToTeam(int teamId, int heroId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return BadRequest("Team not found.");

            var hero = await _context.SuperHeroes.FindAsync(heroId);
            if (hero == null)
                return BadRequest("Hero not found.");

            // Validation rule: A hero cannot join a team if their Universe does not match
            if (hero.Team != null && hero.Team.Universe != team.Universe)
                return BadRequest("Hero's universe does not match team's universe.");

            // Validation rule: A hero cannot join a team if they are inactive (not in any team currently)
            if (hero.TeamId.HasValue && hero.Team != null && hero.Team.Universe != team.Universe)
                return BadRequest("Hero is already assigned to a team with a different universe.");

            hero.TeamId = teamId;
            await _context.SaveChangesAsync();

            return Ok(await _context.SuperHeroes.ToListAsync());
        }
    }
}