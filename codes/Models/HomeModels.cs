namespace ASPNETCoreEmpty.codes.Models
{

    public class AuthData
    {
        public string USER_ID { get; set; } = string.Empty;
        public string USER_NAME { get; set; } = string.Empty;
        public string FIRSTNAME { get; set; } = string.Empty;
        public string LASTNAME { get; set; } = string.Empty;
        public string USER_EMAIL { get; set; } = string.Empty;
        public string USER_PHONE { get; set; } = string.Empty;
        public int USER_MFA { get; set; } = 0;
        public int USER_MFA_OK { get; set; } = 0;
    }

    public class AppPage
    {
        public string PAGE_NAME { get; set; } = string.Empty;
        public string PAGE_AREA { get; set; } = string.Empty;
        public string PAGE_INITACTION { get; set; } = string.Empty;
        public string PAGE_ICON { get; set; } = string.Empty;
    }

    public class PageUserTab
    {
        public string TAB_ID { get; set; } = string.Empty;
        public string TAB_NAME { get; set; } = string.Empty;
        public string TAB_LABEL { get; set; } = string.Empty;
    }
    public class PageUserAction
    {
        public string TABACTION { get; set; } = string.Empty;
        public string TABACTION_LABEL { get; set; } = string.Empty;
    }

}
