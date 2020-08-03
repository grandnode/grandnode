using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class EmailAccountProfile : Profile, IMapperProfile
    {
        public EmailAccountProfile()
        {
            CreateMap<EmailAccount, EmailAccountModel>()
                .ForMember(dest => dest.Password, mo => mo.Ignore())
                .ForMember(dest => dest.IsDefaultEmailAccount, mo => mo.Ignore())
                .ForMember(dest => dest.SendTestEmailTo, mo => mo.Ignore());

            CreateMap<EmailAccountModel, EmailAccount>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Password, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}