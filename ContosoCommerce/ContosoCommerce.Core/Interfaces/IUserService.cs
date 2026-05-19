using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// User management and authentication ops.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        Task<UserDto> GetUserAsync(int userId);

        /// <summary>
        /// Returns all users with pagination.
        /// </summary>
        Task<IList<UserDto>> GetAllUsersAsync(
            int page, int pageSize);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        Task<UserDto> CreateUserAsync(
            CreateUserRequest request);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task<UserDto> UpdateUserAsync(
            int userId,
            UpdateUserRequest request);

        /// <summary>
        /// Soft-deletes a user account.
        /// </summary>
        Task DeleteUserAsync(int userId);

        /// <summary>
        /// Authenticates and returns a token.
        /// </summary>
        Task<AuthResult> AuthenticateAsync(
            string email, string password);

        /// <summary>
        /// Validates a token and returns user ID.
        /// </summary>
        Task<int?> ValidateTokenAsync(string token);

        /// <summary>
        /// Checks if user has specified role.
        /// </summary>
        Task<bool> HasRoleAsync(
            int userId, UserRole role);

        /// <summary>
        /// Gets total user count.
        /// </summary>
        Task<int> GetUserCountAsync();
    }

    [System.Serializable]
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }

    [System.Serializable]
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
    }

    [System.Serializable]
    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
        public bool? IsActive { get; set; }
    }

    [System.Serializable]
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
        public string ErrorMessage { get; set; }
    }
}
