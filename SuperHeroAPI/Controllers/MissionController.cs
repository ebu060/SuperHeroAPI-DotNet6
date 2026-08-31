using Microsoft.AspNetCore.Mvc;

namespace SuperHeroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionController : ControllerBase
    {
        private readonly DataContext _context;

        public MissionController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<List<Mission>>> Create(Mission mission)
        {
            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            return Ok(await _context.Missions.ToListAsync());
        }

        [HttpPost("{missionId}/assign-heroes")]
        public async Task<ActionResult<List<Mission>>> AssignHeroes(int missionId, List<int> heroIds)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            if (heroIds.Count > 4)
                return BadRequest("Cannot assign more than 4 heroes to a mission.");

            var heroes = new List<SuperHero>();
            int totalPowerLevel = 0;

            foreach (var heroId in heroIds)
            {
                var hero = await _context.SuperHeroes.FindAsync(heroId);
                if (hero == null || hero.TeamId == null)
                    return BadRequest("One or more heroes not found or inactive.");

                heroes.Add(hero);
                totalPowerLevel += hero.PowerLevel;
            }

            if (totalPowerLevel < mission.Difficulty * 30)
                return BadRequest("Total power level of assigned heroes is insufficient for the mission difficulty.");

            mission.AssignedHeroes = heroes;
            await _context.SaveChangesAsync();

            return Ok(mission);
        }

        [HttpPost("{missionId}/complete")]
        public async Task<ActionResult<List<Mission>>> Complete(int missionId)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            mission.Status = "Completed";

            foreach (var hero in mission.AssignedHeroes)
            {
                hero.MissionsCompleted++;
            }

            await _context.SaveChangesAsync();

            return Ok(mission);
        }
    }
}