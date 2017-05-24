namespace Fanex.Caching
{
    using System;
    using Memory;

#if NET461

    using System.Collections;
    using System.Configuration;
    using Configuration;

#endif

    public static class CacheServiceManager
    {
        private static Cache _cacheStorage;

        public static Cache CacheStorage
        {
            get
            {
                if (_cacheStorage == null)
                {
                    _cacheStorage = NewDefaultCacheStorage();
                }

                return _cacheStorage;
            }
        }

        public static Cache NewDefaultCacheStorage()
        {
            Cache cacheStorage = new MemoryCache();

#if NET461
            const string SectionName = "Fanex.Caching";

            if (ConfigurationManager.GetSection(SectionName) is CacheSettings settings)
            {
                Type cacheType = string.IsNullOrEmpty(settings.CacheType) ? typeof(MemoryCache) : Type.GetType(settings.CacheType);
                Hashtable connectionSettings = settings?.ConnectionSettingCollection?.ToHashtable();

                if (connectionSettings != null)
                {
                    cacheStorage = Activator.CreateInstance(cacheType, connectionSettings) as Cache;
                }
                else
                {
                    cacheStorage = Activator.CreateInstance(cacheType) as Cache;
                }
            }
#endif

            return cacheStorage;
        }

        public static void Use(Cache cacheStorage)
            => _cacheStorage = cacheStorage ?? throw new ArgumentNullException(nameof(cacheStorage));
    }
}