using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ContactAttributeProfile : Profile, IMapperProfile
    {
        public ContactAttributeProfile()
        {
            CreateMap<ContactAttribute, ContactAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.ConditionAllowed, mo => mo.Ignore())
                .ForMember(dest => dest.ConditionModel, mo => mo.Ignore());
            CreateMap<ContactAttributeModel, ContactAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.ConditionAttributeXml, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(x => x.SelectedCustomerRoleIds != null ? x.SelectedCustomerRoleIds.ToList() : new List<string>()))
                .ForMember(dest => dest.ContactAttributeValues, mo => mo.Ignore());

            CreateMap<ContactAttributeValue, ContactAttributeValueModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<ContactAttributeValueModel, ContactAttributeValue>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}