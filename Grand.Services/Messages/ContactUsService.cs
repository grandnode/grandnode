using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using MongoDB.Driver;
using System;

namespace Grand.Services.Messages
{
    /// <summary>
    /// ContactUs interface
    /// </summary>
    public partial class ContactUsService: IContactUsService
    {

        private readonly IRepository<ContactUs> _contactusRepository;
        private readonly IEventPublisher _eventPublisher;

        public ContactUsService(
            IRepository<ContactUs> contactusRepository,
            IEventPublisher eventPublisher)
        {
            this._contactusRepository = contactusRepository;
            this._eventPublisher = eventPublisher;
        }
        /// <summary>
        /// Deletes a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs item</param>
        public virtual void DeleteContactUs(ContactUs contactus)
        {
            if (contactus == null)
                throw new ArgumentNullException("contactus");

            _contactusRepository.Delete(contactus);

            //event notification
            _eventPublisher.EntityDeleted(contactus);

        }

        /// <summary>
        /// Clears table
        /// </summary>
        public virtual void ClearTable()
        {
            _contactusRepository.Collection.DeleteMany(new MongoDB.Bson.BsonDocument());
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
        public virtual IPagedList<ContactUs> GetAllContactUs(DateTime? fromUtc = null, DateTime? toUtc = null,
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
            var contactus = new PagedList<ContactUs>(query, filter, builderSort, pageIndex, pageSize);

            return contactus;


        }

        /// <summary>
        /// Gets a contactus item
        /// </summary>
        /// <param name="contactUsId">ContactUs item identifier</param>
        /// <returns>ContactUs item</returns>
        public virtual ContactUs GetContactUsById(string contactUsId)
        {
            return _contactusRepository.GetById(contactUsId);
        }

        /// <summary>
        /// Inserts a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs</param>
        /// <returns>A contactus item</returns>
        public virtual void InsertContactUs(ContactUs contactus)
        {
            if (contactus == null)
                throw new ArgumentNullException("contactus");

            _contactusRepository.Insert(contactus);

            //event notification
            _eventPublisher.EntityInserted(contactus);

        }

    }
}
