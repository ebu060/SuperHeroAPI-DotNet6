using System.ComponentModel.DataAnnotations;

namespace SuperHeroAPI
{
    public class Battle
    {
        public int Id { get; set; }
        
        public int AttackerHeroId { get; set; }
        public SuperHero AttackerHero { get; set; } = null!;
        
        public int DefenderHeroId { get; set; }
        public SuperHero DefenderHero { get; set; } = null!;
        
        public int? WinnerHeroId { get; set; }
        
        public int AttackerInitialElo { get; set; }
        public int DefenderInitialElo { get; set; }
        
        public int AttackerEloChange { get; set; }
        public int DefenderEloChange { get; set; }
        
        public int TotalRounds { get; set; }
        
        public DateTime BattleDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(20)]
        public string Status { get; set; } = "InProgress";
        
        public ICollection<BattleLog> CombatLogs { get; set; } = new List<BattleLog>();
    }
}