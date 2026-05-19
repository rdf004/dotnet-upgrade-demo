using System;
using ContosoCommerce.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ContosoCommerce.Users.Services
{
    public class MemoryCacheService
        : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(
            IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
            where T : class
        {
            _cache.TryGetValue(
                key, out T value);
            return value;
        }

        public void Set<T>(
            string key,
            T value,
            TimeSpan expiration)
            where T : class
        {
            _cache.Set(
                key, value, expiration);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public bool Contains(string key)
        {
            return _cache.TryGetValue(
                key, out _);
        }
    }
}
