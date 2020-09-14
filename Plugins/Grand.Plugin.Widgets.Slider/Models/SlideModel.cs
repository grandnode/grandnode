﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Plugin.Widgets.Slider.Models
{
    public partial class SlideModel : BaseEntityModel, ILocalizedModel<SlideLocalizedModel>, IStoreMappingModel
    {
        public SlideModel()
        {
            Locales = new List<SlideLocalizedModel>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<StoreModel>();
        }
        [GrandResourceDisplayName("Plugins.Widgets.Slider.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]
        public string Link { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.FullWidth")]
        public bool FullWidth { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.SliderType")]
        public int SliderTypeId { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string PictureId { get; set; }

        public IList<SlideLocalizedModel> Locales { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Plugins.Widgets.Slider.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Plugins.Widgets.Slider.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Category")]
        public string CategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Manufacturer")]
        public string ManufacturerId { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }

    }

    public partial class SlideLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Description")]

        public string Description { get; set; }

    }
}
