using System.Threading.Tasks;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// Logs audit trail entries for all
    /// data-mutating operations.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Records an audit event.
        /// </summary>
        /// <param name="entityType">
        /// The type of entity changed.
        /// </param>
        /// <param name="entityId">
        /// The ID of the entity changed.
        /// </param>
        /// <param name="action">
        /// The action performed (Create, etc.).
        /// </param>
        /// <param name="details">
        /// Additional context about the change.
        /// </param>
        Task LogAsync(
            string entityType,
            int entityId,
            string action,
            string details);
    }
}
