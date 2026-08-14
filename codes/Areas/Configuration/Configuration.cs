using Microsoft.Data.SqlClient;
using ASPNETCoreEmpty.codes.Models;
using ASPNETCoreEmpty.codes.Shared;
using SkyNet;
using System.Data;
using ASPNETCoreEmpty.codes.Areas.Configuration.Models;

namespace ASPNETCoreEmpty.codes.Areas.Configuration
{
    public class Configuration : WebBase
    {
        const string tempItem = @"<div class=""cfg-tab"" data-tab=""cfg-{name}"" data-name=""Configuration_{name}"" id=""{id}"">
                                    <span class=""cfg-t-label"">{label}</span>
                                    <span class=""cfg-t-dot""></span>
                                </div>";

        public override async Task OnInitialized()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("area", SqlDbType.NVarChar) { Size = 100, Value = "Configuration" });
            parameters.Add(new SqlParameter("userid", SqlDbType.NVarChar) { Size = 100, Value = Helper.AuthUserId() });

            var (pagetabs, emsg) = await Helper.GetPageTabs(parameters);
            if (!string.IsNullOrEmpty(emsg))
            {
                throw new System.Exception(emsg);
            }

            string Html = string.Empty;
            foreach (var pagetab in pagetabs ?? new List<PageUserTab>())
            {
                Html += tempItem
                        .Replace("{id}", pagetab.TAB_ID)
                        .Replace("{name}", pagetab.TAB_NAME)
                        .Replace("{label}", pagetab.TAB_LABEL);
            }
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText.Replace("{plhd_pagetabs}", Html);
        }

        public async Task<ApiResponse> OpenTab()
        {
            string t = GetDataValue("t");
            string tabname = GetDataValue("tabname");

            HtmlDocument htmlDoc = await PartialDocument(tabname);
            ApiResponse response = new ApiResponse();
            response.SetElementContents(t, htmlDoc);
            return response;
        }
    }
}
