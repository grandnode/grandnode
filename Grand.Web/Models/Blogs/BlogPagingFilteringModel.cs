using Grand.Framework.UI.Paging;
using System;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPagingFilteringModel : BasePageableModel
    {
        #region Methods

        public virtual DateTime? GetParsedMonth()
        {
            DateTime? result = null;
            if (!String.IsNullOrEmpty(Month))
            {
                string[] tempDate = Month.Split(new [] { '-' });
                if (tempDate.Length == 2)
                {
                    int.TryParse(tempDate[0], out var year);
                    int.TryParse(tempDate[1], out var month);
                    try
                    {
                        result = new DateTime(year, month, 1);
                    }
                    catch { }
                }
            }
            return result;
        }
        public virtual DateTime? GetFromMonth()
        {
            var filterByMonth = GetParsedMonth();
            if (filterByMonth.HasValue)
                return filterByMonth.Value;
            return null;
        }
        public virtual DateTime? GetToMonth()
        {
            var filterByMonth = GetParsedMonth();
            if (filterByMonth.HasValue)
                return filterByMonth.Value.AddMonths(1).AddSeconds(-1);
            return null;
        }
        #endregion

        #region Properties

        public string Month { get; set; }
        public string Tag { get; set; }
        public string CategorySeName { get; set; }
        public string SearchKeyword { get; set; }

        #endregion
    }
}