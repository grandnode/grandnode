using System;
using System.Linq;
using System.Net;
using Grand.Plugin.Payments.BrainTree.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Payments.BrainTree.Components
{
    [ViewComponent(Name = "PaymentBrainTree")]
    public class PaymentBrainTreeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                var text = (i < 10) ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method != WebRequestMethods.Http.Get)
                return View("~/Plugins/Payments.BrainTree/Views/PaymentInfo.cshtml", model);


            var form = Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];

            var selectedMonth = model.ExpireMonths
                .FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears
                .FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.BrainTree/Views/PaymentInfo.cshtml", model);
        }
    }
}