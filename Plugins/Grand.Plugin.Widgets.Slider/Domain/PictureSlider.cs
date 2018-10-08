using Grand.Core;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Plugin.Widgets.Slider.Domain
{
    public partial class PictureSlider: BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public PictureSlider()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
        }
        public string PictureId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public int SliderTypeId { get; set; }
        public string ObjectEntry { get; set; }
        public SliderType SliderType
        {
            get
            {
                return (SliderType)this.SliderTypeId;
            }
            set
            {
                this.SliderTypeId = (int)value;
            }

        }
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }
        public IList<LocalizedProperty> Locales { get; set; }
    }
}
