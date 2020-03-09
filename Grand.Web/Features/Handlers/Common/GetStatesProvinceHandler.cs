using Grand.Core;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using Grand.Web.Features.Models.Common;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Common
{
    public class GetStatesProvinceHandler : IRequestHandler<GetStatesProvince, IList<StateProvinceModel>>
    {
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        public GetStatesProvinceHandler(IStateProvinceService stateProvinceService, IWorkContext workContext, ILocalizationService localizationService)
        {
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
            _localizationService = localizationService;
        }

        public async Task<IList<StateProvinceModel>> Handle(GetStatesProvince request, CancellationToken cancellationToken)
        {
            var states = await _stateProvinceService.GetStateProvincesByCountryId(request.CountryId, _workContext.WorkingLanguage.Id);
            var model = (from s in states select new StateProvinceModel { id = s.Id, name = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) }).ToList();
            //some country is selected
            if (!model.Any())
            {
                //country does not have states
                model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.OtherNonUS") });
            }
            else
            {
                //country has some states
                if (request.AddSelectStateItem)
                {
                    model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.SelectState") });
                }
            }
            return model;
        }
    }
}
