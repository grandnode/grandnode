using AutoMapper;
using Grand.Domain.Courses;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Courses;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CourseProfile : Profile, IMapperProfile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableLevels, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.ProductName, mo => mo.Ignore());

            CreateMap<CourseModel, Course>()
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(x => x.SelectedCustomerRoleIds != null ? x.SelectedCustomerRoleIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}