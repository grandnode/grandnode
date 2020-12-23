namespace Grand.Web.Areas.Admin.Validators.Common
{
    public static class CommonValid
    {
        public static bool IsCommissionValid(decimal? commission)
        {
            if (!commission.HasValue)
                return true;

            if (commission < 0 || commission > 100)
                return false;

            return true;
        }

    }
}
