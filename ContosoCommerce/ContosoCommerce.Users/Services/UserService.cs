using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using ContosoCommerce.Users.Repositories;
using log4net;

namespace ContosoCommerce.Users.Services
{
    /// <summary>
    /// Manages user CRUD and authentication.
    /// Uses the deprecated FormsAuthentication
    /// API for password hashing. This is a key
    /// migration target for .NET 8.
    /// </summary>
    public class UserService : IUserService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(UserService));

        private readonly UserRepository _repo;
        private readonly IAuditService _audit;
        private readonly CommerceDbContext _db;

        public UserService(
            CommerceDbContext context,
            IAuditService auditService)
        {
            _db = context;
            _repo = new UserRepository(context);
            _audit = auditService;
        }

        public async Task<UserDto> GetUserAsync(
            int userId)
        {
            Log.DebugFormat(
                "Getting user {0}", userId);

            var user = _repo.FindById(userId);
            if (user == null)
            {
                throw new EntityNotFoundException(
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
            Log.InfoFormat(
                "Creating user: {0}",
                request.Email);

            var existing = _repo
                .FindByEmail(request.Email);
            if (existing != null)
            {
                throw new BusinessRuleException(
                    "DuplicateEmail",
                    "A user with this email "
                    + "already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash =
                    HashPassword(
                        request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
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
            Log.InfoFormat(
                "Updating user {0}", userId);

            var user = _repo.FindById(userId);
            if (user == null)
            {
                throw new EntityNotFoundException(
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
            Log.InfoFormat(
                "Deleting user {0}", userId);

            var user = _repo.FindById(userId);
            if (user == null)
            {
                throw new EntityNotFoundException(
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
                string email, string password)
        {
            Log.InfoFormat(
                "Auth attempt for {0}", email);

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

            var hash =
                HashPassword(password);
            if (user.PasswordHash != hash)
            {
                Log.WarnFormat(
                    "Failed login for {0}",
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

            Log.InfoFormat(
                "User {0} authenticated",
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

        #pragma warning disable 618
        private static string HashPassword(
            string password)
        {
            return FormsAuthentication
                .HashPasswordForStoringInConfigFile(
                    password, "SHA256");
        }
        #pragma warning restore 618

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
