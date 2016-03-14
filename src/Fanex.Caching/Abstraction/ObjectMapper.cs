namespace Fanex.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ObjectMapper
    {
        public static T ConvertTo<T>(
            Hashtable data,
            Dictionary<string, Func<object, object>> propertiesMapper = null) where T : new()
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            T result = new T();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (data.ContainsKey(property.Name))
                {
                    object itemValue = data[property.Name];
                    Type propertyType = property.PropertyType;

                    try
                    {
                        if (propertiesMapper?.ContainsKey(property.Name) == true)
                        {
                            property.SetValue(result, propertiesMapper[property.Name](itemValue));
                        }
                        else
                        {
                            property.SetValue(result, Convert.ChangeType(itemValue, propertyType));
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