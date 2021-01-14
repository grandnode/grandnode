using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class UserApiProfile : Profile, IMapperProfile
    {
        public UserApiProfile()
        {
            CreateMap<UserApi, UserApiModel>()
                .ForMember(dest => dest.Password, mo => mo.Ignore());
            CreateMap<UserApiModel, UserApi>()
                .ForMember(dest => dest.Password, mo => mo.Ignore())
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}