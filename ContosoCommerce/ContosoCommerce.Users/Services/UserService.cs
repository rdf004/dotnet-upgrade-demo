using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using ContosoCommerce.Users.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Services
{
    public class UserService : IUserService
    {
        private readonly
            ILogger<UserService> _log;
        private readonly
            UserRepository _repo;
        private readonly IAuditService _audit;
        private readonly
            PasswordHasher<User> _hasher;

        public UserService(
            UserRepository repo,
            IAuditService auditService,
            ILogger<UserService> logger)
        {
            _repo = repo;
            _audit = auditService;
            _log = logger;
            _hasher =
                new PasswordHasher<User>();
        }

        public async Task<UserDto>
            GetUserAsync(int userId)
        {
            _log.LogDebug(
                "Getting user {Id}",
                userId);

            var user = await _repo
                .FindByIdAsync(userId);
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

            var existing = await _repo
                .FindByEmailAsync(
                    request.Email);
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
                FirstName =
                    request.FirstName,
                LastName =
                    request.LastName,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash =
                _hasher.HashPassword(
                    user, request.Password);

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

            var user = await _repo
                .FindByIdAsync(userId);
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

            var user = await _repo
                .FindByIdAsync(userId);
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

            var user = await _repo
                .FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage =
                        "Invalid credentials."
                };
            }

            var result =
                _hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password);
            if (result
                == PasswordVerificationResult
                    .Failed)
            {
                _log.LogWarning(
                    "Failed login: {Email}",
                    email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage =
                        "Invalid credentials."
                };
            }

            var tokenBytes =
                RandomNumberGenerator
                    .GetBytes(32);
            var token = Convert
                .ToBase64String(tokenBytes);

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

        public async Task<bool>
            HasRoleAsync(
                int userId, UserRole role)
        {
            var user = await _repo
                .FindByIdAsync(userId);
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
