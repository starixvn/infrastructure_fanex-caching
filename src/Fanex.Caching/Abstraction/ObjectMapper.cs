namespace Fanex.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ObjectMapper
    {
        public static T ConvertTo<T>(
            Hashtable connectionSettings,
            Dictionary<string, Func<object, object>> propertiesMapper = null) where T : new()
        {
            if (connectionSettings == null)
            {
                throw new ArgumentNullException(nameof(connectionSettings));
            }

            T result = new T();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (connectionSettings.ContainsKey(property.Name))
                {
                    object connectionSettingValue = connectionSettings[property.Name];
                    Type propertyType = property.PropertyType;

                    try
                    {
                        if (propertiesMapper?.ContainsKey(property.Name) == true)
                        {
                            property.SetValue(result, propertiesMapper[property.Name](connectionSettingValue));
                        }
                        else
                        {
                            property.SetValue(result, Convert.ChangeType(connectionSettingValue, propertyType));
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException("Invalid connection settings.");
                    }
                }
            }

            return result;
        }
    }
}