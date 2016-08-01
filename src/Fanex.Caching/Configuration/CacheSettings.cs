#if XMLCONFIG

namespace Fanex.Caching.Configuration
{
    using System.Configuration;

    internal class CacheSettings : ConfigurationSection
    {
        private const string CacheTypeAttributeName = "cacheType";
        private const string ConnectionSettingsElementName = "connectionSettings";

        [ConfigurationProperty(CacheTypeAttributeName, IsRequired = false)]
        public string CacheType => (string)base[CacheTypeAttributeName];

        [ConfigurationProperty(ConnectionSettingsElementName, IsDefaultCollection = false)]
        [ConfigurationCollection(
            typeof(ConnectionSettingCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConnectionSettingCollection ConnectionSettingCollection
            => (ConnectionSettingCollection)base[ConnectionSettingsElementName];
    }
}

#endif