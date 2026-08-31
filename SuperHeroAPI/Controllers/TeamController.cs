using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<Team>>> Get()
        {
            var teams = await _context.Teams.ToListAsync();

            // Add hero count for each team
            foreach (var team in teams)
            {
                team.Heroes = team.Heroes.Where(h => h.TeamId == team.Id).ToList();
            }

            return Ok(teams);
        }

        [HttpPost]
        public async Task<ActionResult<List<Team>>> CreateTeam(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return Ok(await _context.Teams.ToListAsync());
        }

        [HttpPost("{teamId}/add-hero/{heroId}")]
        public async Task<ActionResult<List<Team>>> AddHeroToTeam(int teamId, int heroId)
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

            // Validation rule: A hero cannot join a team if they are inactive (not part of any team)
            if (hero.TeamId.HasValue && hero.Team != null && hero.Team.Universe != team.Universe)
                return BadRequest("Hero's universe does not match team's universe.");

            hero.TeamId = teamId;
            team.Heroes.Add(hero);

            await _context.SaveChangesAsync();

            return Ok(await _context.Teams.ToListAsync());
        }
    }
}