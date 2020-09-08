using Grand.Core;
using Grand.Services.Localization;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    public class SystemConsentCookie : IConsentCookie
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        public SystemConsentCookie(ILocalizationService localizationService, IWorkContext workContext)
        {
            _localizationService = localizationService;
            _workContext = workContext;
        }

        public virtual string SystemName { get => "GrandnodeCookies"; }
        public virtual bool AllowToDisable { get => false; }
        public virtual bool? DefaultState { get => true; }
        public virtual int DisplayOrder { get => 0; }

        public virtual async Task<string> FullDescription()
        {
            return await Task.FromResult(_localizationService.GetResource("PrivacyPreference.System.FullDescription", _workContext.WorkingLanguage.Id));
        }

        public virtual async Task<string> Name()
        {
            return await Task.FromResult(_localizationService.GetResource("PrivacyPreference.System.Name", _workContext.WorkingLanguage.Id));
        }

    }
}
