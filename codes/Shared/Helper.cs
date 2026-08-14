using Microsoft.Data.SqlClient;
using ASPNETCoreEmpty.codes.Models;
using SkyNet;
using System.Data;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using static ASPNETCoreEmpty.codes.Shared.Helper;

namespace ASPNETCoreEmpty.codes.Shared
{
    public class Helper
    {
        public static string IssueFound = "Issue Found";

        private string callerArea = string.Empty;
        private string callerClass = string.Empty;
        private string callerMethod = string.Empty;

        //public Helper() { }

        public Helper(object caller, [CallerMemberName] string method = "")
        {
            callerArea = caller?.GetType()?.Namespace?.Split('.').LastOrDefault() ?? string.Empty;
            callerClass = caller?.GetType()?.Name?.Split('_').LastOrDefault() ?? string.Empty;
            callerMethod = method ?? string.Empty;
        }

        private List<string> SqlLogSql(List<string> sSql, List<SqlParameter>? parameters = null)
        {
            List<string> LogSql = new List<string>();

            string paramArray = parameters == null ? string.Empty : string.Join(" | ", parameters.Cast<SqlParameter>()
                .Select(p => $"{p.ParameterName}:{(p.Value == DBNull.Value ? "NULL" : p.Value)}"));

            string userId = AuthUserId();
            string ipAddress = AuthIpAddress();

            static string Esc(string? v) => (v ?? string.Empty).Replace("'", "''");

            string eArea = Esc(callerArea);
            string eClass = Esc(callerClass);
            string eMethod = Esc(callerMethod);
            string eUser = Esc(userId);
            string eIp = Esc(ipAddress);
            string eParam = Esc(paramArray);

            foreach (string sql in sSql)
            {
                string eSql = Esc(sql);
                LogSql.Add($@"
                        DECLARE
                        @v_logArea      NVARCHAR(100) = N'{eArea}',  
                        @v_logClass     NVARCHAR(100) = N'{eClass}',
                        @v_logMethod    NVARCHAR(200) = N'{eMethod}',
                        @v_logUserId    NVARCHAR(50)  = N'{eUser}',
                        @v_logIpAddress NVARCHAR(50)  = N'{eIp}',
                        @v_logStamp     DATETIME      = GETDATE(),
                        @v_logStr       NVARCHAR(MAX) = N'{eSql}',
                        @v_logParam     NVARCHAR(MAX) = N'{eParam}';

                    INSERT INTO XYSLOG (
                        LOG_ID, AREA, CLASS, METHOD, USER_ID, IP_ADDRESS, SQLSTR, SQLPARAM, SYSDTE
                    ) VALUES (
                        NEWID(), @v_logArea, @v_logClass, @v_logMethod, @v_logUserId, @v_logIpAddress, @v_logStr, @v_logParam, @v_logStamp
                    )");
            }
            return LogSql;
        }

        public async Task<string> PutData(List<string> sSql, List<SqlParameter>? parameters = null)
        {
            if (await AppRunMode() == 2)
            {
                return "The requested process is currently locked in archive mode.";
            }
            List<string> LogSql = SqlLogSql(sSql, parameters);
            sSql.AddRange(LogSql);

            SQLData msql = new SQLData();
            string result = await msql.PutDataAsync(sSql, parameters);
            return result;
        }

        public static async Task<(DataTable dt, string emsg)> GetDataTable(string sSql, List<SqlParameter>? parameters = null)
        {
            SQLData msql = new SQLData();
            var (dt, emsg) = await msql.GetDataAsync(sSql, parameters);
            return (dt, emsg);
        }
        public static async Task<(List<T>? data, string emsg)> GetListT<T>(string sSql, List<SqlParameter>? parameters = null)
        {
            SQLData msql = new SQLData();
            var (dt, emsg) = await msql.GetDataAsync(sSql, parameters);
            if (!string.IsNullOrEmpty(emsg)) return (null, emsg);

            WebCore webcore = new WebCore();
            List<T> data = webcore.DataTableListT<T>(dt);
            return (data, string.Empty);
        }
        public static async Task<int> AppRunMode()
        {
            var (dt, emsg) = await GetDataTable(@"select dbo.FN_ORIGINE_MODE()");

            if (!string.IsNullOrEmpty(emsg)) return 2;
            if (dt == null || dt.Rows.Count == 0) return 2;

            return Convert.ToInt16(dt.Rows[0][0].ToString());
        }
        public static string WebAppCookieName()
        {
            WebCore webcore = new WebCore();
            return string.Concat(webcore.WebAppName.Where(c => !char.IsWhiteSpace(c))).Trim();
        }
        public static AuthData? GetAuthData()
        {
            WebCore webcore = new WebCore();
            AuthData? auth = null;
            try
            {
                string cookie = webcore.CookieValue(WebAppCookieName()) ?? string.Empty;
                if (!string.IsNullOrEmpty(cookie))
                {
                    string json = webcore.Encryptor?.DecryptData(cookie) ?? string.Empty;
                    auth = (AuthData?)webcore.DeserializeObject(json, typeof(AuthData));
                    if (auth.USER_MFA == 1)
                    {
                        if (auth.USER_MFA_OK != 1)
                        {
                            auth = null;
                        }
                    }
                }
            }
            catch { /* fallback to defaults */ }
            return auth;
        }
        public static string AuthUserId()
        {
            AuthData? auth = Helper.GetAuthData();
            string UserId = string.Empty;
            if (auth != null)
            {
                UserId = auth.USER_ID;
            }
            return UserId;
        }
        public static string AuthUserEmail()
        {
            AuthData? auth = Helper.GetAuthData();
            string UserEmail = string.Empty;
            if (auth != null)
            {
                UserEmail = auth.USER_EMAIL;
            }
            return UserEmail;
        }
        public static string AuthIpAddress()
        {
            WebCore webcore = new WebCore();
            return webcore.ClientIPAddress;
        }
        public static string ErrContentHtml(string emsg)
        {
            string msg = emsg ?? string.Empty;
            if (msg.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
                msg = msg.Substring("Error:".Length);
            msg = System.Net.WebUtility.HtmlEncode(msg.Trim());

            return
                "<div style=\"padding:20px 24px 22px;font-family:inherit;text-align:center;\">" +
                    "<div style=\"width:46px;height:46px;border-radius:50%;background:#FDE8E8;display:grid;place-items:center;margin:0 auto 12px;\">" +
                        "<svg width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#C95E3E\" stroke-width=\"2\">" +
                            "<circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"/><line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"/>" +
                        "</svg>" +
                    "</div>" +
                    "<div style=\"font-size:13.5px;color:#2A2520;line-height:1.5;margin-bottom:18px;\">" + msg + "</div>" +
                    "<button onclick=\"$ModalOff()\" style=\"background:#2A2520;color:#fff;border:none;border-radius:6px;padding:9px 26px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;\">Close</button>" +
                "</div>";
        }

        public static async Task<(List<AppPage>? data, string emsg)> GetPages()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("userid", SqlDbType.NVarChar) { Size = 100, Value = Helper.AuthUserId() });

            string sSql = $@"SELECT PAGE_NAME, PAGE_AREA, PAGE_INITACTION, PAGE_ICON 
                     FROM XYSPAGE 
                     WHERE PAGE_ACTIVE = 1 AND PAGE_ID IN (
                         SELECT PAGE_ID FROM XYSROLEPAGE WHERE ROLE_ID IN (
                             SELECT ROLE_ID FROM XYSUSERROLE WHERE USER_ID = @userid
                         )
                     )
                     ORDER BY PAGE_SORT";

            return await Helper.GetListT<AppPage>(sSql, parameters);
        }
        public static async Task<(List<PageUserTab>? data, string emsg)> GetPageTabs(List<SqlParameter> parameters)
        {
            string sSql = $@"SELECT TAB_ID, TAB_NAME, TAB_LABEL FROM XYSPAGETAB  
                            WHERE TAB_ID IN (SELECT TAB_ID FROM XYSROLETAB WHERE ROLE_ID IN (select ROLE_ID from XYSUSERROLE WHERE USER_ID = @userid ) )
                                    AND PAGE_ID = (SELECT PAGE_ID FROM XYSPAGE WHERE PAGE_AREA = @area)
                            ORDER BY TAB_SORT ";

            return await Helper.GetListT<PageUserTab>(sSql, parameters);
        }
        public static async Task<(List<PageUserAction>? data, string emsg)> GetTabActions(List<SqlParameter> parameters)
        {
            string sSql = $@"SELECT a.PAGE_AREA + '.' + b.TAB_NAME + '.' + c.TABACTION_NAME AS TABACTION,
                                   c.TABACTION_LABEL AS TABACTION_LABEL
                            FROM XYSPAGE a
                            INNER JOIN XYSPAGETAB b ON a.PAGE_ID = b.Page_Id
                            INNER JOIN
                            (SELECT T1.TABACTION_ID,T1.TAB_ID,T1.TABACTION_NAME,T1.TABACTION_LABEL,T2.ROLE_ID
                            FROM XYSTABACTION T1
                            INNER JOIN XYSROLETABACTION T2 ON T1.TABACTION_ID = T2.TABACTION_ID) c
                            ON b.TAB_ID = c.TAB_ID
                            WHERE a.PAGE_AREA = @area And b.TAB_NAME = @tab AND c.ROLE_ID  IN (select ROLE_ID from XYSUSERROLE WHERE USER_ID = @userid )
                            ORDER BY a.PAGE_SORT, b.TAB_SORT, c.TABACTION_LABEL ";

            return await Helper.GetListT<PageUserAction>(sSql, parameters);
        }

        public static async Task<string> SetNotification(string typeCode, string pageId, string tabName, string apptId, string desc = "", string accountId = "")
        {
            // 1) Round-trip: is this notification type configured + active?
            //    (Validate on CODE only — page/tab come from the caller via reflection
            //     and are stamped onto the NOTIFICATION row below, not used to gate.)
            var (dt, emsg) = await GetDataTable(
                @"SELECT 1 FROM NOTIFICATION_TYPE
          WHERE NOTI_TYPE_CODE = @typecode
            AND IS_ACTIVE      = 1",
                new List<SqlParameter>
                {
            new SqlParameter("typecode", SqlDbType.NVarChar) { Size = 20, Value = typeCode }
                });

            if (!string.IsNullOrEmpty(emsg)) return string.Empty;
            if (dt == null || dt.Rows.Count == 0) return string.Empty;   // not configured → skip

            // 2) Escape code-controlled literals for safe embedding
            string code = typeCode.Replace("'", "''");
            string page = pageId.Replace("'", "''");
            string tab = tabName.Replace("'", "''");

            string descExpr = !string.IsNullOrEmpty(desc)
                ? "N'" + desc.Replace("'", "''") + "'"
                : "N'Appt ' + ISNULL((SELECT APPT_NO FROM APPOINTMENT WHERE APPT_ID=@apptid), N'') + N' — ' + "
                + "ISNULL((SELECT NOTI_TYPE_NAME FROM NOTIFICATION_TYPE WHERE NOTI_TYPE_CODE=N'" + code + "'), N'')";

            var parts = new List<string>();

            // 3a) NOTIFICATION (always) — stamps caller's page/tab
            parts.Add(
                "INSERT INTO NOTIFICATION (NOTI_ID,NOTI_TYPE_ID,PAGE_ID,TAB_NAME,APPT_ID,NOTI_DESC,NOTI_STATUS,SYSDTE,SYSUSR) " +
                "SELECT NEWID()," +
                " (SELECT TOP 1 NOTI_TYPE_ID FROM NOTIFICATION_TYPE WHERE NOTI_TYPE_CODE=N'" + code + "')," +
                " N'" + page + "', N'" + tab + "', @apptid," +
                " " + descExpr + "," +
                " N'PENDING', GETDATE(), @sysusr " +
                "WHERE EXISTS (SELECT 1 FROM NOTIFICATION_TYPE WHERE NOTI_TYPE_CODE=N'" + code + "' AND IS_ACTIVE=1)");

            // 3b) NOTI_CUSTOMER (email) — only when an account is supplied
            if (!string.IsNullOrEmpty(accountId))
            {
                string acct = accountId.Replace("'", "''");
                parts.Add(
                    "INSERT INTO NOTI_CUSTOMER (NOTI_CUSTOMER_ID,NOTI_ID,ACCOUNT_ID,EMAIL,PHONE,NOTI_DATETIME,IS_SENT,SEND_CHANNEL,SEND_STATUS,SEND_ERROR) " +
                    "SELECT NEWID()," +
                    " (SELECT TOP 1 NOTI_ID FROM NOTIFICATION WHERE APPT_ID=@apptid AND NOTI_STATUS=N'PENDING' ORDER BY SYSDTE DESC)," +
                    " N'" + acct + "'," +
                    " ISNULL((SELECT EMAIL FROM ACCOUNT WHERE ACCOUNT_ID=N'" + acct + "'), N''), N''," +
                    " GETDATE(), 0, N'EMAIL', N'PENDING', N'' " +
                    "WHERE EXISTS (SELECT 1 FROM NOTIFICATION_TYPE WHERE NOTI_TYPE_CODE=N'" + code + "' AND IS_ACTIVE=1)");
            }

            // 3c) NOTI_USER role recipient distribution (always) — dup guard + NOTI_USER_PREF opt-out
            parts.Add(
                "INSERT INTO NOTI_USER (NOTI_USER_ID,NOTI_ID,USER_ID,NOTI_DATETIME,IS_READ,READ_DATETIME) " +
                "SELECT NEWID(), n.NOTI_ID, ur.USER_ID, GETDATE(), 0, NULL " +
                "FROM NOTIFICATION n " +
                "INNER JOIN NOTIFICATION_TYPE  nt  ON nt.NOTI_TYPE_ID  = n.NOTI_TYPE_ID " +
                "INNER JOIN NOTI_TYPE_ROLE     ntr ON ntr.NOTI_TYPE_ID = nt.NOTI_TYPE_ID " +
                "INNER JOIN XYSUSERROLE        ur  ON ur.ROLE_ID       = ntr.ROLE_ID " +
                "WHERE n.APPT_ID = @apptid AND nt.NOTI_TYPE_CODE = N'" + code + "' AND nt.IS_ACTIVE = 1 AND n.NOTI_STATUS = N'PENDING' " +
                "AND NOT EXISTS (SELECT 1 FROM NOTI_USER nu2 WHERE nu2.NOTI_ID = n.NOTI_ID AND nu2.USER_ID = ur.USER_ID) " +
                "AND NOT EXISTS (SELECT 1 FROM NOTI_USER_PREF p WHERE p.NOTI_TYPE_ID = nt.NOTI_TYPE_ID AND p.USER_ID = ur.USER_ID AND p.NOTI_ONOFF = 0)");

            return string.Join("; ", parts);
        }



    }
}
