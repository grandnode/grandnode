using System;

namespace Grand.Framework.Components
{
    public class BaseViewComponentAttribute : Attribute
    {
        public bool AdminAccess { get; set; }
    }
}
