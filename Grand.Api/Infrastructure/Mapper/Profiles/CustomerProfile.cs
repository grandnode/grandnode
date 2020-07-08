using AutoMapper;
using Grand.Api.DTOs.Customers;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Common;
using System;
using System.Linq;

namespace Grand.Api.Infrastructure.Mapper
{
    public class CustomerProfile : Profile, IMapperProfile
    {
        public CustomerProfile()
        {
            CreateMap<CustomerDto, Customer>()
                .ForMember(dest => dest.Addresses, mo => mo.Ignore())
                .ForMember(dest => dest.CannotLoginUntilDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.BillingAddress, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerTags, mo => mo.Ignore())
                .ForMember(dest => dest.FailedLoginAttempts, mo => mo.Ignore())
                .ForMember(dest => dest.HasContributions, mo => mo.Ignore())
                .ForMember(dest => dest.IsSystemAccount, mo => mo.Ignore())
                .ForMember(dest => dest.LastActivityDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LastIpAddress, mo => mo.Ignore())
                .ForMember(dest => dest.LastLoginDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LastPurchaseDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LastUpdateCartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LastUpdateWishListDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Password, mo => mo.Ignore())
                .ForMember(dest => dest.PasswordChangeDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.PasswordFormatId, mo => mo.Ignore())
                .ForMember(dest => dest.PasswordSalt, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingAddress, mo => mo.Ignore())
                .ForMember(dest => dest.ShoppingCartItems, mo => mo.Ignore())
                .ForMember(dest => dest.SystemName, mo => mo.Ignore())
                .ForMember(dest => dest.UrlReferrer, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.FirstName, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName, "")))
                .ForMember(dest => dest.LastName, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName, "")))
                .ForMember(dest => dest.City, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City, "")))
                .ForMember(dest => dest.Company, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company, "")))
                .ForMember(dest => dest.DateOfBirth, mo => mo.MapFrom(src => src.GetAttributeFromEntity<DateTime?>(SystemCustomerAttributeNames.DateOfBirth, "")))
                .ForMember(dest => dest.Fax, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax, "")))
                .ForMember(dest => dest.Gender, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender, "")))
                .ForMember(dest => dest.Phone, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone, "")))
                .ForMember(dest => dest.Signature, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature, "")))
                .ForMember(dest => dest.StateProvinceId, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId, "")))
                .ForMember(dest => dest.StreetAddress, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress, "")))
                .ForMember(dest => dest.StreetAddress2, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2, "")))
                .ForMember(dest => dest.VatNumber, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber, "")))
                .ForMember(dest => dest.VatNumberStatusId, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumberStatusId, "")))
                .ForMember(dest => dest.ZipPostalCode, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode, "")))
                .ForMember(dest => dest.CountryId, mo => mo.MapFrom(src => src.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId, "")))
                .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(src => src.CustomerRoles.Select(x => x.Id)));

        }

        public int Order => 1;
    }
}
