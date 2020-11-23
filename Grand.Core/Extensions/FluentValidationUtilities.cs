using System;
using System.IO;
using System.Linq;

namespace Grand.Core.Extensions
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

        public static bool IsValidPath(string path)
        {
            bool isValid = true;

            try
            {
                Path.GetFullPath(path);
                
                string root = Path.GetPathRoot(path);
                isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
            }
            catch(Exception ex)
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
