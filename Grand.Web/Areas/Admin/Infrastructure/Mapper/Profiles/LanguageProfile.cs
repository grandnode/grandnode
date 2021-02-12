using AutoMapper;
using Grand.Core.Mapper;
using Grand.Web.Areas.Admin.Models.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class LanguageProfile : Profile, IAutoMapperProfile
    {
        public LanguageProfile()
        {
            CreateMap<Domain.Localization.Language, LanguageModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.FlagFileNames, mo => mo.Ignore());

            CreateMap<LanguageModel, Domain.Localization.Language>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()));
        }

        public int Order => 0;
    }
}