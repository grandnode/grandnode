using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class InteractiveFormProfile : Profile, IMapperProfile
    {
        public InteractiveFormProfile()
        {
            CreateMap<InteractiveForm, InteractiveFormModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableTokens, mo => mo.Ignore());

            CreateMap<InteractiveFormModel, InteractiveForm>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.FormAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));

            CreateMap<InteractiveForm.FormAttribute, InteractiveFormAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<InteractiveFormAttributeModel, InteractiveForm.FormAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));

            CreateMap<InteractiveForm.FormAttributeValue, InteractiveFormAttributeValueModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<InteractiveFormAttributeValueModel, InteractiveForm.FormAttributeValue>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}