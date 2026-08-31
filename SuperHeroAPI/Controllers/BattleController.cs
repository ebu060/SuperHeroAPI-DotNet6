using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SuperHeroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BattleController : ControllerBase
    {
        private readonly DataContext _context;

        public BattleController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("simulate")]
        public async Task<ActionResult<Battle>> SimulateBattle([FromBody] BattleRequest request)
        {
            // Validation: Attacker and Defender cannot be the same hero
            if (request.AttackerHeroId == request.DefenderHeroId)
            {
                return BadRequest("Attacker and Defender cannot be the same hero.");
            }

            // Status Check: Both heroes must have IsBusy == false
            var attacker = await _context.SuperHeroes.FindAsync(request.AttackerHeroId);
            var defender = await _context.SuperHeroes.FindAsync(request.DefenderHeroId);

            if (attacker == null || defender == null)
            {
                return BadRequest("One or both heroes not found.");
            }

            if (attacker.IsBusy || defender.IsBusy)
            {
                return Conflict("One or both heroes are currently busy.");
            }

            // Lock: Set IsBusy = true for both heroes during battle calculation within a DB transaction
            attacker.IsBusy = true;
            defender.IsBusy = true;

            await _context.SaveChangesAsync();

            Battle? battle = null;
            try
            {
                // Perform battle simulation logic
                battle = await SimulateBattleLogic(attacker, defender);

                // Save battle and logs
                _context.Battles.Add(battle);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Unlock heroes in case of error
                attacker.IsBusy = false;
                defender.IsBusy = false;
                await _context.SaveChangesAsync();
                throw;
            }

            return Ok(battle);
        }

        private async Task<Battle> SimulateBattleLogic(SuperHero attacker, SuperHero defender)
        {
            var battle = new Battle
            {
                AttackerHeroId = attacker.Id,
                DefenderHeroId = defender.Id,
                AttackerInitialElo = attacker.EloRating,
                DefenderInitialElo = defender.EloRating
            };

            // Base HP for each hero = 100 + (Defense * 2)
            int attackerHp = 100 + (attacker.Defense * 2);
            int defenderHp = 100 + (defender.Defense * 2);

            // Initialize health values
            attacker.Health = attackerHp;
            defender.Health = defenderHp;

            int round = 0;
            int maxRounds = 20;
            bool battleFinished = false;

            // Turn order determined by Speed (faster hero attacks first)
            var (firstAttacker, firstDefender) = attacker.Speed >= defender.Speed
                ? (attacker, defender)
                : (defender, attacker);

            while (!battleFinished && round < maxRounds)
            {
                round++;

                // Damage calculation: Math.Max(5, Attacker.AttackPower - (Defender.Defense / 2)) + Random variance (0 to 5)
                int attackerDamage = Math.Max(5, firstAttacker.AttackPower - (firstDefender.Defense / 2)) + new Random().Next(0, 6);
                int defenderDamage = Math.Max(5, firstDefender.AttackPower - (firstAttacker.Defense / 2)) + new Random().Next(0, 6);

                // Apply damage
                firstDefender.Health -= attackerDamage;
                firstAttacker.Health -= defenderDamage;

                // Update HP for battle logs
                int attackerHpRemaining = Math.Max(0, firstAttacker.Health);
                int defenderHpRemaining = Math.Max(0, firstDefender.Health);

                // Add combat log
                var battleLog = new BattleLog
                {
                    BattleId = battle.Id,
                    RoundNumber = round,
                    AttackerDamageDealt = attackerDamage,
                    DefenderDamageDealt = defenderDamage,
                    AttackerRemainingHp = attackerHpRemaining,
                    DefenderRemainingHp = defenderHpRemaining,
                    RoundSummary = $"Round {round}: {firstAttacker.Name} dealt {attackerDamage} damage to {firstDefender.Name}. " +
                                   $"{firstDefender.Name} dealt {defenderDamage} damage to {firstAttacker.Name}."
                };

                battle.CombatLogs.Add(battleLog);

                // Check if battle is finished
                if (firstAttacker.Health <= 0 || firstDefender.Health <= 0)
                {
                    battleFinished = true;

                    // Determine winner
                    if (firstAttacker.Health <= 0 && firstDefender.Health <= 0)
                    {
                        battle.Status = "Draw";
                    }
                    else if (firstAttacker.Health <= 0)
                    {
                        battle.Status = "Finished";
                        battle.WinnerHeroId = defender.Id;
                        defender.Wins++;
                        attacker.Losses++;
                    }
                    else
                    {
                        battle.Status = "Finished";
                        battle.WinnerHeroId = attacker.Id;
                        attacker.Wins++;
                        defender.Losses++;
                    }
                }
            }

            // If max rounds reached without a winner, it's a draw
            if (!battleFinished)
            {
                battle.Status = "Draw";
            }

            battle.TotalRounds = round;

            // ELO Calculation (K-factor = 32)
            CalculateEloRating(attacker, defender, battle);

            return battle;
        }

        private void CalculateEloRating(SuperHero attacker, SuperHero defender, Battle battle)
        {
            const int K = 32;

            // Calculate expected scores
            double expectedAttackerScore = 1.0 / (1.0 + Math.Pow(10, (defender.EloRating - attacker.EloRating) / 400.0));
            double expectedDefenderScore = 1.0 / (1.0 + Math.Pow(10, (attacker.EloRating - defender.EloRating) / 400.0));

            // Update ELO ratings based on battle result
            if (battle.Status == "Finished" && battle.WinnerHeroId == attacker.Id)
            {
                // Attacker wins
                attacker.EloRating += (int)(K * (1 - expectedAttackerScore));
                defender.EloRating += (int)(K * (0 - expectedDefenderScore));

                battle.AttackerEloChange = (int)(K * (1 - expectedAttackerScore));
                battle.DefenderEloChange = (int)(K * (0 - expectedDefenderScore));
            }
            else if (battle.Status == "Finished" && battle.WinnerHeroId == defender.Id)
            {
                // Defender wins
                attacker.EloRating += (int)(K * (0 - expectedAttackerScore));
                defender.EloRating += (int)(K * (1 - expectedDefenderScore));

                battle.AttackerEloChange = (int)(K * (0 - expectedAttackerScore));
                battle.DefenderEloChange = (int)(K * (1 - expectedDefenderScore));
            }
            else
            {
                // Draw
                attacker.EloRating += (int)(K * (0.5 - expectedAttackerScore));
                defender.EloRating += (int)(K * (0.5 - expectedDefenderScore));

                battle.AttackerEloChange = (int)(K * (0.5 - expectedAttackerScore));
                battle.DefenderEloChange = (int)(K * (0.5 - expectedDefenderScore));
            }
        }

        [HttpGet("history/{heroId}")]
        public async Task<ActionResult<IEnumerable<Battle>>> GetBattleHistory(int heroId)
        {
            var battles = await _context.Battles
                .Where(b => b.AttackerHeroId == heroId || b.DefenderHeroId == heroId)
                .OrderByDescending(b => b.BattleDate)
                .ToListAsync();

            return Ok(battles);
        }

        [HttpGet("leaderboard")]
        public async Task<ActionResult<IEnumerable<SuperHero>>> GetLeaderboard(int top = 10)
        {
            var heroes = await _context.SuperHeroes
                .OrderByDescending(h => h.EloRating)
                .Take(top)
                .ToListAsync();

            return Ok(heroes);
        }

        // Request DTO for battle simulation
        public class BattleRequest
        {
            public int AttackerHeroId { get; set; }
            public int DefenderHeroId { get; set; }
        }
    }
}