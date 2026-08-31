using System.ComponentModel.DataAnnotations;

namespace SuperHeroAPI
{
    public class Mission
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public string Status { get; set; } = "Pending";
        public ICollection<SuperHero> AssignedHeroes { get; set; } = new List<SuperHero>();
    }
}