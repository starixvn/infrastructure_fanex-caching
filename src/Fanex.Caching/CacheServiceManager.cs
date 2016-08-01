namespace Fanex.Caching
{
    using System;

#if XMLCONFIG

    using System.Collections;
    using System.Configuration;
    using Configuration;
    using Memory;

#endif

    public static class CacheServiceManager
    {
        private const string SectionName = "Fanex.Caching";
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

#if XMLCONFIG

            var settings = ConfigurationManager.GetSection(SectionName) as CacheSettings;

            if (settings != null)
            {
                Type cacheType = string.IsNullOrEmpty(settings.CacheType)
                    ? typeof(MemoryCache)
                    : Type.GetType(settings.CacheType);
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
        {
            if (cacheStorage == null)
            {
                throw new ArgumentNullException(nameof(cacheStorage));
            }

            _cacheStorage = cacheStorage;
        }
    }
}