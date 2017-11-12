using Microsoft.AspNetCore.Mvc;
using System;
using Grand.Web.Models.Profile;
using Grand.Services.Forums;
using Grand.Services.Customers;
using Grand.Services.Logging;
using Grand.Services.Localization;
using Grand.Core;
using Grand.Services.Helpers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Media;
using Grand.Core.Domain.Media;
using Grand.Services.Directory;

namespace Grand.Web.ViewComponents
{
    public class ProfileInfoViewComponent : ViewComponent
    {
        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICountryService _countryService;

        public ProfileInfoViewComponent(IForumService forumService,
            ICustomerService customerService, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, IWorkContext workContext,
            IStoreContext storeContext, IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings, CustomerSettings customerSettings,
            IPictureService pictureService, MediaSettings mediaSettings,
            ICountryService countryService)
        {
            this._forumService = forumService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
            this._countryService = countryService;
        }

        public IViewComponentResult Invoke(string customerProfileId)
        {
            var customer = _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
                return Content("");
            }

            //avatar
            var avatarUrl = "";
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                avatarUrl = _pictureService.GetPictureUrl(
                 customer.GetAttribute<string>(SystemCustomerAttributeNames.AvatarPictureId),
                 _mediaSettings.AvatarPictureSize,
                 _customerSettings.DefaultAvatarEnabled,
                 defaultPictureType: PictureType.Avatar);
            }

            //location
            bool locationEnabled = false;
            string location = string.Empty;
            if (_customerSettings.ShowCustomersLocation)
            {
                locationEnabled = true;

                var countryId = customer.GetAttribute<string>(SystemCustomerAttributeNames.CountryId);
                var country = _countryService.GetCountryById(countryId);
                if (country != null)
                {
                    location = country.GetLocalized(x => x.Name);
                }
                else
                {
                    locationEnabled = false;
                }
            }

            //private message
            bool pmEnabled = _forumSettings.AllowPrivateMessages && !customer.IsGuest();

            //total forum posts
            bool totalPostsEnabled = false;
            int totalPosts = 0;
            if (_forumSettings.ForumsEnabled && _forumSettings.ShowCustomersPostCount)
            {
                totalPostsEnabled = true;
                totalPosts = customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount);
            }

            //registration date
            bool joinDateEnabled = false;
            string joinDate = string.Empty;

            if (_customerSettings.ShowCustomersJoinDate)
            {
                joinDateEnabled = true;
                joinDate = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
            }

            //birth date
            bool dateOfBirthEnabled = false;
            string dateOfBirth = string.Empty;
            if (_customerSettings.DateOfBirthEnabled)
            {
                var dob = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                if (dob.HasValue)
                {
                    dateOfBirthEnabled = true;
                    dateOfBirth = dob.Value.ToString("D");
                }
            }

            var model = new ProfileInfoModel
            {
                CustomerProfileId = customer.Id,
                AvatarUrl = avatarUrl,
                LocationEnabled = locationEnabled,
                Location = location,
                PMEnabled = pmEnabled,
                TotalPostsEnabled = totalPostsEnabled,
                TotalPosts = totalPosts.ToString(),
                JoinDateEnabled = joinDateEnabled,
                JoinDate = joinDate,
                DateOfBirthEnabled = dateOfBirthEnabled,
                DateOfBirth = dateOfBirth,
            };

            return View(model);
        }
    }
}