using System;
using System.Collections.Generic;
using System.Text;
using static Grand.Services.Authentication.TwoFactorAuthenticationService;

namespace Grand.Services.Authentication
{
    public interface ITwoFactorAuthenticationService
    {
        bool AuthenticateTwoFactor(string userUniqueKey, string token);

        QrCodeSetup GenerateQrCodeSetup(string userUniqueKey);
    }
}
