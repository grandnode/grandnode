using Grand.Domain.Tax;
using Grand.Services.Tax;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.Misc.EuropaCheckVat
{
    public class EuropaVatService : VatService, IVatService
    {
        public EuropaVatService(TaxSettings taxSettings) :
            base(taxSettings)
        {
        }

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        public override async Task<(VatNumberStatus status, string name, string address, Exception exception)>
            DoVatCheck(string twoLetterIsoCode, string vatNumber)
        {
            var name = string.Empty;
            var address = string.Empty;

            if (vatNumber == null)
                vatNumber = string.Empty;
            vatNumber = vatNumber.Trim().Replace(" ", "");

            if (twoLetterIsoCode == null)
                twoLetterIsoCode = string.Empty;
            if (!String.IsNullOrEmpty(twoLetterIsoCode))
                //The service returns INVALID_INPUT for country codes that are not uppercase.
                twoLetterIsoCode = twoLetterIsoCode.ToUpper();

            EuropaCheckVatService.checkVatPortTypeClient s = null;

            try
            {
                s = new EuropaCheckVatService.checkVatPortTypeClient();
                var result = await s.checkVatAsync(new EuropaCheckVatService.checkVatRequest {
                    vatNumber = vatNumber,
                    countryCode = twoLetterIsoCode
                });

                var valid = result.valid;
                name = result.name;
                address = result.address;

                return (valid ? VatNumberStatus.Valid : VatNumberStatus.Invalid, name, address, null);
            }
            catch (Exception ex)
            {
                return (VatNumberStatus.Unknown, string.Empty, string.Empty, ex);
            }
            finally
            {
                if (name == null)
                    name = string.Empty;

                if (address == null)
                    address = string.Empty;
            }

        }
    }
}
