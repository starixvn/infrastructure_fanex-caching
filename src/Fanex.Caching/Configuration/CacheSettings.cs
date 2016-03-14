namespace Fanex.Caching.Configuration
{
    using System.Configuration;

    internal class CacheSettings : ConfigurationSection
    {
        private const string CacheTypeAttributeName = "cacheType";
        private const string ConnectionSettingsElementName = "connectionSettings";
        private const string SectionName = "Fanex.Caching";

        [ConfigurationProperty(CacheTypeAttributeName, IsRequired = false)]
        public string CacheType
        {
            get { return (string)base[CacheTypeAttributeName]; }
        }

        [ConfigurationProperty(ConnectionSettingsElementName, IsDefaultCollection = false)]
        [ConfigurationCollection(
            typeof(ConnectionSettingCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConnectionSettingCollection ConnectionSettingCollection
        {
            get
            {
                ConnectionSettingCollection connectionSettingCollection
                    = (ConnectionSettingCollection)base[ConnectionSettingsElementName];

                return connectionSettingCollection;
            }
        }

        public static CacheSettings GetSettings()
        {
            return ConfigurationManager.GetSection(SectionName) as CacheSettings;
        }
    }
}