﻿using AutoMapper;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class RewardPointsSettingsProfile : Profile, IMapperProfile
    {
        public RewardPointsSettingsProfile()
        {
            CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ExchangeRate_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinimumRewardPointsToUse_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForRegistration_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_Awarded_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_Canceled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayHowMuchWillBeEarned_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();
        }

        public int Order => 0;
    }
}