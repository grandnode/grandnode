namespace Grand.Admin.Extensions
{
    public static class Constants
    {
        public const string AreaAdmin = "Admin";

        public static string Layout_Admin => $"~/Areas/{AreaAdmin}/Views/Shared/_AdminLayout.cshtml";
        public static string Layout_AdminPopup => $"~/Areas/{AreaAdmin}/Views/Shared/_AdminPopupLayout.cshtml";
        public static string Layout_AdminLogin => $"~/Areas/{AreaAdmin}/Views/Shared/_AdminLoginLayout.cshtml";
    }
}
