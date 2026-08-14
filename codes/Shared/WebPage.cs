using ASPNETCoreEmpty.codes.Shared;
using Microsoft.Data.SqlClient;
using ASPNETCoreEmpty.codes.Models;
using ASPNETCoreEmpty.codes.Shared;
using SkyNet;
using System.Data;
using System.Drawing.Drawing2D;

namespace ASPNETCoreEmpty.codes.Shared
{
    public class WebBase : WebPage
    {
        //public override async Task<ApiResponse?> OnRequest(string type, string method = "")
        //{
        //    ApiResponse response = new ApiResponse();
        //    return response;
        //}
        public override async Task<string> OnInit(string type, string method)
        {
            string rlt = string.Empty;

            AuthData? auth = Helper.GetAuthData();
            if (auth == null)
            {
                rlt = "Your session has expired. To protect your security, please sign in again to continue.";
            }
            else
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("type", SqlDbType.NVarChar) { Size = 100, Value = type.Split(".").LastOrDefault() ?? string.Empty });
                parameters.Add(new SqlParameter("method", SqlDbType.NVarChar) { Size = 100, Value = method });
                parameters.Add(new SqlParameter("userid", SqlDbType.NVarChar) { Size = 100, Value = Helper.AuthUserId() });

                var (dt, _) = await Helper.GetDataTable("SELECT dbo.XFN_UserMethodPermission(@userid,@type,@method) AS PERM", parameters);
                string InitPerm = (dt != null && dt.Rows.Count > 0)
                    ? Convert.ToString(dt.Rows[0][0]) ?? string.Empty
                    : string.Empty;
                if (InitPerm == "0")
                {
                    rlt = "Unauthorized Access. To protect your security, please sign in again to continue.";
                }
            }
            return rlt;
        }
        public override async Task OnAfterRender()
        {
            string ut = await UseTranslation();
            if (ut == "1")
            {
                string lang = ClientLanguage;
                //GoogleTranslator gltrn = new GoogleTranslator();
                //HtmlDoc.Context = await gltrn.TranslatePageBodyAsync(lang, HtmlDoc.Context);
            }
        }
        public override async Task<ApiResponse> OnResponse(ApiResponse apiResponse)
        {
            string ut = await UseTranslation();
            if (ut == "1")
            {
                //var gltrn = new GoogleTranslator();
                //string lang = ClientLanguage;

                //var targets = apiResponse.data
                //    .Where(a => a.o == (int)ApiResponse.Action.SetElementContents
                //             || a.o == (int)ApiResponse.Action.ServerPageMethod)
                //    .ToList();

                //if (targets.Count > 0)
                //{
                //    var translated = await gltrn.TranslateHtmlBatchAsync(targets.Select(a => a.p2).ToList(), lang);
                //    for (int i = 0; i < targets.Count; i++)
                //        targets[i].p2 = translated[i];
                //}
            }
            return apiResponse;
        }

        private async Task<string> UseTranslation()
        {
            var (dt, _) = await Helper.GetDataTable("SELECT dbo.XFN_GOOGLE_TRANSLATION_USE() AS APIUSE");
            string _xlateApiUse = (dt != null && dt.Rows.Count > 0)
                ? Convert.ToString(dt.Rows[0][0]) ?? string.Empty
                : string.Empty;
            return _xlateApiUse;
        }

    }
}


