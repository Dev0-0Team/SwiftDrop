//Done
namespace SwiftDrop.Services.Interfaces
{
    public interface ICacheService
    {

        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan expirationTime, TimeSpan slidingTime);
        void Remove(string key);
        Task<(T? Data,bool FromCache)> GetOrCreateAsync<T>(string key, Func<Task<T?>> dbFallback, TimeSpan absoluteTime, TimeSpan slidingTime, bool storeAfterGet = true);
}
}