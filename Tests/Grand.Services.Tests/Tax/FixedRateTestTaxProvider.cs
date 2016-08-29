﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Grand.Core.Plugins;
using Grand.Services.Tax;

namespace Grand.Services.Tests.Tax {
    public class FixedRateTestTaxProvider : BasePlugin, ITaxProvider {
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest) {
            var result = new CalculateTaxResult {
                TaxRate = GetTaxRate(calculateTaxRequest.TaxCategoryId)
            };
            return result;
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxCategoryId">The tax category identifier</param>
        /// <returns>Tax rate</returns>
        protected decimal GetTaxRate(string taxCategoryId) {
            decimal rate = 10;
            return rate;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues) {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }
    }
}
