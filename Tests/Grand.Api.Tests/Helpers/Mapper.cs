using AutoMapper;
using Grand.Api.Infrastructure.Mapper;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Tests.Helpers
{
    public static class Mapper
    {
        public static void Run()
        {
            ApiMapperModelConfiguration mapper = new ApiMapperModelConfiguration();
            var config = new MapperConfiguration(cfg => {                
                cfg.AddProfile(mapper.GetType());
            });
            AutoMapperConfiguration.Init(config);
        }
    }
}
