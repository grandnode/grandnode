using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Customers)]
    public partial class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomerViewModelService _customerViewModelService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerReportService _customerReportService;
        private readonly ILocalizationService _localizationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IWorkContext _workContext;
        private readonly IExportManager _exportManager;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDownloadService _downloadService;

        #endregion

        #region Constructors

        public CustomerController(ICustomerService customerService,
            ICustomerViewModelService customerViewModelService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerReportService customerReportService,
            ILocalizationService localizationService,
            CustomerSettings customerSettings,
            IWorkContext workContext,
            IExportManager exportManager,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IWorkflowMessageService workflowMessageService,
            IDownloadService downloadService)
        {
            this._customerService = customerService;
            this._customerViewModelService = customerViewModelService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerReportService = customerReportService;
            this._localizationService = localizationService;
            this._customerSettings = customerSettings;
            this._workContext = workContext;
            this._exportManager = exportManager;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._workflowMessageService = workflowMessageService;
            this._downloadService = downloadService;
        }

        #endregion

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                string controlId = string.Format("customer_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!String.IsNullOrEmpty(item))
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.CustomerAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #region Customers

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _customerViewModelService.PrepareCustomerListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult CustomerList(DataSourceRequest command, CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds)
        {
            var customers = _customerViewModelService.PrepareCustomerList(model, searchCustomerRoleIds, searchCustomerTagIds, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.customerModelList.ToList(),
                Total = customers.totalCount
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = new CustomerModel();
            _customerViewModelService.PrepareCustomerModel(model, null, false);
            //default value
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]

        public IActionResult Create(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = _customerService.GetCustomerByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }
            if (!String.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = _customerService.GetCustomerByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = _customerViewModelService.ValidateCustomerRoles(newCustomerRoles);
            if (!String.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            //password
            if (!String.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        ErrorNotification(changePassError);
                }
            }
            if (ModelState.IsValid)
            {
                model.CustomAttributes = ParseCustomCustomerAttributes(form);
                var customer = _customerViewModelService.InsertCustomerModel(model);
                if (customer.IsAdmin() && !String.IsNullOrEmpty(model.VendorId))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                }
                if (customer.IsVendor() && !String.IsNullOrEmpty(model.VendorId))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _customerViewModelService.PrepareCustomerModel(model, null, true);
            return View(model);

        }

        public IActionResult Edit(string id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerModel();
            _customerViewModelService.PrepareCustomerModel(model, customer, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = _customerViewModelService.ValidateCustomerRoles(newCustomerRoles);
            if (!String.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.CustomAttributes = ParseCustomCustomerAttributes(form);
                    customer = _customerViewModelService.UpdateCustomerModel(customer, model);
                    if (customer.IsAdmin() && !String.IsNullOrEmpty(model.VendorId))
                    {
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                    }
                    if (customer.IsVendor() && String.IsNullOrEmpty(model.VendorId))
                    {
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                    }

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabIndex();

                        return RedirectToAction("Edit", new { id = customer.Id });
                    }
                    return RedirectToAction("List");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }
            //If we got this far, something failed, redisplay form
            _customerViewModelService.PrepareCustomerModel(model, customer, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public IActionResult ChangePassword(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public IActionResult MarkVatNumberAsValid(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public IActionResult MarkVatNumberAsInvalid(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public IActionResult RemoveAffiliate(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            customer.AffiliateId = "";
            _customerService.UpdateAffiliate(customer);
            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    _customerViewModelService.DeleteCustomer(customer);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Deleted"));
                    return RedirectToAction("List");
                }
                ErrorNotification(ModelState);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public IActionResult Impersonate(string id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                ErrorNotification("A non-admin user cannot impersonate as an administrator");
                return RedirectToAction("Edit", customer.Id);
            }

            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public IActionResult SendWelcomeMessage(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public IActionResult ReSendActivationMessage(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //email validation message
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
            _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public IActionResult SendEmail(CustomerModel model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (String.IsNullOrWhiteSpace(customer.Email))
                    throw new GrandException("Customer email is empty");
                if (!CommonHelper.IsValidEmail(customer.Email))
                    throw new GrandException("Customer email is not valid");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new GrandException("Email subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new GrandException("Email body is empty");

                _customerViewModelService.SendEmail(customer, model.SendEmail);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public IActionResult SendPm(CustomerModel model, [FromServices] ForumSettings forumSettings)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (!forumSettings.AllowPrivateMessages)
                    throw new GrandException("Private messages are disabled");
                if (customer.IsGuest())
                    throw new GrandException("Customer should be registered");
                if (String.IsNullOrWhiteSpace(model.SendPm.Subject))
                    throw new GrandException("PM subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendPm.Message))
                    throw new GrandException("PM message is empty");

                _customerViewModelService.SendPM(customer, model.SendPm);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendPM.Sent"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        #endregion

        #region Reward points history

        [HttpPost]
        public IActionResult RewardPointsHistorySelect(string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            var model = _customerViewModelService.PrepareRewardPointsHistoryModel(customerId).ToList();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }


        public IActionResult RewardPointsHistoryAdd(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            _customerViewModelService.InsertRewardPointsHistory(customerId, storeId, addRewardPointsValue, addRewardPointsMessage);

            return Json(new { Result = true });
        }

        #endregion

        #region Addresses

        [HttpPost]
        public IActionResult AddressesSelect(string customerId, DataSourceRequest command)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var addresses = _customerViewModelService.PrepareAddressModel(customer).ToList();
            var gridModel = new DataSourceResult
            {
                Data = addresses,
                Total = addresses.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AddressDelete(string id, string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var address = customer.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                //No customer found with the specified id
                return Content("No customer found with the specified id");
            if (ModelState.IsValid)
            {
                _customerViewModelService.DeleteAddress(customer, address);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult AddressCreate(string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerAddressModel();
            _customerViewModelService.PrepareAddressModel(model, null, customer, false);

            return View(model);
        }

        [HttpPost]

        public IActionResult AddressCreate(CustomerAddressModel model, IFormCollection form)
        {
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = _customerViewModelService.InsertAddressModel(customer, model, customAttributes);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            _customerViewModelService.PrepareAddressModel(model, null, customer, true);
            return View(model);
        }

        public IActionResult AddressEdit(string addressId, string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == addressId).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            var model = new CustomerAddressModel();
            _customerViewModelService.PrepareAddressModel(model, address, customer, false);
            return View(model);
        }

        [HttpPost]

        public IActionResult AddressEdit(CustomerAddressModel model, IFormCollection form)
        {
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == model.Address.Id).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = _customerViewModelService.UpdateAddressModel(customer, address, model, customAttributes);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }
            //If we got this far, something failed, redisplay form
            _customerViewModelService.PrepareAddressModel(model, address, customer, true);

            return View(model);
        }

        #endregion

        #region Orders

        [HttpPost]
        public IActionResult OrderList(string customerId, DataSourceRequest command)
        {
            var orders = _customerViewModelService.PrepareOrderModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.orderModels.ToList(),
                Total = orders.totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Reports

        public IActionResult Reports()
        {
            var model = _customerViewModelService.PrepareCustomerReportsModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ReportBestCustomersByOrderTotalList(DataSourceRequest command, BestCustomersReportModel model)
        {
            var items = _customerViewModelService.PrepareBestCustomerReportLineModel(model, 1, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = items.bestCustomerReportLineModels.ToList(),
                Total = items.totalCount
            };
            return Json(gridModel);
        }
        [HttpPost]
        public IActionResult ReportBestCustomersByNumberOfOrdersList(DataSourceRequest command, BestCustomersReportModel model)
        {
            var items = _customerViewModelService.PrepareBestCustomerReportLineModel(model, 2, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = items.bestCustomerReportLineModels.ToList(),
                Total = items.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportRegisteredCustomersList(DataSourceRequest command)
        {
            var model = _customerViewModelService.GetReportRegisteredCustomersModel();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportCustomerTimeChart(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            var model = _customerReportService.GetCustomerByTimeReport(startDate, endDate);
            var gridModel = new DataSourceResult
            {
                Data = model
            };
            return Json(gridModel);
        }

        #endregion

        #region Current shopping cart/ wishlist

        [HttpPost]
        public IActionResult GetCartList(string customerId, int cartTypeId)
        {
            var cart = _customerViewModelService.PrepareShoppingCartItemModel(customerId, cartTypeId);
            var gridModel = new DataSourceResult
            {
                Data = cart,
                Total = cart.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult DeleteCart(string id, string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            _customerViewModelService.DeleteCart(customer, id);

            return new NullJsonResult();
        }
        #endregion

        #region Customer Product Personalize / Price

        [HttpPost]
        public IActionResult ProductsPrice(DataSourceRequest command, string customerId)
        {
            var productPrices = _customerViewModelService.PrepareProductPriceModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productPrices.productPriceModels.ToList(),
                Total = productPrices.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PersonalizedProducts(DataSourceRequest command, string customerId)
        {
            var products = _customerViewModelService.PreparePersonalizedProducts(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = products.productModels.ToList(),
                Total = products.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult ProductAddPopup(string customerId)
        {
            var model = _customerViewModelService.PrepareCustomerModelAddProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerModel.AddProductModel model)
        {
            var products = _customerViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult();
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(string customerId, bool personalized, CustomerModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _customerViewModelService.InsertCustomerAddProductModel(customerId, personalized, model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }
        public IActionResult UpdateProductPrice(CustomerModel.ProductPriceModel model)
        {
            _customerViewModelService.UpdateProductPrice(model);
            return new NullJsonResult();
        }
        public IActionResult DeleteProductPrice(string id)
        {
            _customerViewModelService.DeleteProductPrice(id);
            return new NullJsonResult();
        }

        public IActionResult UpdatePersonalizedProduct(CustomerModel.ProductModel model)
        {
            _customerViewModelService.UpdatePersonalizedProduct(model);
            return new NullJsonResult();
        }

        public IActionResult DeletePersonalizedProduct(string id)
        {
            _customerViewModelService.DeletePersonalizedProduct(id);

            return new NullJsonResult();
        }

        #endregion

        #region Activity log and message contact form

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string customerId)
        {
            var activityLog = _customerViewModelService.PrepareActivityLogModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModels.ToList(),
                Total = activityLog.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ContactFormList(DataSourceRequest command, string customerId)
        {
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            var contactform = _customerViewModelService.PrepareContactFormModel(customerId, vendorId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = contactform.contactFormModels.ToList(),
                Total = contactform.totalCount
            };
            return Json(gridModel);
        }
        #endregion

        #region Back in stock subscriptions

        [HttpPost]
        public IActionResult BackInStockSubscriptionList(DataSourceRequest command, string customerId)
        {
            var subscriptions = _customerViewModelService.PrepareBackInStockSubscriptionModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = subscriptions.backInStockSubscriptionModels.ToList(),
                Total = subscriptions.totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Customer note

        [HttpPost]
        public IActionResult CustomerNotesSelect(string customerId, DataSourceRequest command)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            var customerNoteModels = _customerViewModelService.PrepareCustomerNoteList(customerId);
            var gridModel = new DataSourceResult
            {
                Data = customerNoteModels,
                Total = customerNoteModels.Count
            };

            return Json(gridModel);
        }


        public IActionResult CustomerNoteAdd(string customerId, string downloadId, bool displayToCustomer, string title, string message)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Json(new { Result = false });

            _customerViewModelService.InsertCustomerNote(customerId, downloadId, displayToCustomer, title, message);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult CustomerNoteDelete(string id, string customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                throw new ArgumentException("AccessDenied");

            _customerViewModelService.DeleteCustomerNote(id, customerId);

            return new NullJsonResult();
        }


        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public IActionResult ExportExcelAll(CustomerListModel model)
        {
            var customers = _customerService.GetAllCustomers(
                customerRoleIds: model.SearchCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                byte[] bytes = _exportManager.ExportCustomersToXlsx(customers);
                return File(bytes, "text/xls", "customers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportExcelSelected(string selectedIds)
        {
            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            byte[] bytes = _exportManager.ExportCustomersToXlsx(customers);
            return File(bytes, "text/xls", "customers.xlsx");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public IActionResult ExportXmlAll(CustomerListModel model)
        {
            var customers = _customerService.GetAllCustomers(
                customerRoleIds: model.SearchCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                var xml = _exportManager.ExportCustomersToXml(customers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportXmlSelected(string selectedIds)
        {
            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            var xml = _exportManager.ExportCustomersToXml(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }

        #endregion
    }
}
