using CapsuleAPI.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CapsuleAPI.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<TimeCapsule> TimeCapsules { get; set; }

        public DbSet<Contributor> Contributors { get; set; }
        public DbSet<Contribution> Contributions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Contributor>()
                .HasOne(c => c.Capsule)
                .WithMany(c => c.Contributors)
                .HasForeignKey(c => c.CapsuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Contributor>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Contribution>()
                .HasOne(c => c.Capsule)
                .WithMany(c => c.Contributions)
                .HasForeignKey(c => c.CapsuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Contribution>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
