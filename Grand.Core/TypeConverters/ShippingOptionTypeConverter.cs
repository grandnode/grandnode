using Grand.Domain.Shipping;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Grand.Core.TypeConverters
{
    public class ShippingOptionTypeConverter : TypeConverter
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
                ShippingOption shippingOption = null;
                var valueStr = value as string;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        shippingOption = JsonSerializer.Deserialize<ShippingOption>(valueStr);
                    }
                    catch
                    {
                        //deserialize error
                    }
                }
                return shippingOption;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOption = value as ShippingOption;
                if (shippingOption != null)
                {
                    return JsonSerializer.Serialize(shippingOption);
                }

                return "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
