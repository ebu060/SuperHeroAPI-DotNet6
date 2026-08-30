namespace SuperHeroAPI
{
    public class SuperHero
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; } = null;
    }
}
