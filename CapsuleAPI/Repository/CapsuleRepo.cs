using CapsuleAPI.Data;
using CapsuleAPI.DTO;
using CapsuleAPI.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CapsuleAPI.Repository
{
    public class CapsuleRepo
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;

        public CapsuleRepo(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        public async Task AddAsync(TimeCapsule capsule)
        {
            await _appDbContext.TimeCapsules.AddAsync(capsule);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<(List<TimeCapsule> capsules, int totalCount)> GetCapsuleByUserAsync(
    string userId, string sortBy = "unlockDate", string sortDirection = "desc",
    int pageIndex = 0, int pageSize = 10, string? search = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            sortBy = sortBy?.ToLower() ?? "unlockDate";
            sortDirection = sortDirection?.ToLower() ?? "desc";

            if (sortBy != "title" && sortBy != "unlockDate" && sortBy != "createdAt")
            {
                sortBy = "unlockDate";
            }

            if (sortDirection != "asc" && sortDirection != "desc")
            {
                sortDirection = "desc";
            }

            pageIndex = Math.Max(0, pageIndex);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var query = _appDbContext.TimeCapsules
                .Where(c => c.UserId == userId || c.Contributors.Any(con => con.UserId == userId))
                .AsQueryable();

            var filterStart = stopwatch.ElapsedMilliseconds;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.Title != null && c.Title.ToLower().Contains(search)) ||
                    (search.Contains("unsealed") && !c.UnlockDate.HasValue) ||
                    (search.Contains("sealed") && c.UnlockDate.HasValue && c.UnlockDate > DateTime.UtcNow) ||
                    (search.Contains("unlocked") && c.UnlockDate.HasValue && c.UnlockDate <= DateTime.UtcNow));
            }
            Console.WriteLine($"Filter application took: {stopwatch.ElapsedMilliseconds - filterStart} ms");

            var countStart = stopwatch.ElapsedMilliseconds;
            var totalCount = await query.CountAsync();
            Console.WriteLine($"Count query took: {stopwatch.ElapsedMilliseconds - countStart} ms");

            query = (sortBy, sortDirection) switch
            {
                ("title", "asc") => query.OrderBy(c => c.Title ?? ""),
                ("title", "desc") => query.OrderByDescending(c => c.Title ?? ""),
                ("unlockDate", "asc") => query
                    .OrderBy(c => c.UnlockDate.HasValue ? 0 : 1)
                    .ThenBy(c => c.UnlockDate ?? DateTime.MaxValue)
                    .ThenByDescending(c => c.CreatedAt),
                ("unlockDate", "desc") => query
                    .OrderByDescending(c => c.UnlockDate.HasValue ? 0 : 1)
                    .ThenByDescending(c => c.UnlockDate ?? DateTime.MaxValue)
                    .ThenByDescending(c => c.CreatedAt),
                ("createdAt", "asc") => query.OrderBy(c => c.CreatedAt),
                ("createdAt", "desc") => query.OrderByDescending(c => c.CreatedAt),
                _ => query
                    .OrderByDescending(c => c.UnlockDate.HasValue ? 0 : 1)
                    .ThenByDescending(c => c.UnlockDate ?? DateTime.MaxValue)
                    .ThenByDescending(c => c.CreatedAt)
            };

            var fetchStart = stopwatch.ElapsedMilliseconds;
            var capsules = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            Console.WriteLine($"Fetch query took: {stopwatch.ElapsedMilliseconds - fetchStart} ms");

            var relatedStart = stopwatch.ElapsedMilliseconds;
            foreach (var capsule in capsules)
            {
                await _appDbContext.Entry(capsule)
                    .Collection(c => c.Contributors)
                    .LoadAsync();
                await _appDbContext.Entry(capsule)
                    .Collection(c => c.Contributions)
                    .Query()
                    .Include(con => con.User)
                    .LoadAsync();
            }
            Console.WriteLine($"Related data load took: {stopwatch.ElapsedMilliseconds - relatedStart} ms");

            stopwatch.Stop();
            Console.WriteLine($"Total query time: {stopwatch.ElapsedMilliseconds} ms");

            return (capsules, totalCount);
        }

        public async Task<bool> InviteContributorAsync(InviteContributorDTO inviteDto, string createrUserId)
        {
            var capsule = await _appDbContext.TimeCapsules
                .FirstOrDefaultAsync(c => c.Id == inviteDto.CapsuleId && c.UserId == createrUserId);

            if (capsule == null || (capsule.UnlockDate.HasValue && capsule.UnlockDate <= DateTime.UtcNow))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(inviteDto.CollaboratorEmail);
            if (user == null)
            {
                return false;
            }

            var exists = await _appDbContext.Contributors
                .AnyAsync(c => c.CapsuleId == inviteDto.CapsuleId && c.UserId == user.Id);

            if (exists)
            {
                return false;
            }

            var contributor = new Contributor
            {
                CapsuleId = inviteDto.CapsuleId,
                UserId = user.Id,
            };

            _appDbContext.Contributors.Add(contributor);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeCapsule?> GetCapsuleByIdAsync(int capsuleId)
        {
            return await _appDbContext.TimeCapsules.FindAsync(capsuleId);
        }

        public async Task<bool> IsUserContributorAsync(int capsuleId, string userId)
        {
            return await _appDbContext.Contributors.AnyAsync(c => c.CapsuleId == capsuleId && c.UserId == userId);
        }

        public async Task AddContributionAsync(Contribution contribution)
        {
            await _appDbContext.Contributions.AddAsync(contribution);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<bool> SealCapsuleAsync(int capsuleId, DateTime unlockDate, string userId)
        {
            var capsule = await _appDbContext.TimeCapsules
               .FirstOrDefaultAsync(c => c.Id == capsuleId && c.UserId == userId);

            if (capsule == null)
            {
                return false;
            }

            capsule.UnlockDate = unlockDate;
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeCapsule?> GetCapsuleWithContributionsAsync(int capsuleId, string userId)
        {
            var capsule = await _appDbContext.TimeCapsules
                .Include(c => c.Contributors)
                .Include(c => c.Contributions)
                .ThenInclude(con => con.User)
                .FirstOrDefaultAsync(c => c.Id == capsuleId && (c.UserId == userId || c.Contributions.Any(con => con.UserId == userId)));

            if (capsule == null || (capsule.UnlockDate.HasValue && capsule.UnlockDate > DateTime.UtcNow))
            {
                return null;
            }

            return capsule;
        }
    }
}