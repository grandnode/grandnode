using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Widgets.Slider.Models
{
    public class SlideListModel: BaseGrandModel
    {
        public string Id { get; set; }
        public string PictureUrl { get; set; }
        public string Name { get; set; }
        public string DisplayOrder { get; set; }
        public string Link { get; set; }
        public bool Published { get; set; }
        public string ObjectType { get; set; }

    }
}
