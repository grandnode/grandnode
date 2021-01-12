using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Plugin.Widgets.Slider.Services;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Grand.Plugin.Widgets.Slider
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<SliderPlugin>();
            BsonClassMap.RegisterClassMap<PictureSlider>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(c => c.SliderType);
            });
            builder.AddScoped<ISliderService,SliderService>();
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
