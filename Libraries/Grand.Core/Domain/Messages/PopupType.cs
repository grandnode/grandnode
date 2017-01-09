using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Messages
{
    public enum PopupType
    {
        /// <summary>
        /// Banner
        /// </summary>
        Banner = 10,
        /// <summary>
        /// Interactive form
        /// </summary>
        InteractiveForm = 20,
        /// <summary>
        /// Other
        /// </summary>
        Other = 99,
    }
}
