namespace Fanex.Caching.Configuration
{
    using System.Collections;
    using System.Configuration;

    internal class ConnectionSettingCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ConnectionSetting)?.Name;
        }

        public Hashtable ToHashtable()
        {
            var connectionSettings = new Hashtable();

            foreach (ConfigurationElement item in this)
            {
                var setting = item as ConnectionSetting;

                if (item != null)
                {
                    connectionSettings.Add(setting.Name, setting.Value);
                }
            }

            return connectionSettings;
        }
    }
}