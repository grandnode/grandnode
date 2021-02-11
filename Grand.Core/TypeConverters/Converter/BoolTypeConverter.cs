using System;
using System.ComponentModel;
using System.Globalization;

namespace Grand.Core.TypeConverters.Converter
{
    public class BoolTypeConverter : BooleanConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            try
            {
                var result = base.ConvertFrom(context, culture, value);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
