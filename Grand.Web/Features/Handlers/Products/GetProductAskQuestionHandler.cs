using Grand.Domain.Customers;
using Grand.Framework.Security.Captcha;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductAskQuestionHandler : IRequestHandler<GetProductAskQuestion, ProductAskQuestionModel>
    {
        private readonly CaptchaSettings _captchaSettings;

        public GetProductAskQuestionHandler(CaptchaSettings captchaSettings)
        {
            _captchaSettings = captchaSettings;
        }

        public async Task<ProductAskQuestionModel> Handle(GetProductAskQuestion request, CancellationToken cancellationToken)
        {
            var model = new ProductAskQuestionModel();
            model.Id = request.Product.Id;
            model.ProductName = request.Product.GetLocalized(x => x.Name, request.Language.Id);
            model.ProductSeName = request.Product.GetSeName(request.Language.Id);
            model.Email = request.Customer.Email;
            model.FullName = request.Customer.GetFullName();
            model.Phone = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
            model.Message = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

            return await Task.FromResult(model);
        }
    }
}
