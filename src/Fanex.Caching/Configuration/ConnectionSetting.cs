#if XMLCONFIG

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
            get
            {
                return (string)this[NameAttributeName];
            }

            set
            {
                this[NameAttributeName] = value;
            }
        }

        [ConfigurationProperty(ValueAttributeName, IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this[ValueAttributeName];
            }

            set
            {
                this[ValueAttributeName] = value;
            }
        }
    }
}

#endif