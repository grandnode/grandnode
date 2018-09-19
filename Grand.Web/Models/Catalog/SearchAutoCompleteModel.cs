using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Models.Catalog
{
    public class SearchAutoCompleteModel
    {
        public string SearchType { get; set; }
        public string Label { get; set; }
        public string Url { get; set; }
        public string Desc { get; set; }
        public string PictureUrl { get; set; }
    }
}
