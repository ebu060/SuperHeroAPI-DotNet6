using System.ComponentModel.DataAnnotations;

namespace SuperHeroAPI
{
    public class Team
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Universe { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public ICollection<SuperHero> Heroes { get; set; } = new List<SuperHero>();
    }
}