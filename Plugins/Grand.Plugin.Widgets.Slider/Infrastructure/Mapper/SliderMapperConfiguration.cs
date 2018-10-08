using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Plugin.Widgets.Slider.Models;

namespace Grand.Plugin.Widgets.Slider.Infrastructure.Mapper
{
    public class SliderMapperConfiguration : Profile, IMapperProfile
    {
        protected string SetObjectEntry(SlideModel model)
        {
            if(model.SliderTypeId == (int)SliderType.HomePage)
                return "";

            if (model.SliderTypeId == (int)SliderType.Category)
                return model.CategoryId;

            if (model.SliderTypeId == (int)SliderType.Manufacturer)
                return model.ManufacturerId;

            return "";
        }
        protected string GetCategoryId(PictureSlider pictureSlider)
        {
            if (pictureSlider.SliderType == SliderType.Category)
                return pictureSlider.ObjectEntry;

            return "";
        }

        protected string GetManufacturerId(PictureSlider pictureSlider)
        {
            if (pictureSlider.SliderType == SliderType.Manufacturer)
                return pictureSlider.ObjectEntry;

            return "";
        }

        public SliderMapperConfiguration()
        {
            CreateMap<SlideModel, PictureSlider>()
                .ForMember(dest => dest.ObjectEntry, mo => mo.ResolveUsing(x=>SetObjectEntry(x)))
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<PictureSlider, SlideModel>()
                .ForMember(dest => dest.CategoryId, mo => mo.ResolveUsing(x => GetCategoryId(x)))
                .ForMember(dest => dest.ManufacturerId, mo => mo.ResolveUsing(x => GetManufacturerId(x)))
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<SlideListModel, PictureSlider>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.Ignore());

            CreateMap<PictureSlider, SlideListModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                .ForMember(dest => dest.PictureUrl, mo => mo.Ignore())
                .ForMember(dest => dest.ObjectType, mo => mo.ResolveUsing(y=>y.SliderType.ToString()));
        }
        public int Order
        {
            get { return 0; }
        }
    }
}
