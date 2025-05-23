using System.ComponentModel.DataAnnotations;
using CapsuleAPI.Data;

namespace CapsuleAPI.Model
{
    public class Contributor
    {
        public int Id { get; set; }

        [Required]
        public int CapsuleId { get; set; }
        public TimeCapsule Capsule { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
