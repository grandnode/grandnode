﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Grand.Core.TypeConverters
{
    /// <summary>
    /// Generic List type converted
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public class GenericListTypeConverter<T> : TypeConverter
    {
        /// <summary>
        /// Type converter
        /// </summary>
        protected readonly TypeConverter typeConverter;

        /// <summary>
        /// Ctor
        /// </summary>
        public GenericListTypeConverter()
        {
            typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter == null)
                throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
        }

        /// <summary>
        /// Get string array from a comma-separate string
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Array</returns>
        protected virtual string[] GetStringArray(string input)
        {
            return string.IsNullOrEmpty(input) ? new string[0] : input.Split(',').Select(x => x.Trim()).ToArray();
        }

        /// <summary>
        /// Gets a value indicating whether this converter can        
        /// convert an object in the given source type to the native type of the converter
        /// using the context.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="sourceType">Source type</param>
        /// <returns>Result</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                var items = GetStringArray(sourceType.ToString());
                return items.Any();
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the converter's native type.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="culture">Culture</param>
        /// <param name="value">Value</param>
        /// <returns>Result</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var items = GetStringArray((string)value);
                var result = new List<T>();
                Array.ForEach(items, s =>
                {
                    var item = typeConverter.ConvertFromInvariantString(s);
                    if (item != null)
                    {
                        result.Add((T)item);
                    }
                });

                return result;
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified destination type using the specified context and arguments
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="culture">Culture</param>
        /// <param name="value">Value</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Result</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var result = string.Empty;
                if (value != null)
                {
                    //we don't use string.Join() because it doesn't support invariant culture
                    for (var i = 0; i < ((IList<T>)value).Count; i++)
                    {
                        var str1 = Convert.ToString(((IList<T>)value)[i], CultureInfo.InvariantCulture);
                        result += str1;
                        //don't add comma after the last element
                        if (i != ((IList<T>)value).Count - 1)
                            result += ",";
                    }
                }
                return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
