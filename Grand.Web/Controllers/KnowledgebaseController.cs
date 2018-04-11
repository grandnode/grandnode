using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Services.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    [HttpsRequirement(SslRequirement.No)]
    public class KnowledgebaseController : BasePublicController
    {
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly IKnowledgebaseService _knowledgebaseService;

        public KnowledgebaseController(KnowledgebaseSettings knowledgebaseSettings, IKnowledgebaseService knowledgebaseService)
        {
            this._knowledgebaseSettings = knowledgebaseSettings;
            this._knowledgebaseService = knowledgebaseService;
        }

        public IActionResult List()
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            return View("List");
        }


    }
}
