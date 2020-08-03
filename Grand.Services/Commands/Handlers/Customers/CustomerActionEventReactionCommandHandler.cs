using Grand.Core;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Customers;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Customers
{
    public class CustomerActionEventReactionCommandHandler : IRequestHandler<CustomerActionEventReactionCommand, bool>
    {
        private readonly IRepository<Banner> _bannerRepository;
        private readonly IRepository<InteractiveForm> _interactiveFormRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IWorkContext _workContext;
        private readonly IPopupService _popupService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICustomerService _customerService;

        public CustomerActionEventReactionCommandHandler(
            IRepository<Banner> bannerRepository,
            IRepository<InteractiveForm> interactiveFormRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            IWorkContext workContext,
            IPopupService popupService,
            IServiceProvider serviceProvider,
            ICustomerService customerService)
        {
            _bannerRepository = bannerRepository;
            _interactiveFormRepository = interactiveFormRepository;
            _customerActionHistoryRepository = customerActionHistoryRepository;
            _workContext = workContext;
            _popupService = popupService;
            _serviceProvider = serviceProvider;
            _customerService = customerService;
        }

        public async Task<bool> Handle(CustomerActionEventReactionCommand request, CancellationToken cancellationToken)
        {
            await Reaction(request.CustomerActionTypes, request.Action, request.CustomerId, request.CartItem, request.Order);
            return true;
        }
        public async Task Reaction(IList<CustomerActionType> customerActionTypes, CustomerAction action, string customerId, ShoppingCartItem cartItem, Order order)
        {
            if (action.ReactionType == CustomerReactionTypeEnum.Banner)
            {
                var banner = await _bannerRepository.GetByIdAsync(action.BannerId);
                if (banner != null)
                    await PrepareBanner(action, banner, customerId);
            }
            if (action.ReactionType == CustomerReactionTypeEnum.InteractiveForm)
            {
                var interactiveform = await _interactiveFormRepository.GetByIdAsync(action.InteractiveFormId);
                if (interactiveform != null)
                    await PrepareInteractiveForm(action, interactiveform, customerId);
            }

            var customer = await _customerService.GetCustomerById(customerId);

            if (action.ReactionType == CustomerReactionTypeEnum.Email)
            {
                var workflowMessageService = _serviceProvider.GetRequiredService<IWorkflowMessageService>();
                if (action.ActionTypeId == customerActionTypes.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id)
                {
                    if (cartItem != null)
                        await workflowMessageService.SendCustomerActionEvent_AddToCart_Notification(action, cartItem,
                            _workContext.WorkingLanguage.Id, customer);
                }

                if (action.ActionTypeId == customerActionTypes.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id)
                {
                    if (order != null)
                        await workflowMessageService.SendCustomerActionEvent_AddToOrder_Notification(action, order, customer,
                            _workContext.WorkingLanguage.Id);
                }

                if (action.ActionTypeId != customerActionTypes.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id && action.ActionTypeId != customerActionTypes.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id)
                {
                    await workflowMessageService.SendCustomerActionEvent_Notification(action,
                        _workContext.WorkingLanguage.Id, customer);
                }
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerRole)
            {
                await AssignToCustomerRole(action, customer);
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerTag)
            {
                await AssignToCustomerTag(action, customer);
            }

            await SaveActionToCustomer(action.Id, customer.Id);

        }

        protected async Task PrepareBanner(CustomerAction action, Banner banner, string customerId)
        {
            var banneractive = new PopupActive() {
                Body = banner.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id),
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = banner.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                PopupTypeId = (int)PopupType.Banner
            };
            await _popupService.InsertPopupActive(banneractive);
        }

        protected async Task PrepareInteractiveForm(CustomerAction action, InteractiveForm form, string customerId)
        {

            var body = PrepareDataInteractiveForm(form);

            var formactive = new PopupActive() {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = form.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                PopupTypeId = (int)PopupType.InteractiveForm
            };
            await _popupService.InsertPopupActive(formactive);
        }

        protected string PrepareDataInteractiveForm(InteractiveForm form)
        {
            var body = form.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id);
            body += "<input type=\"hidden\" name=\"Id\" value=\"" + form.Id + "\">";
            foreach (var item in form.FormAttributes)
            {
                if (item.AttributeControlType == FormControlType.TextBox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textbox = string.Format("<input type=\"text\"  name=\"{0}\" class=\"{1}\" style=\"{2}\" value=\"{3}\" {4}>", item.SystemName, _class, _style, _value, item.IsRequired ? "required" : "");
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textbox);
                }
                if (item.AttributeControlType == FormControlType.MultilineTextbox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textarea = string.Format("<textarea name=\"{0}\" class=\"{1}\" style=\"{2}\" {3}> {4} </textarea>", item.SystemName, _class, _style, item.IsRequired ? "required" : "", _value);
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textarea);
                }
                if (item.AttributeControlType == FormControlType.Checkboxes)
                {
                    var checkbox = "<div class=\"custom-controls-stacked\">";
                    foreach (var itemcheck in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        checkbox += "<div class=\"custom-control custom-checkbox\">";
                        checkbox += string.Format("<input type=\"checkbox\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\">", _class, _style,
                            itemcheck.IsPreSelected ? "checked" : "", itemcheck.Id, item.SystemName, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        checkbox += string.Format("<label class=\"custom-control-label\" for=\"{0}\">{1}</label>", itemcheck.Id, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        checkbox += "</div>";
                    }
                    checkbox += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), checkbox);
                }

                if (item.AttributeControlType == FormControlType.DropdownList)
                {
                    var dropdown = string.Empty;
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control custom-select", item.Class);

                    dropdown = string.Format("<select name=\"{0}\" class=\"{1}\" style=\"{2}\">", item.SystemName, _class, _style);
                    foreach (var itemdropdown in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        dropdown += string.Format("<option value=\"{0}\" {1}>{2}</option>", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), itemdropdown.IsPreSelected ? "selected" : "", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                    }
                    dropdown += "</select>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), dropdown);
                }
                if (item.AttributeControlType == FormControlType.RadioList)
                {
                    var radio = "<div class=\"custom-controls-stacked\">";
                    foreach (var itemradio in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        radio += "<div class=\"custom-control custom-radio\">";
                        radio += string.Format("<input type=\"radio\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\">", _class, _style,
                            itemradio.IsPreSelected ? "checked" : "", itemradio.Id, item.SystemName, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += string.Format("<label class=\"custom-control-label\" for=\"{0}\">{1}</label>", itemradio.Id, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += "</div>";
                    }
                    radio += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), radio);
                }
            }
            body = body.Replace("%sendbutton%", "<input type=\"submit\" id=\"send-interactive-form\" class=\"btn btn-success interactive-form-button\" value=\"Send\" \" />");
            body = body.Replace("%errormessage%", "<div class=\"message-error\"><div class=\"validation-summary-errors\"><div id=\"errorMessages\"></div></div></div>");

            return body;
        }

        protected async Task AssignToCustomerRole(CustomerAction action, Customer customer)
        {
            if (customer.CustomerRoles.Where(x => x.Id == action.CustomerRoleId).Count() == 0)
            {
                var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                var customerRole = await customerService.GetCustomerRoleById(action.CustomerRoleId);
                if (customerRole != null)
                {
                    customerRole.CustomerId = customer.Id;
                    await customerService.InsertCustomerRoleInCustomer(customerRole);
                }
            }
        }

        protected async Task AssignToCustomerTag(CustomerAction action, Customer customer)
        {
            if (customer.CustomerTags.Where(x => x == action.CustomerTagId).Count() == 0)
            {
                var customerTagService = _serviceProvider.GetRequiredService<ICustomerTagService>();
                await customerTagService.InsertTagToCustomer(action.CustomerTagId, customer.Id);
            }
        }

        protected async Task SaveActionToCustomer(string actionId, string customerId)
        {
            await _customerActionHistoryRepository.InsertAsync(new CustomerActionHistory() { CustomerId = customerId, CustomerActionId = actionId, CreateDateUtc = DateTime.UtcNow });
        }

    }
}
