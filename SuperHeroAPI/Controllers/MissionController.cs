using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<List<Mission>>> CreateMission(Mission mission)
        {
            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            return Ok(await _context.Missions.ToListAsync());
        }

        [HttpPost("{missionId}/assign-heroes")]
        public async Task<ActionResult<List<Mission>>> AssignHeroesToMission(int missionId, [FromBody] List<int> heroIds)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            // Validation rule: Assign up to 4 active heroes
            if (heroIds.Count > 4)
                return BadRequest("Cannot assign more than 4 heroes to a mission.");

            var heroes = new List<SuperHero>();
            int totalPowerLevel = 0;

            foreach (var heroId in heroIds)
            {
                var hero = await _context.SuperHeroes.FindAsync(heroId);
                if (hero == null)
                    return BadRequest($"Hero with ID {heroId} not found.");

                // Check if hero is active (part of a team)
                if (hero.TeamId == null)
                    return BadRequest($"Hero with ID {heroId} is inactive and cannot be assigned to mission.");

                heroes.Add(hero);
                totalPowerLevel += hero.PowerLevel;
            }

            // Validation rule: Total combined PowerLevel must be >= Difficulty * 30
            if (totalPowerLevel < mission.Difficulty * 30)
                return BadRequest($"Total power level of assigned heroes ({totalPowerLevel}) is not sufficient for mission difficulty {mission.Difficulty}.");

            // Assign heroes to mission
            foreach (var hero in heroes)
            {
                if (!mission.AssignedHeroes.Contains(hero))
                {
                    mission.AssignedHeroes.Add(hero);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(await _context.Missions.ToListAsync());
        }

        [HttpPost("{missionId}/complete")]
        public async Task<ActionResult<List<Mission>>> CompleteMission(int missionId)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Set status to "Completed"
                mission.Status = "Completed";

                // Increment MissionsCompleted for all assigned heroes within a database transaction
                foreach (var hero in mission.AssignedHeroes)
                {
                    hero.MissionsCompleted++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(await _context.Missions.ToListAsync());
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}