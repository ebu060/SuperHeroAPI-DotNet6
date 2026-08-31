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
        public async Task<ActionResult<List<Mission>>> Create(Mission mission)
        {
            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            return Ok(await _context.Missions.ToListAsync());
        }

        [HttpPost("{missionId}/assign-heroes")]
        public async Task<ActionResult<List<SuperHero>>> AssignHeroesToMission(int missionId, [FromBody] List<int> heroIds)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            // Validation rule: Cannot assign more than 4 heroes
            if (heroIds.Count > 4)
                return BadRequest("Cannot assign more than 4 heroes to a mission.");

            var heroes = await _context.SuperHeroes
                .Where(h => heroIds.Contains(h.Id))
                .ToListAsync();

            // Check if all heroes exist
            if (heroes.Count != heroIds.Count)
                return BadRequest("One or more heroes not found.");

            // Validation rule: Total combined PowerLevel of assigned heroes must be >= Difficulty * 30
            var totalPowerLevel = heroes.Sum(h => h.PowerLevel);
            if (totalPowerLevel < mission.Difficulty * 30)
                return BadRequest($"Total power level ({totalPowerLevel}) is not sufficient for mission difficulty {mission.Difficulty}.");

            // Assign heroes to mission
            foreach (var hero in heroes)
            {
                if (!mission.AssignedHeroes.Contains(hero))
                {
                    mission.AssignedHeroes.Add(hero);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(heroes);
        }

        [HttpPost("{missionId}/complete")]
        public async Task<ActionResult> CompleteMission(int missionId)
        {
            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                return BadRequest("Mission not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Set mission status to completed
                mission.Status = "Completed";

                // Increment MissionsCompleted for all assigned heroes
                foreach (var hero in mission.AssignedHeroes)
                {
                    hero.MissionsCompleted++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Mission completed successfully.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}