using System.Collections.Generic;
using Grand.Plugin.Feed.GoogleShopping.Domain;

namespace Grand.Plugin.Feed.GoogleShopping.Services
{
    public partial interface IGoogleService
    {
        void DeleteGoogleProduct(GoogleProductRecord googleProductRecord);

        IList<GoogleProductRecord> GetAll();

        GoogleProductRecord GetById(string googleProductRecordId);

        GoogleProductRecord GetByProductId(string productId);

        void InsertGoogleProductRecord(GoogleProductRecord googleProductRecord);

        void UpdateGoogleProductRecord(GoogleProductRecord googleProductRecord);

        IList<string> GetTaxonomyList();
    }
}
