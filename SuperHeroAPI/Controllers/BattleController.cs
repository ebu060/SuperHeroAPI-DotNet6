using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public async Task<ActionResult<Battle>> SimulateBattle([FromBody] BattleSimulationRequest request)
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                attacker.IsBusy = true;
                defender.IsBusy = true;
                await _context.SaveChangesAsync();

                // Turn-Based Combat Logic
                var battle = new Battle
                {
                    AttackerHeroId = request.AttackerHeroId,
                    DefenderHeroId = request.DefenderHeroId,
                    AttackerInitialElo = attacker.EloRating,
                    DefenderInitialElo = defender.EloRating,
                    BattleDate = DateTime.UtcNow
                };

                int attackerHp = 100 + (attacker.Defense * 2);
                int defenderHp = 100 + (defender.Defense * 2);

                int round = 0;
                int maxRounds = 20;
                bool isDraw = false;
                SuperHero? winner = null;

                var battleLogs = new List<BattleLog>();

                // Determine turn order based on speed
                var (firstAttacker, firstDefender) = attacker.Speed >= defender.Speed 
                    ? (attacker, defender) 
                    : (defender, attacker);

                while (round < maxRounds && attackerHp > 0 && defenderHp > 0)
                {
                    round++;

                    // Calculate damage
                    int attackerDamage = Math.Max(5, firstAttacker.AttackPower - (firstDefender.Defense / 2)) + new Random().Next(0, 6);
                    int defenderDamage = Math.Max(5, firstDefender.AttackPower - (firstAttacker.Defense / 2)) + new Random().Next(0, 6);

                    // Apply damage
                    if (firstAttacker == attacker)
                    {
                        defenderHp -= attackerDamage;
                        if (defenderHp <= 0) defenderHp = 0;
                    }
                    else
                    {
                        attackerHp -= attackerDamage;
                        if (attackerHp <= 0) attackerHp = 0;
                    }

                    if (firstDefender == defender)
                    {
                        attackerHp -= defenderDamage;
                        if (attackerHp <= 0) attackerHp = 0;
                    }
                    else
                    {
                        defenderHp -= defenderDamage;
                        if (defenderHp <= 0) defenderHp = 0;
                    }

                    // Create battle log entry
                    var battleLog = new BattleLog
                    {
                        BattleId = battle.Id,
                        RoundNumber = round,
                        AttackerDamageDealt = firstAttacker == attacker ? attackerDamage : 0,
                        DefenderDamageDealt = firstDefender == defender ? defenderDamage : 0,
                        AttackerRemainingHp = attackerHp,
                        DefenderRemainingHp = defenderHp,
                        RoundSummary = $"Round {round}: {firstAttacker.Name} dealt {attackerDamage} damage to {firstDefender.Name}. " +
                                       $"{firstDefender.Name} dealt {defenderDamage} damage to {firstAttacker.Name}."
                    };

                    battleLogs.Add(battleLog);

                    // Switch turn order for next round
                    (firstAttacker, firstDefender) = (firstDefender, firstAttacker);
                }

                // Determine winner or draw
                if (attackerHp <= 0 && defenderHp <= 0)
                {
                    battle.Status = "Draw";
                    isDraw = true;
                }
                else if (attackerHp <= 0)
                {
                    battle.Status = "Finished";
                    winner = defender;
                }
                else
                {
                    battle.Status = "Finished";
                    winner = attacker;
                }

                // ELO Calculation (K-factor = 32)
                int kFactor = 32;
                double expectedScoreAttacker = 1.0 / (1.0 + Math.Pow(10, (defender.EloRating - attacker.EloRating) / 400.0));
                double expectedScoreDefender = 1.0 / (1.0 + Math.Pow(10, (attacker.EloRating - defender.EloRating) / 400.0));

                int eloChangeAttacker = 0;
                int eloChangeDefender = 0;

                if (!isDraw)
                {
                    // Update ELO ratings
                    if (winner == attacker)
                    {
                        eloChangeAttacker = (int)Math.Round(kFactor * (1 - expectedScoreAttacker));
                        eloChangeDefender = (int)Math.Round(kFactor * (0 - expectedScoreDefender));
                    }
                    else
                    {
                        eloChangeAttacker = (int)Math.Round(kFactor * (0 - expectedScoreAttacker));
                        eloChangeDefender = (int)Math.Round(kFactor * (1 - expectedScoreDefender));
                    }

                    attacker.EloRating += eloChangeAttacker;
                    defender.EloRating += eloChangeDefender;

                    // Update wins/losses
                    if (winner == attacker)
                    {
                        attacker.Wins++;
                        defender.Losses++;
                    }
                    else
                    {
                        defender.Wins++;
                        attacker.Losses++;
                    }
                }

                battle.AttackerEloChange = eloChangeAttacker;
                battle.DefenderEloChange = eloChangeDefender;
                battle.TotalRounds = round;

                if (winner != null)
                {
                    battle.WinnerHeroId = winner.Id;
                }

                // Save battle and logs
                battle.CombatLogs = battleLogs;
                _context.Battles.Add(battle);
                await _context.SaveChangesAsync();

                // Unlock: Set IsBusy = false
                attacker.IsBusy = false;
                defender.IsBusy = false;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(battle);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
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
    }

    public class BattleSimulationRequest
    {
        [Required]
        public int AttackerHeroId { get; set; }
        
        [Required]
        public int DefenderHeroId { get; set; }
    }
}