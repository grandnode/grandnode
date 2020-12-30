using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Grand.Core.TypeConverters
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
