using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// ContactUs interface
    /// </summary>
    public partial class ContactUsService: IContactUsService
    {

        private readonly IRepository<ContactUs> _contactusRepository;
        private readonly IMediator _mediator;

        public ContactUsService(
            IRepository<ContactUs> contactusRepository,
            IMediator mediator)
        {
            _contactusRepository = contactusRepository;
            _mediator = mediator;
        }
        /// <summary>
        /// Deletes a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs item</param>
        public virtual async Task DeleteContactUs(ContactUs contactus)
        {
            if (contactus == null)
                throw new ArgumentNullException("contactus");

            await _contactusRepository.DeleteAsync(contactus);

            //event notification
            await _mediator.EntityDeleted(contactus);

        }

        /// <summary>
        /// Clears table
        /// </summary>
        public virtual async Task ClearTable()
        {
            await _contactusRepository.Collection.DeleteManyAsync(new MongoDB.Bson.BsonDocument());
        }

        /// <summary>
        /// Gets all contactUs items
        /// </summary>
        /// <param name="fromUtc">ContactUs item creation from; null to load all records</param>
        /// <param name="toUtc">ContactUs item creation to; null to load all records</param>
        /// <param name="email">email</param>
        /// <param name="vendorId">vendorId; null to load all records</param>
        /// <param name="customerId">customerId; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>ContactUs items</returns>
        public virtual async Task<IPagedList<ContactUs>> GetAllContactUs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string email = "", string vendorId = "", string customerId = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var builder = Builders<ContactUs>.Filter;
            var filter = builder.Where(c => true);

            if (fromUtc.HasValue)
                filter = filter & builder.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            if (toUtc.HasValue)
                filter = filter & builder.Where(l => toUtc.Value >= l.CreatedOnUtc);
            if (!String.IsNullOrEmpty(vendorId))
                filter = filter & builder.Where(l => vendorId == l.VendorId);
            if (!String.IsNullOrEmpty(customerId))
                filter = filter & builder.Where(l => customerId == l.CustomerId);
            if (!String.IsNullOrEmpty(storeId))
                filter = filter & builder.Where(l => storeId == l.StoreId);

            if (!String.IsNullOrEmpty(email))
                filter = filter & builder.Where(l => l.Email.ToLower().Contains(email.ToLower()));

            var builderSort = Builders<ContactUs>.Sort.Descending(x => x.CreatedOnUtc);
            var query = _contactusRepository.Collection;
            var contactus = await PagedList<ContactUs>.Create(query, filter, builderSort, pageIndex, pageSize);
            return contactus;
        }

        /// <summary>
        /// Gets a contactus item
        /// </summary>
        /// <param name="contactUsId">ContactUs item identifier</param>
        /// <returns>ContactUs item</returns>
        public virtual Task<ContactUs> GetContactUsById(string contactUsId)
        {
            return _contactusRepository.GetByIdAsync(contactUsId);
        }

        /// <summary>
        /// Inserts a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs</param>
        /// <returns>A contactus item</returns>
        public virtual async Task InsertContactUs(ContactUs contactus)
        {
            if (contactus == null)
                throw new ArgumentNullException("contactus");

            await _contactusRepository.InsertAsync(contactus);

            //event notification
            await _mediator.EntityInserted(contactus);

        }

    }
}
