using Grand.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Grand.Core.TypeConverters
{
    public class CustomAttributeListTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                List<CustomAttribute> customAttributes = null;
                var valueStr = value as string;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        customAttributes = JsonSerializer.Deserialize<List<CustomAttribute>>(valueStr);
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return customAttributes;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var customAttributes = value as List<CustomAttribute>;
                if (customAttributes != null)
                {
                    return JsonSerializer.Serialize(customAttributes);
                }

                return "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
