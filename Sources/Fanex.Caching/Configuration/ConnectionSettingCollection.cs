#if NET461

namespace Fanex.Caching.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;

    internal class ConnectionSettingCollection<T> : ConfigurationElementCollection, ICollection<T> where T : ConfigurationElement, new()
    {
        public override ConfigurationElementCollectionType CollectionType
            => ConfigurationElementCollectionType.AddRemoveClearMap;

        bool ICollection<T>.IsReadOnly => IsReadOnly();

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

        public void Add(T item)
            => BaseAdd(item);

        public void Clear()
            => BaseClear();

        public bool Contains(T item)
            => IsElementName((item as ConnectionSetting)?.Name);

        public void CopyTo(T[] array, int arrayIndex)
            => base.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            var key = (item as ConnectionSetting)?.Name;

            BaseRemove(key);

            return BaseIsRemoved(key);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerator baseInterator = GetEnumerator();

            while (baseInterator.MoveNext())
            {
                yield return baseInterator.Current as T;
            }
        }
    }
}

#endif