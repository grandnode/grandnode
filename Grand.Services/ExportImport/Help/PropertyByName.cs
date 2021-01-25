using System;

namespace Grand.Services.ExportImport.Help
{
    /// <summary>
    /// A helper class to access the property by name
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class PropertyByName<T>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="func">Feature property access</param>
        public PropertyByName(string propertyName, Func<T, object> func = null)
        {
            PropertyName = propertyName;
            GetProperty = func;

            PropertyOrderPosition = 0;
        }

        /// <summary>
        /// Property order position
        /// </summary>
        public int PropertyOrderPosition { get; set; }

        /// <summary>
        /// Feature property access
        /// </summary>
        public Func<T, object> GetProperty { get; private set; }

        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Property value
        /// </summary>
        public object PropertyValue { get; set; }

        /// <summary>
        /// Converted property value to Int32
        /// </summary>
        public int IntValue {
            get {
                if (PropertyValue == null || !int.TryParse(PropertyValue.ToString(), out var rez))
                    return default;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to bool
        /// </summary>
        public bool BooleanValue {
            get {
                if (PropertyValue == null || !bool.TryParse(PropertyValue.ToString(), out var rez))
                    return default;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to string
        /// </summary>
        public string StringValue {
            get { return PropertyValue == null ? string.Empty : Convert.ToString(PropertyValue); }
        }

        /// <summary>
        /// Converted property value to decimal
        /// </summary>
        public decimal DecimalValue {
            get {
                if (PropertyValue == null || !decimal.TryParse(PropertyValue.ToString(), out var rez))
                    return default;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to decimal?
        /// </summary>
        public decimal? DecimalValueNullable {
            get {
                if (PropertyValue == null || !decimal.TryParse(PropertyValue.ToString(), out var rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to double
        /// </summary>
        public double DoubleValue {
            get {
                if (PropertyValue == null || !double.TryParse(PropertyValue.ToString(), out var rez))
                    return default;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to DateTime?
        /// </summary>
        public DateTime? DateTimeNullable {
            get {
                if (PropertyValue != null && DateTime.TryParse(PropertyValue.ToString(), out DateTime date))
                    return (DateTime?)date;

                return default;
            }

        }

        public override string ToString()
        {
            return PropertyName;
        }
    }
}