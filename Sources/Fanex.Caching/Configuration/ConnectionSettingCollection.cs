#if NET461

namespace Fanex.Caching.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;

    internal class ConnectionSettingCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
            => ConfigurationElementCollectionType.AddRemoveClearMap;

        protected override ConfigurationElement CreateNewElement()
            => new ConnectionSetting();

        protected override object GetElementKey(ConfigurationElement element)
            => (element as ConnectionSetting)?.Name;

        public Hashtable ToHashtable()
        {
            var connectionSettings = new Hashtable(StringComparer.OrdinalIgnoreCase);

            foreach (ConfigurationElement item in this)
            {
                if (item is ConnectionSetting setting)
                {
                    connectionSettings.Add(setting.Name, setting.Value);
                }
            }

            return connectionSettings;
        }
    }
}

#endif