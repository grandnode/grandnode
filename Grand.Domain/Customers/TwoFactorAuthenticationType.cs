namespace Grand.Domain.Customers
{
    public enum TwoFactorAuthenticationType
    {
        /// <summary>
        /// Google/MS authenticator
        /// </summary>
        AppVerification = 0,
        /// <summary>
        /// Email verification
        /// </summary>
        EmailVerification = 1,
        /// <summary>
        /// SMS verification
        /// </summary>
        SMSVerification = 2,
    }
}
