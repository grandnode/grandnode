using System.Linq;

namespace Grand.Framework.Extensions
{
    public static class FluentValidationUtilities
    {
        public static bool PageSizeOptionsValidator(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            var notValid = value.Split(',').Select(p => p.Trim()).GroupBy(p => p).Any(p => p.Count() > 1);
            return !notValid;
        }
    }
}
