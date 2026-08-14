namespace ASPNETCoreEmpty.codes.Areas.Configuration.Models
{
    public class PageProperty
    {
        public string PAGE_ID { get; set; } = string.Empty;
        public string PAGE_NAME { get; set; } = string.Empty;
        public string PAGE_AREA { get; set; } = string.Empty;
        public string PAGE_INITACTION { get; set; } = string.Empty;
        public int PAGE_SORT { get; set; } = 0;
        public string TAB_ID { get; set; } = string.Empty;
        public string TAB_NAME { get; set; } = string.Empty;
        public string TAB_LABEL { get; set; } = string.Empty;
        public Nullable<int> TAB_SORT { get; set; } = 0;
        public string TABACTION_ID { get; set; } = string.Empty;
        public string TABACTION_NAME { get; set; } = string.Empty;
        public string TABACTION_LABEL { get; set; } = string.Empty;
        public string TABACTION_METHOD { get; set; } = string.Empty;
    }

    public class Role
    {
        public string ROLE_ID { get; set; } = string.Empty;
        public string ROLE_NAME { get; set; } = string.Empty;
        public string ROLE_DESCRIPTION { get; set; } = string.Empty;
        public int ROLE_SORT { get; set; }
        public int ROLE_STATUS { get; set; }
    }

    public class RolePermission
    {
        public string ROLE_ID { get; set; } = string.Empty;
        public string TYPE { get; set; } = string.Empty;
        public string KEY { get; set; } = string.Empty;
    }

    public class Page
    {
        public string PAGE_ID { get; set; } = string.Empty;
        public string PAGE_NAME { get; set; } = string.Empty;
    }

    public class PageTab
    {
        public string PAGE_ID { get; set; } = string.Empty;
        public string TAB_ID { get; set; } = string.Empty;
        public string TAB_NAME { get; set; } = string.Empty;
        public string TAB_LABEL { get; set; } = string.Empty;
    }

    public class TabAction
    {
        public string TAB_ID { get; set; } = string.Empty;
        public string TABACTION_ID { get; set; } = string.Empty;
        public string TABACTION_NAME { get; set; } = string.Empty;
        public string TABACTION_LABEL { get; set; } = string.Empty;
        public string TABACTION_METHOD { get; set; } = string.Empty;
    }

    public class PageTreeItem
    {
        public string PAGE_ID { get; set; } = string.Empty;
        public string PAGE_NAME { get; set; } = string.Empty;
        public int PAGE_SORT { get; set; }
        public List<TabTreeItem>? tabs { get; set; }
    }

    public class TabTreeItem
    {
        public string TAB_ID { get; set; } = string.Empty;
        public string TAB_NAME { get; set; } = string.Empty;
        public string TAB_LABEL { get; set; } = string.Empty;
        public int TAB_SORT { get; set; }
        public List<ActionItem>? actions { get; set; }
    }

    public class ActionItem
    {
        public string TABACTION_ID { get; set; } = string.Empty;
        public string TABACTION_NAME { get; set; } = string.Empty;
        public string TABACTION_LABEL { get; set; } = string.Empty;
    }

    // ── Updated for Permissions ───────────────────────────────────────────
    public class PermRow
    {
        public string ROLE_ID { get; set; } = string.Empty;
        public string ITEM_ID { get; set; } = string.Empty;
    }

    public class PermData
    {
        public List<PermRow> pages { get; set; } = new List<PermRow>();
        public List<PermRow> tabs { get; set; } = new List<PermRow>();
        public List<PermRow> actions { get; set; } = new List<PermRow>();
    }

    public class PermChange
    {
        public string type { get; set; } = string.Empty;  // "page" | "tab" | "action"
        public string id { get; set; } = string.Empty;  // PAGE_ID, TAB_ID, or TABACTION_ID
        public string was { get; set; } = string.Empty;
        public string now { get; set; } = string.Empty;
    }

    // ── Local Models ──────────────────────────────────────────────────────
    public class CfguUser
    {
        public string USER_ID { get; set; } = string.Empty;
        public string USER_NAME { get; set; } = string.Empty;
        public int USER_STATUS { get; set; }
        public string FIRST_NAME { get; set; } = string.Empty;
        public string LAST_NAME { get; set; } = string.Empty;
        public string MID_NAME { get; set; } = string.Empty;
        public string USER_EMAIL { get; set; } = string.Empty;
        public string USER_PHONE { get; set; } = string.Empty;
        public int USER_MFA { get; set; }
        public int USER_SMS { get; set; }
        public string PHOTO_LINK { get; set; } = string.Empty;
    }

    public class CfguUserRole
    {
        public string USER_ID { get; set; } = string.Empty;
        public string ROLE_ID { get; set; } = string.Empty;
    }

    public class CfguUserSite
    {
        public string USER_ID { get; set; } = string.Empty;
        public string SITE_ID { get; set; } = string.Empty;
    }
     
    public class CfgsSite
    {
        public string SITE_ID { get; set; } = string.Empty;
        public string SITE_NAME { get; set; } = string.Empty;
        public string SITE_CODE { get; set; } = string.Empty;
        public string SITE_TYPE { get; set; } = string.Empty;
        public int SITE_STATUS { get; set; }
        public int SITE_SORT { get; set; }
        public string SITE_DESCRIPTION { get; set; } = string.Empty;
        public string CONTACT_NAME { get; set; } = string.Empty;
        public string PHONE { get; set; } = string.Empty;
        public string PHONE2 { get; set; } = string.Empty;
        public string EMAIL { get; set; } = string.Empty;
        public string WEBSITE { get; set; } = string.Empty;
        public string LINE1 { get; set; } = string.Empty;
        public string LINE2 { get; set; } = string.Empty;
        public string CITY { get; set; } = string.Empty;
        public string STATE { get; set; } = string.Empty;
        public string ZIP { get; set; } = string.Empty;
        public string TIMEZONE { get; set; } = string.Empty;
    }

    public class CfgsSiteType
    {
        public string SD01 { get; set; } = string.Empty;  // label e.g. "Headquarters"
        public string SD02 { get; set; } = string.Empty;  // code  e.g. "HQ"
        public int SNO { get; set; }
    }

    public class CfgsState
    {
        public string SD01 { get; set; } = string.Empty;  // code  e.g. "NC"
        public string SD02 { get; set; } = string.Empty;  // label e.g. "North Carolina"
    }


}
