using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Customers)]
    public partial class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IProductReviewService _productReviewService;
        private readonly IProductReviewViewModelService _productReviewViewModelService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly ICustomerViewModelService _customerViewModelService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ILocalizationService _localizationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
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
            IProductService productService,
            IProductReviewService productReviewService,
            IProductReviewViewModelService productReviewViewModelService,
            IProductViewModelService productViewModelService,
            ICustomerViewModelService customerViewModelService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ILocalizationService localizationService,
            CustomerSettings customerSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IExportManager exportManager,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IWorkflowMessageService workflowMessageService,
            IDownloadService downloadService)
        {
            _customerService = customerService;
            _productService = productService;
            _productReviewService = productReviewService;
            _productReviewViewModelService = productReviewViewModelService;
            _productViewModelService = productViewModelService;
            _customerViewModelService = customerViewModelService;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _localizationService = localizationService;
            _customerSettings = customerSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _exportManager = exportManager;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _workflowMessageService = workflowMessageService;
            _downloadService = downloadService;
        }

        #endregion

        protected virtual async Task<string> ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributes();
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

        public async Task<IActionResult> List()
        {
            var model = await _customerViewModelService.PrepareCustomerListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> CustomerList(DataSourceRequest command, CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds)
        {
            var (customerModelList, totalCount) = await _customerViewModelService.PrepareCustomerList(model, searchCustomerRoleIds, searchCustomerTagIds, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customerModelList.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new CustomerModel();
            await _customerViewModelService.PrepareCustomerModel(model, null, false);
            //default value
            model.Active = true;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]

        public async Task<IActionResult> Create(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = await _customerService.GetCustomerByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }

            if (!string.IsNullOrWhiteSpace(model.Owner))
            {
                var custowner = await _customerService.GetCustomerByEmail(model.Owner);
                if (custowner == null)
                    ModelState.AddModelError("", "Owner email is not exists");
            }

            if (!string.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = await _customerService.GetCustomerByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate customer roles
            var allCustomerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = _customerViewModelService.ValidateCustomerRoles(newCustomerRoles);
            if (!string.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            if (ModelState.IsValid)
            {
                model.CustomAttributes = await ParseCustomCustomerAttributes(form);
                var customer = await _customerViewModelService.InsertCustomerModel(model);
                
                //password
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = await _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            ErrorNotification(changePassError);
                    }
                }

                if (customer.IsAdmin() && !string.IsNullOrEmpty(model.VendorId))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                }

                if (newCustomerRoles.Any(x=>x.SystemName == SystemCustomerRoleNames.Vendors) && string.IsNullOrEmpty(model.VendorId))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }
                if (newCustomerRoles.Any(x => x.SystemName == SystemCustomerRoleNames.Staff) && string.IsNullOrEmpty(model.StaffStoreId))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInStaffRoleWithoutStaffAssociated"));
                }
                if (customer.IsStaff() && customer.IsVendor())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.VendorShouldNotbeStaff"));
                }

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _customerViewModelService.PrepareCustomerModel(model, null, true);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var customer = await _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerModel();
            await _customerViewModelService.PrepareCustomerModel(model, customer, false);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            //validate customer roles
            var allCustomerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);
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
            if (!string.IsNullOrWhiteSpace(model.Owner))
            {
                var custowner = await _customerService.GetCustomerByEmail(model.Owner);
                if (custowner == null)
                    ModelState.AddModelError("", "Owner email is not exists");

                if(model.Owner.ToLower()==model.Email.ToLower())
                    ModelState.AddModelError("", "You can't assign own email");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    model.CustomAttributes = await ParseCustomCustomerAttributes(form);
                    customer = await _customerViewModelService.UpdateCustomerModel(customer, model);
                    if (customer.IsAdmin() && !string.IsNullOrEmpty(model.VendorId))
                    {
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                    }
                    if (customer.IsVendor() && string.IsNullOrEmpty(model.VendorId))
                    {
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                    }
                    if (customer.IsStaff() && string.IsNullOrEmpty(model.StaffStoreId))
                    {
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInStaffRoleWithoutStaffAssociated"));
                    }
                    if (customer.IsStaff() && customer.IsVendor())
                    {
                        WarningNotification(_localizationService.GetResource("Admin.Customers.Customers.VendorShouldNotbeStaff"));
                    }
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        await SaveSelectedTabIndex();

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
            await _customerViewModelService.PrepareCustomerModel(model, customer, true);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public async Task<IActionResult> ChangePassword(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = await _customerRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public async Task<IActionResult> MarkVatNumberAsValid(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            await _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public async Task<IActionResult> MarkVatNumberAsInvalid(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            await _genericAttributeService.SaveAttribute(customer,
                SystemCustomerAttributeNames.VatNumberStatusId,
                (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public async Task<IActionResult> RemoveAffiliate(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            customer.AffiliateId = "";
            await _customerService.UpdateAffiliate(customer);
            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var customer = await _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");
            if (customer.Id == _workContext.CurrentCustomer.Id)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.NoSelfDelete"));
                return RedirectToAction("List");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _customerViewModelService.DeleteCustomer(customer);
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

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> DeleteSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                await _customerViewModelService.DeleteSelected(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public async Task<IActionResult> Impersonate(string id)
        {
            var customer = await _customerService.GetCustomerById(id);
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

            await _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public async Task<IActionResult> SendWelcomeMessage(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            await _workflowMessageService.SendCustomerWelcomeMessage(customer, _storeContext.CurrentStore, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public async Task<IActionResult> ReSendActivationMessage(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //email validation message
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
            await _workflowMessageService.SendCustomerEmailValidationMessage(customer, _storeContext.CurrentStore, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> SendEmail(CustomerModel model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
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

                await _customerViewModelService.SendEmail(customer, model.SendEmail);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> SendPm(CustomerModel model, [FromServices] ForumSettings forumSettings)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
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

                await _customerViewModelService.SendPM(customer, model.SendPm);

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

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> RewardPointsHistorySelect(string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            var model = (await _customerViewModelService.PrepareRewardPointsHistoryModel(customerId)).ToList();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> RewardPointsHistoryAdd(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            await _customerViewModelService.InsertRewardPointsHistory(customerId, storeId, addRewardPointsValue, addRewardPointsMessage);

            return Json(new { Result = true });
        }

        #endregion

        #region Addresses

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> AddressesSelect(string customerId, DataSourceRequest command)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var addresses = (await _customerViewModelService.PrepareAddressModel(customer)).ToList();
            var gridModel = new DataSourceResult
            {
                Data = addresses,
                Total = addresses.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddressDelete(string id, string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var address = customer.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                //No customer found with the specified id
                return Content("No customer found with the specified id");
            if (ModelState.IsValid)
            {
                await _customerViewModelService.DeleteAddress(customer, address);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddressCreate(string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerAddressModel();
            await _customerViewModelService.PrepareAddressModel(model, null, customer, false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddressCreate(CustomerAddressModel model, IFormCollection form)
        {
            var customer = await _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = await _customerViewModelService.InsertAddressModel(customer, model, customAttributes);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
            }

            //If we got this far, something failed, redisplay form
            await _customerViewModelService.PrepareAddressModel(model, null, customer, true);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddressEdit(string addressId, string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == addressId).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            var model = new CustomerAddressModel();
            await _customerViewModelService.PrepareAddressModel(model, address, customer, false);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]

        public async Task<IActionResult> AddressEdit(CustomerAddressModel model, IFormCollection form)
        {
            var customer = await _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var address = customer.Addresses.Where(x => x.Id == model.Address.Id).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = customer.Id });

            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = await _customerViewModelService.UpdateAddressModel(customer, address, model, customAttributes);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }
            //If we got this far, something failed, redisplay form
            await _customerViewModelService.PrepareAddressModel(model, address, customer, true);

            return View(model);
        }

        #endregion

        #region Orders

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> OrderList(string customerId, DataSourceRequest command)
        {
            var (orderModels, totalCount) = await _customerViewModelService.PrepareOrderModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orderModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Reviews

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ReviewList(string customerId, DataSourceRequest command)
        {
            var productReviews = await _productReviewService.GetAllProductReviews(customerId, null,
                null, null, "", null, "", command.Page - 1, command.PageSize);
            var items = new List<ProductReviewModel>();
            foreach (var x in productReviews)
            {
                var m = new ProductReviewModel();
                await _productViewModelService.PrepareProductReviewModel(m, x, false, true);
                items.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = productReviews.TotalCount,
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ReviewDelete(string id)
        {
            var productReview = await _productReviewService.GetProductReviewById(id);
            if (productReview == null)
                throw new ArgumentException("No review found with the specified id", "id");

            await _productReviewViewModelService.DeleteProductReview(productReview);
            return new NullJsonResult();
        }

        #endregion

        #region Current shopping cart/ wishlist

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> GetCartList(string customerId, int cartTypeId)
        {
            var cart = await _customerViewModelService.PrepareShoppingCartItemModel(customerId, cartTypeId);
            var gridModel = new DataSourceResult
            {
                Data = cart,
                Total = cart.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> DeleteCart(string id, string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            await _customerViewModelService.DeleteCart(customer, id);

            return new NullJsonResult();
        }
        #endregion

        #region Customer Product Personalize / Price

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductsPrice(DataSourceRequest command, string customerId)
        {
            var (productPriceModels, totalCount) = await _customerViewModelService.PrepareProductPriceModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productPriceModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> PersonalizedProducts(DataSourceRequest command, string customerId)
        {
            var (productModels, totalCount) = await _customerViewModelService.PreparePersonalizedProducts(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAddPopup(string customerId)
        {
            var model = await _customerViewModelService.PrepareCustomerModelAddProductModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, CustomerModel.AddProductModel model)
        {
            var products = await _customerViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = products.products.ToList(),
                Total = products.totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(string customerId, bool personalized, CustomerModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _customerViewModelService.InsertCustomerAddProductModel(customerId, personalized, model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> UpdateProductPrice(CustomerModel.ProductPriceModel model)
        {
            await _customerViewModelService.UpdateProductPrice(model);
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> DeleteProductPrice(string id)
        {
            await _customerViewModelService.DeleteProductPrice(id);
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> UpdatePersonalizedProduct(CustomerModel.ProductModel model)
        {
            await _customerViewModelService.UpdatePersonalizedProduct(model);
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> DeletePersonalizedProduct(string id)
        {
            await _customerViewModelService.DeletePersonalizedProduct(id);
            return new NullJsonResult();
        }

        #endregion

        #region Activity log and message contact form

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ListActivityLog(DataSourceRequest command, string customerId)
        {
            var (activityLogModels, totalCount) = await _customerViewModelService.PrepareActivityLogModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLogModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ContactFormList(DataSourceRequest command, string customerId)
        {
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            var (contactFormModels, totalCount) = await _customerViewModelService.PrepareContactFormModel(customerId, vendorId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = contactFormModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }
        #endregion

        #region Back in stock subscriptions

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> BackInStockSubscriptionList(DataSourceRequest command, string customerId)
        {
            var (backInStockSubscriptionModels, totalCount) = await _customerViewModelService.PrepareBackInStockSubscriptionModel(customerId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = backInStockSubscriptionModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Customer note

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> CustomerNotesSelect(string customerId, DataSourceRequest command)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            var customerNoteModels = await _customerViewModelService.PrepareCustomerNoteList(customerId);
            var gridModel = new DataSourceResult
            {
                Data = customerNoteModels,
                Total = customerNoteModels.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> CustomerNoteAdd(string customerId, string downloadId, bool displayToCustomer, string title, string message)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Json(new { Result = false });

            await _customerViewModelService.InsertCustomerNote(customerId, downloadId, displayToCustomer, title, message);

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> CustomerNoteDelete(string id, string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("No customer found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                throw new ArgumentException("AccessDenied");

            await _customerViewModelService.DeleteCustomerNote(id, customerId);

            return new NullJsonResult();
        }


        #endregion

        #region Export / Import

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public async Task<IActionResult> ExportExcelAll(CustomerListModel model)
        {
            var customers = await _customerService.GetAllCustomers(
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

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(await _customerService.GetCustomersByIds(ids));
            }

            byte[] bytes = _exportManager.ExportCustomersToXlsx(customers);
            return File(bytes, "text/xls", "customers.xlsx");
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public async Task<IActionResult> ExportXmlAll(CustomerListModel model)
        {
            var customers = await _customerService.GetAllCustomers(
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
                var xml = await _exportManager.ExportCustomersToXml(customers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                customers.AddRange(await _customerService.GetCustomersByIds(ids));
            }

            var xml = await _exportManager.ExportCustomersToXml(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }

        #endregion
    }
}
