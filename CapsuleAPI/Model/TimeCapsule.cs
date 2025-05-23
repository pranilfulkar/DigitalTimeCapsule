using CapsuleAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace CapsuleAPI.Model
{
    public class TimeCapsule
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string Content { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime? UnlockDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }

        public AppUser User { get; set; }

        public ICollection<Contributor> Contributors { get; set; }

        public ICollection<Contribution> Contributions { get; set; }

    }
}
