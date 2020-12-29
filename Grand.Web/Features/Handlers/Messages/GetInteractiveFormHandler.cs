using Grand.Core;
using Grand.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Features.Models.Messages;
using Grand.Web.Models.Messages;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Messages
{
    public class GetInteractiveFormHandler : IRequestHandler<GetInteractiveForm, InteractiveFormModel>
    {
        private readonly IInteractiveFormService _interactiveFormService;
        private readonly IWorkContext _workContext;

        public GetInteractiveFormHandler(IInteractiveFormService interactiveFormService, IWorkContext workContext)
        {
            _interactiveFormService = interactiveFormService;
            _workContext = workContext;
        }

        public async Task<InteractiveFormModel> Handle(GetInteractiveForm request, CancellationToken cancellationToken)
        {
            var form = await _interactiveFormService.GetFormBySystemName(request.SystemName);
            if (form == null)
                return await Task.FromResult<InteractiveFormModel>(null);

            var model = new InteractiveFormModel {
                InteractiveForm = form,
                Body = PrepareDataInteractiveForm(form)
            };

            return model;
        }
        protected string PrepareDataInteractiveForm(InteractiveForm form)
        {
            var body = form.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id);
            body += $"<input type='hidden' name='Id' value='{form.Id}'>";
            foreach (var item in form.FormAttributes)
            {
                if (item.AttributeControlType == FormControlType.TextBox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textbox = string.Format("<input type='text'  name='{0}' class='{1}' style='{2}' value='{3}' {4}>", item.SystemName, _class, _style, _value, item.IsRequired ? "required" : "");
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textbox);
                }
                if (item.AttributeControlType == FormControlType.MultilineTextbox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textarea = string.Format("<textarea name='{0}' class='{1}' style='{2}' {3}> {4} </textarea>", item.SystemName, _class, _style, item.IsRequired ? "required" : "", _value);
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textarea);
                }
                if (item.AttributeControlType == FormControlType.Checkboxes)
                {
                    var checkbox = "<div class='custom-controls-stacked'>";
                    foreach (var itemcheck in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        checkbox += "<div class='custom-control custom-checkbox'>";
                        checkbox += string.Format("<input type='checkbox' class='{0}' style='{1}' {2} id='{3}' name='{4}' value='{5}'>", _class, _style,
                            itemcheck.IsPreSelected ? "checked" : "", itemcheck.Id, item.SystemName, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        checkbox += string.Format("<label class='custom-control-label' for='{0}'>{1}</label>", itemcheck.Id, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
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

                    dropdown = string.Format("<select name='{0}' class='{1}' style='{2}'>", item.SystemName, _class, _style);
                    foreach (var itemdropdown in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        dropdown += string.Format("<option value='{0}' {1}>{2}</option>", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), itemdropdown.IsPreSelected ? "selected" : "", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                    }
                    dropdown += "</select>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), dropdown);
                }
                if (item.AttributeControlType == FormControlType.RadioList)
                {
                    var radio = "<div class='custom-controls-stacked'>";
                    foreach (var itemradio in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        radio += "<div class='custom-control custom-radio'>";
                        radio += string.Format("<input type='radio' class='{0}' style='{1}' {2} id='{3}' name='{4}' value='{5}'>", _class, _style,
                            itemradio.IsPreSelected ? "checked" : "", itemradio.Id, item.SystemName, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += string.Format("<label class='custom-control-label' for='{0}'>{1}</label>", itemradio.Id, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += "</div>";
                    }
                    radio += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), radio);
                }
            }
            body = body.Replace("%sendbutton%", "<input type='submit' id='send-interactive-form' class='btn btn-success interactive-form-button' value='Send' />");
            body = body.Replace("%errormessage%", "<div class='message-error'><div class='validation-summary-errors'><div id='errorMessages'></div></div></div>");

            return body;
        }
    }
}
