using System.ComponentModel.DataAnnotations;

namespace SuperHeroAPI
{
    public class Mission
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Difficulty { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public ICollection<SuperHero> AssignedHeroes { get; set; } = new List<SuperHero>();
    }
}