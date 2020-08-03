using System;

namespace Grand.Domain.Messages
{
    /// <summary>
    /// Represents an email account
    /// </summary>
    public partial class EmailAccount : BaseEntity
    {
        /// <summary>
        /// Gets or sets an email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets an email display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets an email host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets an email port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets an email user name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets an email password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value that the remote certificate is invalid according to the validation procedure.
        /// https://github.com/jstedfast/MailKit/blob/master/FAQ.md#InvalidSslCertificate
        /// </summary>
        public bool UseServerCertificateValidation { get; set; }

        /// <summary>
        /// Provides a way of specifying the SSL and/or TLS encryption that should be used for a connection
        /// </summary>
        public int SecureSocketOptionsId { get; set; }

        /// <summary>
        /// Gets a friendly email account name
        /// </summary>
        public string FriendlyName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(this.DisplayName))
                    return this.Email + " (" + this.DisplayName + ")";
                return this.Email;
            }
        }



    }
}
