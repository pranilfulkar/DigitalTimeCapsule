using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace CapsuleAPI.Data
{
    public class AppUser : IdentityUser
    {
        public string Password { get; set; }
    }
}
