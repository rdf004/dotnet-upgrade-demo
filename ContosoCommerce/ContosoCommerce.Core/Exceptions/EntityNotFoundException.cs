using System;

namespace ContosoCommerce.Core.Exceptions
{
    /// <summary>
    /// Thrown when a requested entity does not
    /// exist in the data store.
    /// </summary>
    [Serializable]
    public class EntityNotFoundException
        : Exception
    {
        public string EntityType { get; private set; }
        public int EntityId { get; private set; }

        public EntityNotFoundException(
            string entityType, int entityId)
            : base(string.Format(
                "{0} with ID {1} was not found.",
                entityType, entityId))
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityNotFoundException(
            string message)
            : base(message)
        {
        }

        public EntityNotFoundException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        protected EntityNotFoundException(
            System.Runtime.Serialization
                .SerializationInfo info,
            System.Runtime.Serialization
                .StreamingContext context)
            : base(info, context)
        {
        }
    }
}
