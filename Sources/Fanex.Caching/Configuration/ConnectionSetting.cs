#if NET461

namespace Fanex.Caching.Configuration
{
    using System.Configuration;

    internal class ConnectionSetting : ConfigurationElement
    {
        private const string NameAttributeName = "name";
        private const string ValueAttributeName = "value";

        [ConfigurationProperty(NameAttributeName, IsRequired = true, IsKey = true)]
        public string Name
        {
            get => this[NameAttributeName] as string;
            set => this[NameAttributeName] = value;
        }

        [ConfigurationProperty(ValueAttributeName, IsRequired = true)]
        public string Value
        {
            get => this[ValueAttributeName] as string;
            set => this[ValueAttributeName] = value;
        }
    }
}

#endif