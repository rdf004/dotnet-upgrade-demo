using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using ContosoCommerce.Users.Repositories;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService>
            _log;
        private readonly UserRepository _repo;
        private readonly IAuditService _audit;
        private readonly CommerceDbContext _db;

        public UserService(
            CommerceDbContext context,
            IAuditService auditService,
            ILogger<UserService> logger)
        {
            _db = context;
            _repo =
                new UserRepository(context);
            _audit = auditService;
            _log = logger;
        }

        public async Task<UserDto>
            GetUserAsync(int userId)
        {
            _log.LogDebug(
                "Getting user {Id}", userId);

            var user =
                _repo.FindById(userId);
            if (user == null)
            {
                throw
                    new EntityNotFoundException(
                        "User", userId);
            }
            return MapToDto(user);
        }

        public async Task<IList<UserDto>>
            GetAllUsersAsync(
                int page, int pageSize)
        {
            var users = await _repo
                .GetAllAsync(page, pageSize);
            return users
                .Select(MapToDto)
                .ToList();
        }

        public async Task<UserDto>
            CreateUserAsync(
                CreateUserRequest request)
        {
            _log.LogInformation(
                "Creating user: {Email}",
                request.Email);

            var existing = _repo
                .FindByEmail(request.Email);
            if (existing != null)
            {
                throw
                    new BusinessRuleException(
                        "DuplicateEmail",
                        "A user with this"
                        + " email already"
                        + " exists.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash =
                    HashPassword(
                        request.Password),
                FirstName =
                    request.FirstName,
                LastName =
                    request.LastName,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);

            await _audit.LogAsync(
                "User", user.Id,
                "Create",
                string.Format(
                    "User {0} created",
                    user.Email));

            return MapToDto(user);
        }

        public async Task<UserDto>
            UpdateUserAsync(
                int userId,
                UpdateUserRequest request)
        {
            _log.LogInformation(
                "Updating user {Id}",
                userId);

            var user =
                _repo.FindById(userId);
            if (user == null)
            {
                throw
                    new EntityNotFoundException(
                        "User", userId);
            }

            if (!string.IsNullOrEmpty(
                request.Email))
            {
                user.Email = request.Email;
            }
            if (!string.IsNullOrEmpty(
                request.FirstName))
            {
                user.FirstName =
                    request.FirstName;
            }
            if (!string.IsNullOrEmpty(
                request.LastName))
            {
                user.LastName =
                    request.LastName;
            }
            user.Role = request.Role;
            if (request.IsActive.HasValue)
            {
                user.IsActive =
                    request.IsActive.Value;
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);

            await _audit.LogAsync(
                "User", user.Id,
                "Update",
                string.Format(
                    "User {0} updated",
                    user.Email));

            return MapToDto(user);
        }

        public async Task DeleteUserAsync(
            int userId)
        {
            _log.LogInformation(
                "Deleting user {Id}",
                userId);

            var user =
                _repo.FindById(userId);
            if (user == null)
            {
                throw
                    new EntityNotFoundException(
                        "User", userId);
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            await _audit.LogAsync(
                "User", user.Id,
                "Delete",
                string.Format(
                    "User {0} soft-deleted",
                    user.Email));
        }

        public async Task<AuthResult>
            AuthenticateAsync(
                string email,
                string password)
        {
            _log.LogInformation(
                "Auth attempt for {Email}",
                email);

            var user =
                _repo.FindByEmail(email);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage =
                        "Invalid credentials."
                };
            }

            if (!VerifyPassword(
                password,
                user.PasswordHash))
            {
                _log.LogWarning(
                    "Failed login for {Email}",
                    email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage =
                        "Invalid credentials."
                };
            }

            var token = Guid.NewGuid()
                .ToString("N")
                + Guid.NewGuid()
                    .ToString("N");

            var authToken = new AuthToken
            {
                Token = token,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt =
                    DateTime.UtcNow
                        .AddHours(24),
                IsRevoked = false
            };

            await _repo
                .CreateTokenAsync(authToken);

            _log.LogInformation(
                "User {Email} authenticated",
                email);

            return new AuthResult
            {
                Success = true,
                Token = token,
                User = MapToDto(user)
            };
        }

        public async Task<int?>
            ValidateTokenAsync(string token)
        {
            var authToken = await _repo
                .FindTokenAsync(token);

            if (authToken == null
                || authToken.ExpiresAt
                    <= DateTime.UtcNow)
            {
                return null;
            }
            return authToken.UserId;
        }

        public async Task<bool> HasRoleAsync(
            int userId, UserRole role)
        {
            var user =
                _repo.FindById(userId);
            if (user == null) return false;
            return user.Role == role
                || user.Role
                    == UserRole.Administrator;
        }

        public async Task<int>
            GetUserCountAsync()
        {
            return await _repo.CountAsync();
        }

        internal static string HashPassword(
            string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(
                Encoding.UTF8.GetBytes(
                    password));
            return Convert
                .ToHexString(bytes);
        }

        internal static bool VerifyPassword(
            string password, string stored)
        {
            var hash = HashPassword(password);
            return string.Equals(
                hash, stored,
                StringComparison
                    .OrdinalIgnoreCase);
        }

        private static UserDto MapToDto(
            User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
