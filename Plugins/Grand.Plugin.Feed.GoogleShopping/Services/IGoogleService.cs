using Grand.Plugin.Feed.GoogleShopping.Domain;
using System.Collections.Generic;

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
