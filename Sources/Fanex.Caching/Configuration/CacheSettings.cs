#if NET461

namespace Fanex.Caching.Configuration
{
    using System.Configuration;

    internal class CacheSettings : ConfigurationSection
    {
        private const string CacheTypeAttributeName = "cacheType";
        private const string ConnectionSettingsElementName = "connectionSettings";

        [ConfigurationProperty(CacheTypeAttributeName, IsRequired = false)]
        public string CacheType => base[CacheTypeAttributeName] as string;

        [ConfigurationProperty(ConnectionSettingsElementName, IsDefaultCollection = false)]
        [ConfigurationCollection(
            typeof(ConnectionSettingCollection<ConnectionSetting>),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConnectionSettingCollection<ConnectionSetting> ConnectionSettingCollection
            => base[ConnectionSettingsElementName] as ConnectionSettingCollection<ConnectionSetting>;
    }
}

#endif