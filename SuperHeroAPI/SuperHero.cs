namespace SuperHeroAPI
{
    public class SuperHero
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
        public int PowerLevel { get; set; } = 50;
        public int MissionsCompleted { get; set; } = 0;
    }
}