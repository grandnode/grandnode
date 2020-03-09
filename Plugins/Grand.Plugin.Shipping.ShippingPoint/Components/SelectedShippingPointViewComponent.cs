using Grand.Core;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Shipping.ShippingPoint.Components
{
    [ViewComponent(Name = "ShippingPoint")]
    public class SelectedShippingPointViewComponent : ViewComponent
    {
        private readonly ILocalizationService _localizationService;
        private readonly IShippingPointService _shippingPointService;
        private readonly IStoreContext _storeContext;

        public SelectedShippingPointViewComponent(ILocalizationService localizationService,
            IShippingPointService shippingPointService, IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _shippingPointService = shippingPointService;
            _storeContext = storeContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(string shippingOption)
        {
            var parameter = shippingOption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (parameter == _localizationService.GetResource("Plugins.Shipping.ShippingPoint.PluginName"))
            {
                var shippingPoints = await _shippingPointService.GetAllStoreShippingPoint(_storeContext.CurrentStore.Id);

                var shippingPointsModel = new List<SelectListItem>();
                shippingPointsModel.Add(new SelectListItem() { Value = "", Text = _localizationService.GetResource("Plugins.Shipping.ShippingPoint.SelectShippingOption") });

                foreach (var shippingPoint in shippingPoints)
                {
                    shippingPointsModel.Add(new SelectListItem() { Value = shippingPoint.Id, Text = shippingPoint.ShippingPointName });
                }

                return View("~/Plugins/Shipping.ShippingPoint/Views/FormComboBox.cshtml", shippingPointsModel);
            }
            return Content("ShippingPointController: given Shipping Option doesn't exist");

        }
    }
}
