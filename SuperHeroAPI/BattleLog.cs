namespace SuperHeroAPI
{
    public class BattleLog
    {
        public int Id { get; set; }
        
        public int BattleId { get; set; }
        public Battle Battle { get; set; } = null!;
        
        public int RoundNumber { get; set; }
        
        public int AttackerDamageDealt { get; set; }
        public int DefenderDamageDealt { get; set; }
        
        public int AttackerRemainingHp { get; set; }
        public int DefenderRemainingHp { get; set; }
        
        public string RoundSummary { get; set; } = string.Empty;
    }
}