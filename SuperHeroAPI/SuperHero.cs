namespace SuperHeroAPI
{
    public class SuperHero
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;

        // New properties for battle simulation
        public int AttackPower { get; set; } = 50;
        public int Defense { get; set; } = 30;
        public int Speed { get; set; } = 40;
        public int EloRating { get; set; } = 1200;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public bool IsBusy { get; set; } = false;

        // Health property for battle simulation
        public int Health { get; set; }
    }
}
