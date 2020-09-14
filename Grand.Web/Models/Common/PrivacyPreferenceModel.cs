﻿using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public class PrivacyPreferenceModel : BaseModel
    {
        public string SystemName { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string Description { get; set; }
        public bool AllowToDisable { get; set; }
        public bool State { get; set; }
    }
}
