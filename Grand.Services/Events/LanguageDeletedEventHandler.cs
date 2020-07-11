using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Localization;
using Grand.Core.Events;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using MediatR;

namespace Grand.Services.Events
{
    public class LanguageDeletedEventHandler : INotificationHandler<EntityDeleted<Language>>
    {
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;

        private readonly LocalizationSettings _localizationSettings;

        public LanguageDeletedEventHandler(
            ILanguageService languageService,
            ISettingService settingService,
            LocalizationSettings localizationSettings
            )
        {
            _languageService = languageService;
            _settingService = settingService;
            _localizationSettings = localizationSettings;
        }
        public async Task Handle(EntityDeleted<Language> notification, CancellationToken cancellationToken)
        {
            //update default admin area language (if required)
            if (_localizationSettings.DefaultAdminLanguageId == notification.Entity.Id)
            {
                foreach (var activeLanguage in await _languageService.GetAllLanguages())
                {
                    if (activeLanguage.Id != notification.Entity.Id)
                    {
                        _localizationSettings.DefaultAdminLanguageId = activeLanguage.Id;
                        await _settingService.SaveSetting(_localizationSettings);
                        break;
                    }
                }
            }

        }
    }
}
