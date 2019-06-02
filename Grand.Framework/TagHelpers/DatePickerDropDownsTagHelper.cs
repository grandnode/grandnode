using Grand.Framework.Mvc.Models;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("date-picker-dropdown")]
    public class DatePickerDropDownsTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;
        private readonly ILocalizationService _localizationService;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("Attribute")]
        public string Attribute { get; set; }

        [HtmlAttributeName("control-day")]
        public string ControlId_Day { get; set; }

        [HtmlAttributeName("control-month")]
        public string ControlId_Month { get; set; }

        [HtmlAttributeName("control-year")]
        public string ControlId_Year { get; set; }

        [HtmlAttributeName("begin-year")]
        public int Begin_Year { get; set; }

        [HtmlAttributeName("end-year")]
        public int End_Year { get; set; }

        [HtmlAttributeName("selected-day")]
        public int SelectedDay { get; set; }

        [HtmlAttributeName("selected-month")]
        public int SelectedMonth { get; set; }

        [HtmlAttributeName("selected-year")]
        public int SelectedYear { get; set; }

        public DatePickerDropDownsTagHelper(IHtmlHelper htmlHelper, ILocalizationService localizationService)
        {
            _htmlHelper = htmlHelper;
            _localizationService = localizationService;
        }

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var model = new DatePickerDropDownsModel() {
                Attribute = Attribute,
                Begin_Year = Begin_Year,
                Day = ControlId_Day,
                Month = ControlId_Month,
                Year = ControlId_Year,
                End_Year = End_Year,
                SelectedDay = SelectedDay,
                SelectedMonth = SelectedMonth,
                SelectedYear = SelectedYear
            };

            model.SelectListDay.Add(new SelectListItem() { Value = "0", Text = _localizationService.GetResource("Common.Day") });
            for (var i = 1; i <= 31; i++)
            {
                model.SelectListDay.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString(), Selected = (SelectedDay == i) });
            }

            model.SelectListMonth.Add(new SelectListItem() { Value = "0", Text = _localizationService.GetResource("Common.Month") });
            for (var i = 1; i <= 12; i++)
            {
                model.SelectListMonth.Add(new SelectListItem() { Value = i.ToString(), Text = CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i), Selected = (SelectedMonth == i) });
            }

            model.SelectListYear.Add(new SelectListItem() { Value = "0", Text = _localizationService.GetResource("Common.Year") });

            if (Begin_Year == 0)
                Begin_Year = DateTime.UtcNow.Year - 100;
            if (End_Year == 0)
                End_Year = DateTime.UtcNow.Year;

            if (End_Year > Begin_Year)
            {
                for (var i = Begin_Year; i <= End_Year; i++)
                    model.SelectListYear.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString(), Selected = (SelectedYear == i) });
            }
            else
            {
                for (var i = Begin_Year; i >= End_Year; i--)
                    model.SelectListYear.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString(), Selected = (SelectedYear == i) });
            }

            output.Content.SetHtmlContent((await _htmlHelper.PartialAsync("_DatePickerDropDowns", model)).ToHtmlString());

        }
    }
}
