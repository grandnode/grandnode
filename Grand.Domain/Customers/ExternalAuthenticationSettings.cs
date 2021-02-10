using Grand.Domain.Configuration;
using System.Collections.Generic;

namespace Grand.Domain.Customers
{
    /// <summary>
    /// External authentication settings
    /// </summary>
    public class ExternalAuthenticationSettings : ISettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalAuthenticationSettings()
        {
            ActiveAuthenticationMethodSystemNames = new List<string>();
        }
               
        /// <summary>
        /// Gets or sets system names of active payment methods
        /// </summary>
        public List<string> ActiveAuthenticationMethodSystemNames { get; set; }
    }
}