using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Framework.Mvc.Models
{
    public partial class DatePickerDropDownsModel : BaseGrandEntityModel
    {
        public DatePickerDropDownsModel()
        {
            SelectListDay = new List<SelectListItem>();
            SelectListMonth = new List<SelectListItem>();
            SelectListYear = new List<SelectListItem>();
        }
        public string Attribute { get; set; }

        public string Day { get; set; }

        public IList<SelectListItem> SelectListDay { get; set; }

        public string Month { get; set; }
        public IList<SelectListItem> SelectListMonth { get; set; }

        public string Year { get; set; }
        public IList<SelectListItem> SelectListYear { get; set; }

        public int Begin_Year { get; set; }

        public int End_Year { get; set; }

        public int SelectedDay { get; set; }

        public int SelectedMonth { get; set; }

        public int SelectedYear { get; set; }
    }
}
