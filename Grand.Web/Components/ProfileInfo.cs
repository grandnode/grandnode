using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Media;
using Grand.Framework.Components;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ProfileInfoViewComponent : BaseViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICountryService _countryService;

        public ProfileInfoViewComponent(ICustomerService customerService, 
            IWorkContext workContext, IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings, CustomerSettings customerSettings,
            IPictureService pictureService, MediaSettings mediaSettings,
            ICountryService countryService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _countryService = countryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string customerProfileId)
        {
            var customer = await _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
                return Content("");
            }

            //avatar
            var avatarUrl = "";
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                avatarUrl = await _pictureService.GetPictureUrl(
                 customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
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

                var countryId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                var country = await _countryService.GetCountryById(countryId);
                if (country != null)
                {
                    location = country.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
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
                totalPosts = customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount);
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
                var dob = customer.GetAttributeFromEntity<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
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