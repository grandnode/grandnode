using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Web.Areas.Admin.Extensions;

namespace Grand.Plugin.Widgets.Slider
{
    public static class MyExtensions
    {
        
        public static SlideModel ToModel(this PictureSlider entity)
        {
            return entity.MapTo<PictureSlider, SlideModel>();
        }

        public static PictureSlider ToEntity(this SlideModel model)
        {
            return model.MapTo<SlideModel, PictureSlider>();
        }
       

        public static SlideListModel ToListModel(this PictureSlider entity)
        {
            return entity.MapTo<PictureSlider, SlideListModel>();
        }

    }


}