using Grand.Core;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Commands.Models.Customers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Grand.Web.Commands.Handler.Customers
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, bool>
    {
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly CustomerSettings _customerSettings;

        public UploadAvatarCommandHandler(
            IPictureService pictureService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            CustomerSettings customerSettings)
        {
            _pictureService = pictureService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _customerSettings = customerSettings;
        }

        public async Task<bool> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var customerAvatar = await _pictureService.GetPictureById(request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId));
            if ((request.UploadedFile != null) && (!string.IsNullOrEmpty(request.UploadedFile.FileName)))
            {
                int avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                if (request.UploadedFile.Length > avatarMaxSize)
                    throw new GrandException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                byte[] customerPictureBinary = request.UploadedFile.GetPictureBits();
                if (customerAvatar != null)
                    customerAvatar = await _pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, request.UploadedFile.ContentType, null);
                else
                    customerAvatar = await _pictureService.InsertPicture(customerPictureBinary, request.UploadedFile.ContentType, null);

                if (customerAvatar != null)
                    await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatar.Id);
            }

            if (request.Remove)
                await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.AvatarPictureId, "");

            return true;
        }
    }
}
