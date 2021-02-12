using Grand.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Grand.Core.TypeConverters.Converter
{
    public class ShippingOptionListTypeConverter : TypeConverter
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
                List<ShippingOption> shippingOptions = null;
                var valueStr = value as string;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        shippingOptions = JsonSerializer.Deserialize<List<ShippingOption>>(valueStr);
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return shippingOptions;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOptions = value as List<ShippingOption>;
                if (shippingOptions != null)
                {
                    return JsonSerializer.Serialize(shippingOptions);
                }

                return "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
