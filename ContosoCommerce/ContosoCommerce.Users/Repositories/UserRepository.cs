using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContosoCommerce.Users.Repositories
{
    public class UserRepository
    {
        private readonly CommerceDbContext _db;

        public UserRepository(
            CommerceDbContext context)
        {
            _db = context;
        }

        public User FindByEmail(string email)
        {
            return _db.Users
                .FromSqlRaw(
                    "SELECT * FROM Users"
                    + " WHERE Email = {0}"
                    + " AND IsActive = 1",
                    email)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public User FindById(int id)
        {
            return _db.Users
                .FromSqlRaw(
                    "SELECT * FROM Users"
                    + " WHERE Id = {0}",
                    id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public async Task<List<User>>
            GetAllAsync(
                int page, int pageSize)
        {
            return await _db.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Id)
                .Skip(
                    (page - 1) * pageSize)
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

        public async Task UpdateAsync(
            User user)
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
            FindTokenAsync(
                string tokenValue)
        {
            return await _db.AuthTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == tokenValue
                    && !t.IsRevoked);
        }
    }
}
