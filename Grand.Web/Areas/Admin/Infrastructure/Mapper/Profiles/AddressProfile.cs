using AutoMapper;
using Grand.Domain.Common;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class AddressProfile : Profile, IMapperProfile
    {
        public AddressProfile()
        {
            CreateMap<Address, AddressModel>()
               .ForMember(dest => dest.AddressHtml, mo => mo.Ignore())
               .ForMember(dest => dest.CustomAddressAttributes, mo => mo.Ignore())
               .ForMember(dest => dest.FormattedCustomAddressAttributes, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableStates, mo => mo.Ignore())
               .ForMember(dest => dest.FirstNameEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.FirstNameRequired, mo => mo.Ignore())
               .ForMember(dest => dest.LastNameEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.LastNameRequired, mo => mo.Ignore())
               .ForMember(dest => dest.EmailEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.EmailRequired, mo => mo.Ignore())
               .ForMember(dest => dest.CompanyEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CompanyRequired, mo => mo.Ignore())
               .ForMember(dest => dest.CountryEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.StateProvinceEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CityEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CityRequired, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddressEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddressRequired, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddress2Enabled, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddress2Required, mo => mo.Ignore())
               .ForMember(dest => dest.ZipPostalCodeEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.ZipPostalCodeRequired, mo => mo.Ignore())
               .ForMember(dest => dest.PhoneEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.PhoneRequired, mo => mo.Ignore())
               .ForMember(dest => dest.FaxEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.FaxRequired, mo => mo.Ignore());

            //address
            CreateMap<AddressModel, Address>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.CustomAttributes, mo => mo.Ignore());

        }

        public int Order => 0;
    }
}
