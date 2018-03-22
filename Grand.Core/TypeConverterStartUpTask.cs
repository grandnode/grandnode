using Grand.Core.ComponentModel;
using Grand.Core.Domain.Shipping;
using Grand.Core.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grand.Core
{
    /// <summary>
    /// Startup task for the registration custom type converters
    /// </summary>
    public class TypeConverterRegistrationStartUpTask : IStartupTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            //lists
            TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
            TypeDescriptor.AddAttributes(typeof(List<decimal>), new TypeConverterAttribute(typeof(GenericListTypeConverter<decimal>)));
            TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));

            //dictionaries
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, int>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<int, int>)));

            //shipping option
            TypeDescriptor.AddAttributes(typeof(ShippingOption), new TypeConverterAttribute(typeof(ShippingOptionTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(List<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(IList<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));

        }

        /// <summary>
        /// Gets order of this startup task implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
