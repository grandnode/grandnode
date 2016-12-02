using System;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Vendors;
using Grand.Services.Events;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Grand.Services.Vendors
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial class VendorService : IVendorService
    {
        #region Fields

        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="vendorRepository">Vendor repository</param>
        /// <param name="eventPublisher">Event published</param>
        public VendorService(IRepository<Vendor> vendorRepository,
            IEventPublisher eventPublisher)
        {
            this._vendorRepository = vendorRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        public virtual Vendor GetVendorById(string vendorId)
        {
            return _vendorRepository.GetById(vendorId);
        }

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void DeleteVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            vendor.Deleted = true;
            UpdateVendor(vendor);
        }

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        public virtual IPagedList<Vendor> GetAllVendors(string name = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _vendorRepository.Table;

            if (!String.IsNullOrWhiteSpace(name))
                query = query.Where(v => v.Name.ToLower().Contains(name.ToLower()));
            if (!showHidden)
                query = query.Where(v => v.Active);
            query = query.Where(v => !v.Deleted);
            query = query.OrderBy(v => v.DisplayOrder).ThenBy(v => v.Name);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void InsertVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            _vendorRepository.Insert(vendor);

            //event notification
            _eventPublisher.EntityInserted(vendor);
        }

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void UpdateVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            _vendorRepository.Update(vendor);

            //event notification
            _eventPublisher.EntityUpdated(vendor);
        }


        /// <summary>
        /// Gets a vendor note note
        /// </summary>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        /// <returns>Vendor note</returns>
        public virtual VendorNote GetVendorNoteById(string vendorId, string vendorNoteId)
        {
            if (String.IsNullOrEmpty(vendorNoteId))
                return null;
            var vendor = _vendorRepository.GetById(vendorId);
            if(vendor == null)
                return null;

            return vendor.VendorNotes.FirstOrDefault(x => x.Id == vendorNoteId);
        }

        public virtual void InsertVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException("vendorNote");

            var updatebuilder = Builders<Vendor>.Update;
            var update = updatebuilder.AddToSet(p => p.VendorNotes, vendorNote);
            _vendorRepository.Collection.UpdateOneAsync(new BsonDocument("_id", vendorNote.VendorId), update);

            //event notification
            _eventPublisher.EntityInserted(vendorNote);
        }

        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="vendorNote">The vendor note</param>
        public virtual void DeleteVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException("vendorNote");

            var updatebuilder = Builders<Vendor>.Update;
            var update = updatebuilder.Pull(p => p.VendorNotes, vendorNote);
            _vendorRepository.Collection.UpdateOneAsync(new BsonDocument("_id", vendorNote.VendorId), update);

            //event notification
            _eventPublisher.EntityDeleted(vendorNote);
        }

        /// <summary>
        /// Gets a vendor mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>vendor mapping</returns>
        public virtual IList<Vendor> GetAllVendorsByDiscount(string discountId)
        {
            var query = from c in _vendorRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;
            var vendors = query.ToList();
            return vendors;
        }
        #endregion
    }
}