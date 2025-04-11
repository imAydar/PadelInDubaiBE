using Microsoft.Extensions.Caching.Memory;

namespace PadelInDubai.Extensions
{
    public static class CacheExtensions
    {
        public static void Clear(this IMemoryCache cache)
        {
            if (cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }
    }
}
