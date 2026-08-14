using Microsoft.Data.SqlClient;
using ASPNETCoreEmpty.codes.Models;
using ASPNETCoreEmpty.codes.Shared;
using SkyNet;
using System.Data;
using System.Text.Json;
using static SkyNet.ApiResponse;
using ASPNETCoreEmpty.codes.Areas.Configuration.Models;

namespace ASPNETCoreEmpty.codes.Areas.Configuration
{
    public class Configuration_Users : WebBase
    {
        const string FILE_REF = "Configuration.Users.UploadPhoto";

        const string addusr = @"<button class=""cfgu-btn cfgu-btn-add"" onclick=""Configuration_UsersJs.openAddUserDialog()"">+ Add User</button>";
        const string savebtn = @"<button class=""cfgu-btn cfgu-btn-ghost"" id=""cfgu-discard-btn"" onclick=""Configuration_UsersJs.discardChanges()"" style=""display:none;"">Discard</button>
            <button class=""cfgu-btn cfgu-btn-save"" id=""cfgu-save-btn"" onclick=""Configuration_UsersJs.openDiffDialog()"" disabled>Review &amp; Save</button>";

        public override async Task OnInitialized()
        {
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("area",   SqlDbType.NVarChar) { Size = 100, Value = "Configuration" },
                new SqlParameter("tab",    SqlDbType.NVarChar) { Size = 100, Value = "Users"         },
                new SqlParameter("userid", SqlDbType.NVarChar) { Size = 100, Value = Helper.AuthUserId()   }
            };

            var (PageUserActions, emsg) = await Helper.GetTabActions(parameters);
            if (!string.IsNullOrEmpty(emsg)) throw new Exception(emsg);

            var permAddUser = PageUserActions?.FirstOrDefault(x => x.TABACTION == "Configuration.Users.AddUser");
            var permDeleteUser = PageUserActions?.FirstOrDefault(x => x.TABACTION == "Configuration.Users.DeleteUser");
            var permResetPwd = PageUserActions?.FirstOrDefault(x => x.TABACTION == "Configuration.Users.ResetPassword");
            var permSave = PageUserActions?.FirstOrDefault(x => x.TABACTION == "Configuration.Users.ReviewSave");
            var permUploadPhoto = PageUserActions?.FirstOrDefault(x => x.TABACTION == "Configuration.Users.UploadPhoto");

            var (roles, emsg2) = await GetRoles();
            if (!string.IsNullOrEmpty(emsg2)) throw new Exception(emsg2);

            var (sites, emsg3) = await GetSites();
            if (!string.IsNullOrEmpty(emsg3)) throw new Exception(emsg3);

            string rolesJson = JsonSerializer.Serialize(roles);
            string sitesJson = JsonSerializer.Serialize(sites);
            string permsJson = JsonSerializer.Serialize(new
            {
                save = permSave != null,
                addUser = permAddUser != null,
                uploadPhoto = permUploadPhoto != null,
                deleteUser = permDeleteUser != null,
                resetPwd = permResetPwd != null
            });

            string initData = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{{\"roles\":{rolesJson},\"sites\":{sitesJson},\"perms\":{permsJson}}}"));

            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_initdata}", initData)
                .Replace("{plhd_addusr}", permAddUser != null ? addusr : string.Empty)
                .Replace("{plhd_savebtn}", permSave != null ? savebtn : string.Empty);
        }

        public static async Task<(List<Role>? data, string emsg)> GetRoles()
        {
            string sSql = @"SELECT ROLE_ID, ROLE_NAME, ROLE_DESCRIPTION, ROLE_SORT, ROLE_STATUS
                            FROM XYSROLE
                            ORDER BY ROLE_SORT";
            return await Helper.GetListT<Role>(sSql);
        }

        // ── Search Users ──────────────────────────────────────────────────────
        public async Task<ApiResponse> SearchUsers()
        {
            string q = GetDataValue("q");
            ApiResponse response = new ApiResponse();

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("q",       SqlDbType.NVarChar) { Size = 200, Value = q ?? "" },
                new SqlParameter("fileref", SqlDbType.NVarChar) { Size = 200, Value = FILE_REF }
            };

            string sSql = @"SELECT u.USER_ID, u.USER_NAME, u.USER_STATUS,
                                   u.FIRST_NAME, u.LAST_NAME, u.MID_NAME,
                                   u.USER_EMAIL, u.USER_PHONE, u.USER_MFA, u.USER_SMS,
                                   f.FILELINK AS PHOTO_LINK
                            FROM XYSUSER u
                            LEFT JOIN XYSFILE f ON f.FILEREFID = u.USER_ID AND f.FILEREF = @fileref
                            WHERE @q = ''
                               OR u.FIRST_NAME LIKE '%'+@q+'%'
                               OR u.LAST_NAME  LIKE '%'+@q+'%'
                               OR u.USER_NAME  LIKE '%'+@q+'%'
                               OR u.USER_EMAIL LIKE '%'+@q+'%'
                            ORDER BY u.FIRST_NAME, u.LAST_NAME";

            var (users, emsg) = await Helper.GetListT<CfguUser>(sSql, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow("Error", emsg); return response; }

            var userIds = users?.Select(u => u.USER_ID).ToList() ?? new List<string>();

            var (useRoles, emsg2) = await GetUserRolesByIds(userIds);
            if (!string.IsNullOrEmpty(emsg2)) { response.ModalWindow("Error", emsg2); return response; }

            var (useSites, emsg3) = await GetUserSitesByIds(userIds);
            if (!string.IsNullOrEmpty(emsg3)) { response.ModalWindow("Error", emsg3); return response; }

            string usersJson = JsonSerializer.Serialize(users);
            string useRolesJson = JsonSerializer.Serialize(useRoles);
            string useSitesJson = JsonSerializer.Serialize(useSites);
            response.ExecuteScript($"Configuration_UsersJs.onSearchResult({usersJson},{useRolesJson},{useSitesJson});");
            return response;
        }

        // ── Get User ──────────────────────────────────────────────────────────
        public async Task<ApiResponse> GetUser()
        {
            string userId = GetDataValue("userid");
            ApiResponse response = new ApiResponse();

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("userid",  SqlDbType.NVarChar) { Size = 50,  Value = userId   },
                new SqlParameter("fileref", SqlDbType.NVarChar) { Size = 200, Value = FILE_REF }
            };

            string sSql = @"SELECT u.USER_ID, u.USER_NAME, u.USER_STATUS,
                                   u.FIRST_NAME, u.LAST_NAME, u.MID_NAME,
                                   u.USER_EMAIL, u.USER_PHONE, u.USER_MFA, u.USER_SMS,
                                   f.FILELINK AS PHOTO_LINK
                            FROM XYSUSER u
                            LEFT JOIN XYSFILE f ON f.FILEREFID = u.USER_ID AND f.FILEREF = @fileref
                            WHERE u.USER_ID = @userid";

            var (users, emsg) = await Helper.GetListT<CfguUser>(sSql, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow("Error", emsg); return response; }

            var user = users?.FirstOrDefault();
            if (user == null) return response;

            var (useRoles, emsg2) = await GetUserRolesByIds(new List<string> { userId });
            if (!string.IsNullOrEmpty(emsg2)) { response.ModalWindow("Error", emsg2); return response; }

            var (useSites, emsg3) = await GetUserSitesByIds(new List<string> { userId });
            if (!string.IsNullOrEmpty(emsg3)) { response.ModalWindow("Error", emsg3); return response; }

            string userJson = JsonSerializer.Serialize(user);
            string useRolesJson = JsonSerializer.Serialize(useRoles);
            string useSitesJson = JsonSerializer.Serialize(useSites);
            response.ExecuteScript($"Configuration_UsersJs.onGetUser({userJson},{useRolesJson},{useSitesJson});");
            return response;
        }

        // ── Save User ─────────────────────────────────────────────────────────
        public async Task<ApiResponse> SaveUser()
        {
            string userId = GetDataValue("userid");
            string firstName = GetDataValue("firstname");
            string lastName = GetDataValue("lastname");
            string email = GetDataValue("email");
            string phone = GetDataValue("phone");
            string username = GetDataValue("username");
            int status = int.Parse(GetDataValue("status") ?? "1");
            int mfa = int.Parse(GetDataValue("mfa") ?? "0");
            int sms = int.Parse(GetDataValue("sms") ?? "0");
            string roles = GetDataValue("roles");
            string sites = GetDataValue("sites");

            ApiResponse response = new ApiResponse();

            var sqls = new List<string>
            {
                @"UPDATE XYSUSER SET
                    FIRST_NAME  = @firstname,
                    LAST_NAME   = @lastname,
                    USER_EMAIL  = @email,
                    USER_PHONE  = @phone,
                    USER_NAME   = @username,
                    USER_STATUS = @status,
                    USER_MFA    = @mfa,
                    USER_SMS    = @sms,
                    SYSDTE      = GETDATE(),
                    SYSUSR      = @sysusr
                  WHERE USER_ID = @userid",
                "DELETE FROM XYSUSERROLE WHERE USER_ID = @userid",
                "DELETE FROM XYSUSERSITE WHERE USER_ID = @userid"
            };

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("userid",    SqlDbType.NVarChar) { Size = 50,  Value = userId    },
                new SqlParameter("firstname", SqlDbType.NVarChar) { Size = 100, Value = firstName },
                new SqlParameter("lastname",  SqlDbType.NVarChar) { Size = 100, Value = lastName  },
                new SqlParameter("email",     SqlDbType.NVarChar) { Size = 200, Value = email     },
                new SqlParameter("phone",     SqlDbType.NVarChar) { Size = 50,  Value = phone ?? "" },
                new SqlParameter("username",  SqlDbType.NVarChar) { Size = 100, Value = username  },
                new SqlParameter("status",    SqlDbType.Int)      { Value = status                },
                new SqlParameter("mfa",       SqlDbType.Int)      { Value = mfa                   },
                new SqlParameter("sms",       SqlDbType.Int)      { Value = sms                   },
                new SqlParameter("sysusr",    SqlDbType.NVarChar) { Size = 50,  Value = Helper.AuthUserId() }
            };

            int i = 0;
            foreach (var roleId in (roles ?? "").Split(',').Where(r => !string.IsNullOrEmpty(r)))
            {
                sqls.Add($@"INSERT INTO XYSUSERROLE (USERROLE_ID, USER_ID, ROLE_ID, SYSDTE, SYSUSR)
                            VALUES (NEWID(), @userid, @roleid{i}, GETDATE(), @sysusr)");
                parameters.Add(new SqlParameter($"roleid{i}", SqlDbType.NVarChar) { Size = 50, Value = roleId });
                i++;
            }

            int j = 0;
            foreach (var siteId in (sites ?? "").Split(',').Where(s => !string.IsNullOrEmpty(s)))
            {
                sqls.Add($@"INSERT INTO XYSUSERSITE (USERSITE_ID, USER_ID, SITE_ID, SYSDTE, SYSUSR)
                            VALUES (NEWID(), @userid, @siteid{j}, GETDATE(), @sysusr)");
                parameters.Add(new SqlParameter($"siteid{j}", SqlDbType.NVarChar) { Size = 50, Value = siteId });
                j++;
            }

            Helper help = new Helper(this) {};
            string emsg = await help.PutData(sqls, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }

            var getParams = new List<SqlParameter>
            {
                new SqlParameter("userid",  SqlDbType.NVarChar) { Size = 50,  Value = userId   },
                new SqlParameter("fileref", SqlDbType.NVarChar) { Size = 200, Value = FILE_REF }
            };
            string getSql = @"SELECT u.USER_ID, u.USER_NAME, u.USER_STATUS,
                                     u.FIRST_NAME, u.LAST_NAME, u.MID_NAME,
                                     u.USER_EMAIL, u.USER_PHONE, u.USER_MFA, u.USER_SMS,
                                     f.FILELINK AS PHOTO_LINK
                              FROM XYSUSER u
                              LEFT JOIN XYSFILE f ON f.FILEREFID = u.USER_ID AND f.FILEREF = @fileref
                              WHERE u.USER_ID = @userid";

            var (users, emsg2) = await Helper.GetListT<CfguUser>(getSql, getParams);
            var (useRoles, emsg3) = await GetUserRolesByIds(new List<string> { userId });
            var (useSites, emsg4) = await GetUserSitesByIds(new List<string> { userId });

            string userJson = JsonSerializer.Serialize(users?.FirstOrDefault());
            string useRolesJson = JsonSerializer.Serialize(useRoles);
            string useSitesJson = JsonSerializer.Serialize(useSites);
            response.ExecuteScript($"Configuration_UsersJs.onSaved({userJson},{useRolesJson},{useSitesJson});");
            return response;
        }

        // ── Add User ──────────────────────────────────────────────────────────
        public async Task<ApiResponse> AddUser()
        {
            string firstName = GetDataValue("firstname");
            string lastName = GetDataValue("lastname");
            string email = GetDataValue("email");
            string phone = GetDataValue("phone");
            string username = GetDataValue("username");
            int mfa = int.Parse(GetDataValue("mfa") ?? "0");
            int sms = int.Parse(GetDataValue("sms") ?? "0");
            int status = int.Parse(GetDataValue("status") ?? "1");
            string roleId = GetDataValue("roleid");
            string siteId = GetDataValue("siteid");

            ApiResponse response = new ApiResponse();

            string newUserId = Guid.NewGuid().ToString().ToUpper();

            var sqls = new List<string>
            {
                @"INSERT INTO XYSUSER
                    (USER_ID, USER_NAME, USER_STATUS, FIRST_NAME, LAST_NAME, MID_NAME,
                     USER_EMAIL, USER_PHONE, USER_MFA, USER_SMS, PASSWORD, PASSWORD_EXPIRY,
                     RESET_FLAG, RESET_PASSWORD, RESET_EXPIRY, SYSDTE, SYSUSR)
                  VALUES
                    (@newuserid, @username, @status, @firstname, @lastname, '',
                     @email, @phone, @mfa, @sms, '', GETDATE(),
                     0, '', GETDATE(), GETDATE(), @sysusr)"
            };

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("newuserid", SqlDbType.NVarChar) { Size = 50,  Value = newUserId  },
                new SqlParameter("username",  SqlDbType.NVarChar) { Size = 100, Value = username   },
                new SqlParameter("status",    SqlDbType.Int)      { Value = status                 },
                new SqlParameter("firstname", SqlDbType.NVarChar) { Size = 100, Value = firstName  },
                new SqlParameter("lastname",  SqlDbType.NVarChar) { Size = 100, Value = lastName   },
                new SqlParameter("email",     SqlDbType.NVarChar) { Size = 200, Value = email      },
                new SqlParameter("phone",     SqlDbType.NVarChar) { Size = 50,  Value = phone ?? "" },
                new SqlParameter("mfa",       SqlDbType.Int)      { Value = mfa                    },
                new SqlParameter("sms",       SqlDbType.Int)      { Value = sms                   },
                new SqlParameter("sysusr",    SqlDbType.NVarChar) { Size = 50,  Value = Helper.AuthUserId() }
            };

            if (!string.IsNullOrEmpty(roleId))
            {
                sqls.Add(@"INSERT INTO XYSUSERROLE (USERROLE_ID, USER_ID, ROLE_ID, SYSDTE, SYSUSR)
                           VALUES (NEWID(), @newuserid, @roleid, GETDATE(), @sysusr)");
                parameters.Add(new SqlParameter("roleid", SqlDbType.NVarChar) { Size = 50, Value = roleId });
            }

            if (!string.IsNullOrEmpty(siteId))
            {
                sqls.Add(@"INSERT INTO XYSUSERSITE (USERSITE_ID, USER_ID, SITE_ID, SYSDTE, SYSUSR)
                           VALUES (NEWID(), @newuserid, @siteid, GETDATE(), @sysusr)");
                parameters.Add(new SqlParameter("siteid", SqlDbType.NVarChar) { Size = 50, Value = siteId });
            }

            Helper help = new Helper(this);
            string emsg = await help.PutData(sqls, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }

            var getParams = new List<SqlParameter>
            {
                new SqlParameter("userid",  SqlDbType.NVarChar) { Size = 50,  Value = newUserId },
                new SqlParameter("fileref", SqlDbType.NVarChar) { Size = 200, Value = FILE_REF  }
            };
            string getSql = @"SELECT u.USER_ID, u.USER_NAME, u.USER_STATUS,
                                     u.FIRST_NAME, u.LAST_NAME, u.MID_NAME,
                                     u.USER_EMAIL, u.USER_PHONE, u.USER_MFA, u.USER_SMS,
                                     f.FILELINK AS PHOTO_LINK
                              FROM XYSUSER u
                              LEFT JOIN XYSFILE f ON f.FILEREFID = u.USER_ID AND f.FILEREF = @fileref
                              WHERE u.USER_ID = @userid";

            var (users, emsg2) = await Helper.GetListT<CfguUser>(getSql, getParams);
            var (useRoles, emsg3) = await GetUserRolesByIds(new List<string> { newUserId });
            var (useSites, emsg4) = await GetUserSitesByIds(new List<string> { newUserId });

            string userJson = JsonSerializer.Serialize(users?.FirstOrDefault());
            string useRolesJson = JsonSerializer.Serialize(useRoles);
            string useSitesJson = JsonSerializer.Serialize(useSites);
            response.ExecuteScript($"Configuration_UsersJs.onUserAdded({userJson},{useRolesJson},{useSitesJson});");
            return response;
        }

        // ── Upload Photo ──────────────────────────────────────────────────────
        public async Task<ApiResponse> UploadPhoto()
        {
            string userId = GetDataValue("userid");
            string base64 = GetDataValue("base64");
            string fileType = GetDataValue("filetype");

            ApiResponse response = new ApiResponse();

            string ext = fileType == "image/png" ? ".png" : ".jpg";
            string fileName = userId + ext;

            string photoFolder = PhysicalFolder + GetWebEnv("user.folders.photo");
            string filePath = Path.Combine(photoFolder, fileName);
            string photoLink = VirtualPath + GetWebEnv("user.folders.photo") + "/" + fileName;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64);
                if (!System.IO.Directory.Exists(photoFolder))
                    System.IO.Directory.CreateDirectory(photoFolder);
                System.IO.File.WriteAllBytes(filePath, imageBytes);
            }
            catch (Exception ex) { response.ModalWindow("Error", ex.Message); return response; }

            var sqls = new List<string>
            {
                "DELETE FROM XYSFILE WHERE FILEREF = @fileref AND FILEREFID = @userid",
                @"INSERT INTO XYSFILE (FILEID, FILEREF, FILEREFID, FILETYPE, FILENAME, FILELINK, FILEPATH, SYSDTE, SYSUSR)
                  VALUES (NEWID(), @fileref, @userid, @filetype, @filename, @filelink, @filepath, GETDATE(), @sysusr)"
            };
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("userid",    SqlDbType.NVarChar) { Size = 50,  Value = userId    },
                new SqlParameter("fileref",   SqlDbType.NVarChar) { Size = 200, Value = FILE_REF  },
                new SqlParameter("filetype",  SqlDbType.NVarChar) { Size = 200, Value = fileType  },
                new SqlParameter("filename",  SqlDbType.NVarChar) { Size = 200, Value = fileName  },
                new SqlParameter("filelink",  SqlDbType.NVarChar) { Size = 500, Value = photoLink },
                new SqlParameter("filepath",  SqlDbType.NVarChar) { Size = 500, Value = filePath  },
                new SqlParameter("sysusr",    SqlDbType.NVarChar) { Size = 50,  Value = Helper.AuthUserId() }
            };

            Helper help = new Helper(this);
            string emsg = await help.PutData(sqls, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }

            response.ExecuteScript($"Configuration_UsersJs.onPhotoUploaded('{userId}','{photoLink}');");
            return response;
        }

        // ── Delete User ───────────────────────────────────────────────────────
        public async Task<ApiResponse> DeleteUser()
        {
            string userId = GetDataValue("userid");
            ApiResponse response = new ApiResponse();

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("userid", SqlDbType.NVarChar) { Size = 50, Value = userId }
            };
            var sqls = new List<string>
            {
                @"IF EXISTS (SELECT 1 FROM TECH WHERE USER_ID = @userid)
                      RAISERROR(N'This user is linked to a technician and cannot be deleted.', 16, 1)",
                "DELETE FROM XYSUSERSITE WHERE USER_ID = @userid",
                "DELETE FROM XYSUSERROLE WHERE USER_ID = @userid",
                "DELETE FROM XYSUSER     WHERE USER_ID = @userid"
            };

            Helper help = new Helper(this);
            string emsg = await help.PutData(sqls, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }

            response.ExecuteScript($"Configuration_UsersJs.onUserDeleted('{userId}');");
            return response;
        }

        // ── Reset Password ─────────────────────────────────────────────────────
        public async Task<ApiResponse> ResetUserPassword()
        {
            string userId = GetDataValue("userid");
            ApiResponse response = new ApiResponse();

            // Get user first
            var (users, emsg0) = await Helper.GetListT<CfguUser>(
                "SELECT USER_ID, USER_NAME, FIRST_NAME, LAST_NAME, USER_EMAIL FROM XYSUSER WHERE USER_ID = @userid",
                new List<SqlParameter> { new SqlParameter("userid", SqlDbType.NVarChar) { Size = 50, Value = userId } });
            if (!string.IsNullOrEmpty(emsg0)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg0)); return response; }

            var user = users?.FirstOrDefault();
            if (user == null) { response.ModalWindow(Helper.IssueFound, "User not found."); return response; }

            string resetLinkBase = VirtualPath + "RESETPASSWORD";

            // Build encrypted token: sendingdatetime|userid|expirydatetime
            DateTime now = DateTime.Now;
            DateTime expiry = now.AddMinutes(15);
            string token = $"{now:yyyy-MM-dd HH:mm:ss}|{userId}|{expiry:yyyy-MM-dd HH:mm:ss}";
            string encToken = Encryptor?.EncryptData(token) ?? string.Empty;
            string resetLink = string.IsNullOrEmpty(resetLinkBase)
                ? string.Empty
                : $"{resetLinkBase}?x={Uri.EscapeDataString(encToken)}";

            // Set RESET_FLAG
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("userid",  SqlDbType.NVarChar) { Size = 50,  Value = userId    },
                new SqlParameter("otp",     SqlDbType.NVarChar) { Size = 50,  Value = string.Empty },
                new SqlParameter("expiry",  SqlDbType.DateTime) {             Value = expiry    },
                new SqlParameter("sysusr",  SqlDbType.NVarChar) { Size = 50,  Value = Helper.AuthUserId() }
            };
            var sqls = new List<string>
            {
                @"UPDATE XYSUSER SET
                    RESET_FLAG     = 1,
                    RESET_PASSWORD = @otp,
                    RESET_EXPIRY   = @expiry,
                    SYSDTE         = GETDATE(),
                    SYSUSR         = @sysusr
                  WHERE USER_ID = @userid"
            };

            Helper help = new Helper(this);
            string emsg = await help.PutData(sqls, parameters);
            if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }

            // Build email body
            string linkSection = string.IsNullOrEmpty(resetLink) ? string.Empty :
                $@"<p>Or click the link below to set your new password directly:</p>
                   <p style='margin:16px 0;'>
                       <a href='{resetLink}' style='display:inline-block;padding:10px 24px;background:#D97757;
                          color:#fff;border-radius:6px;text-decoration:none;font-weight:600;font-size:13px;'>
                           Reset My Password
                       </a>
                   </p>
";

            string mailResult = string.Empty;
            if (!string.IsNullOrEmpty(user.USER_EMAIL))
            {
                try
                {
                    var mail = new SkyNet.Mail
                    {
                        ToAddr = new[] { user.USER_EMAIL },
                        Subject = "ServiceNet — Password Reset",
                        Body = string.Format(SkyNet.Mail.TempMailBody, $@"
                            <div style='font-family:system-ui,sans-serif;max-width:480px;'>
                                <h2 style='color:#191815;'>Password Reset</h2>
                                <p>Hi {user.FIRST_NAME},</p>
                                <p>An administrator has initiated a password reset for your account.</p>
                                {linkSection}
                                <p>This link expires in <strong>15 minutes</strong>.</p>
                                <hr style='border:none;border-top:1px solid #E4E0D3;margin:20px 0;'/>
                                <p style='font-size:11px;color:#8C897F;'>ServiceNet v2.6</p>
                            </div>")
                    };
                    mailResult = mail.SendMail();
                }
                catch (Exception ex) { mailResult = ex.Message; }

                if (!string.IsNullOrEmpty(mailResult))
                {
                    response.ExecuteScript($"Configuration_UsersJs.onPasswordReset('warning', '{HtmlEncode(mailResult)}');");
                    return response;
                }
            }

            response.ExecuteScript($"Configuration_UsersJs.onPasswordReset('success', '{HtmlEncode(user.USER_EMAIL)}');");
            return response;
        }

        // ── Data Helpers ──────────────────────────────────────────────────────
        private static async Task<(List<CfguUserRole>? data, string emsg)> GetUserRolesByIds(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0) return (new List<CfguUserRole>(), string.Empty);
            string inClause = string.Join(",", userIds.Select(id => $"'{id}'"));
            string sSql = $"SELECT USER_ID, ROLE_ID FROM XYSUSERROLE WHERE USER_ID IN ({inClause})";
            return await Helper.GetListT<CfguUserRole>(sSql);
        }

        private static async Task<(List<CfguUserSite>? data, string emsg)> GetUserSitesByIds(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0) return (new List<CfguUserSite>(), string.Empty);
            string inClause = string.Join(",", userIds.Select(id => $"'{id}'"));
            string sSql = $"SELECT USER_ID, SITE_ID FROM XYSUSERSITE WHERE USER_ID IN ({inClause})";
            return await Helper.GetListT<CfguUserSite>(sSql);
        }

        private static async Task<(List<CfgsSite>? data, string emsg)> GetSites()
        {
            string sSql = @"SELECT SITE_ID, SITE_NAME FROM XYSSITE WHERE SITE_STATUS = 1 ORDER BY SITE_SORT";
            return await Helper.GetListT<CfgsSite>(sSql);
        }


    }
}

