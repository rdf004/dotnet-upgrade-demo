using System;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// Abstraction over in-memory caching.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieves an item from cache.
        /// </summary>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Stores an item in cache.
        /// </summary>
        void Set<T>(
            string key,
            T value,
            TimeSpan expiration)
            where T : class;

        /// <summary>
        /// Removes an item from cache.
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Checks if a key exists in cache.
        /// </summary>
        bool Contains(string key);
    }
}
