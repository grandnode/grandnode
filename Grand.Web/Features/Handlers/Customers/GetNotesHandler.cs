using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetNotesHandler : IRequestHandler<GetNotes, CustomerNotesModel>
    {
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public GetNotesHandler(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<CustomerNotesModel> Handle(GetNotes request, CancellationToken cancellationToken)
        {
            var model = new CustomerNotesModel();
            model.CustomerId = request.Customer.Id;
            var notes = await _customerService.GetCustomerNotes(request.Customer.Id, true);
            foreach (var item in notes)
            {
                var mm = new CustomerNote();
                mm.NoteId = item.Id;
                mm.CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc);
                mm.Note = item.Note;
                mm.Title = item.Title;
                mm.DownloadId = item.DownloadId;
                model.CustomerNoteList.Add(mm);
            }
            return model;
        }
    }
}
