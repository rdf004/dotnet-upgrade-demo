using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;

namespace ContosoCommerce.Users.Repositories
{
    /// <summary>
    /// Data access for User entities. Uses raw
    /// SQL queries via Database.SqlQuery for
    /// "performance-critical" lookups (a pattern
    /// that needs updating in EF Core).
    /// </summary>
    public class UserRepository
    {
        private readonly CommerceDbContext _db;

        public UserRepository(
            CommerceDbContext context)
        {
            _db = context;
        }

        /// <summary>
        /// Uses raw SQL for fast email lookup.
        /// </summary>
        public User FindByEmail(string email)
        {
            var sql =
                "SELECT * FROM Users "
                + "WHERE Email = @p0 "
                + "AND IsActive = 1";
            return _db.Database
                .SqlQuery<User>(sql, email)
                .FirstOrDefault();
        }

        /// <summary>
        /// Uses raw SQL for fast ID lookup.
        /// </summary>
        public User FindById(int id)
        {
            var sql =
                "SELECT * FROM Users "
                + "WHERE Id = @p0";
            return _db.Database
                .SqlQuery<User>(sql, id)
                .FirstOrDefault();
        }

        public async Task<List<User>>
            GetAllAsync(int page, int pageSize)
        {
            return await _db.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _db.Users
                .CountAsync(u => u.IsActive);
        }

        public async Task<User> AddAsync(
            User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _db.Entry(user).State =
                EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<AuthToken>
            CreateTokenAsync(AuthToken token)
        {
            _db.AuthTokens.Add(token);
            await _db.SaveChangesAsync();
            return token;
        }

        public async Task<AuthToken>
            FindTokenAsync(string tokenValue)
        {
            return await _db.AuthTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == tokenValue
                    && !t.IsRevoked);
        }
    }
}
