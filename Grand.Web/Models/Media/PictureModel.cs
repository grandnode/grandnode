﻿using Grand.Core.Models;

namespace Grand.Web.Models.Media
{
    public partial class PictureModel : BaseEntityModel
    {
        public string ImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }
    }
}