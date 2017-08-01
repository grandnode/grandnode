using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;

namespace Grand.Services.Common
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class AddressAttributeExtensions
    {
        /// <summary>
        /// A value indicating whether this address attribute should have values
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        /// <returns>Result</returns>
        public static bool ShouldHaveValues(this AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                return false;

            if (addressAttribute.AttributeControlType == AttributeControlType.TextBox ||
                addressAttribute.AttributeControlType == AttributeControlType.MultilineTextbox ||
                addressAttribute.AttributeControlType == AttributeControlType.Datepicker ||
                addressAttribute.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            //other attribute controle types support values
            return true;
        }
    }
}
