using Grand.Domain.Configuration;

namespace Grand.Domain.Messages
{
    public class EmailAccountSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a store default email account identifier
        /// </summary>
        public string DefaultEmailAccountId { get; set; }

    }

}
