---
name: skynet-framework
description: >-
  Architecture and conventions of the SkyNet web framework (ASP.NET Core / .NET 10,
  SQL Server, vanilla JavaScript, no SPA/build step) that underpins the ServiceNet,
  SalesNet, and BizJournal products. Use this whenever writing, reading, reviewing,
  or debugging any page built on SkyNet — the .cs page class, its .js/.css/.html
  companions, or its SQL. It covers the self-contained per-page model, the
  skynet.core.js "$" client engine and its WebAct DOM-operation opcodes, the server
  ApiResponse / SQLData / WebBase / Helper model, the XFN permission system and its
  XYS* tables, CS→JS variable transfer, and application.cfg configuration. Reach for
  this skill even when the request only mentions a single page, a method, an opcode,
  a permission grant, or "the $ call" — anything on SkyNet touches these conventions.
---

# SkyNet Framework

SkyNet is a **server-driven** web framework: ASP.NET Core / .NET 10 on the back end,
SQL Server for data, and **vanilla JavaScript** on the client with **no SPA and no
build step**. The defining idea: an interaction never returns a new HTML page. The
browser POSTs to a page method; the method returns a **JSON list of DOM operations**;
a small client engine (`skynet.core.js`) applies them in place. The server owns the
DOM and pushes surgical mutations to it.


## 1. Request lifecycle (one round trip)

1. A user action calls `$('Method')`, `$Call('Method', 'k=v&...')`, or an opcode
   re-invokes a call. Names are relative to the current page.
2. The client POSTs **FormData** to `<area>/<Page>/<Method>` (URI derived from the
   current path by `$appPath`).
3. `WebBase.OnInit` authenticates; the framework routes to the page method; the
   method returns an **`ApiResponse`**.
4. The response body is JSON: `{ "data": [ {o,k,p1,p2}, ... ] }` — a list of DOM ops.
5. The client's `$ApiRequestSuccess` → `$WebAct(JSON.parse(...))` applies each op.

If the response body begins with `<!DOCTYPE html>` the client does a full
`document.write` (used for hard navigations / login redirects).

---

## 2. Server — `ApiResponse`

 The ApiResponse class is the central component for server-side control over the client's browser in the SkyLite/SkyNet framework. 
 It serves as the required return type for any server-side function invoked by the client-side $ApiRequest function. 
 Instead of returning raw data, ApiResponse acts as a powerful command container or instruction set. 
 A developer instantiates an ApiResponse object, populates it with a sequence of commands by calling its methods, 
 and then returns the object. The framework serializes these commands and sends them to the client, 
 where framework core JavaScript library interprets and executes them in order, enabling dynamic and 
 real-time manipulation of the web page without a full postback or page refresh.

### Usage
public ApiResponse ProcessForm()
{
    // 1. Instantiate
    ApiResponse response = new ApiResponse();

    // 2. Populate with commands
    response.SetElementContents("statusMessage", "Processing complete!");
    response.SetElementStyle("submitButton", "display", "none");
    response.Navigate("ConfirmationPage");

    // 3. Return
    return response;
}

### Method signatures
** DataId : using element attribute data-id = "elmid",
- public void SetBodyContents(string contentsHtml)
- public void SetBodyContents(HtmlDocument htmlDoc)
- public void AddBodyContents(string contentsHtml)
- public void AddBodyContents(HtmlDocument htmlDoc)
- public void SetElementAttribute(string elementId, string attribute, string value)
- public void SetElementAttributeByName(string elementName, string attribute, string value)
- public void SetElementAttributeByDataId(string dataId, string attribute, string value)
- public void RemoveElementAttribute(string elementId, string attribute)
- public void RemoveElementAttributeByName(string elementName, string attribute)
- public void RemoveElementAttributeByDataId(string dataId, string attribute)
- public void SetElementValue(string elementId, string contentsHtml)
- public void SetElementValueByName(string elementName, string contentsHtml)
- public void SetElementContents(string elementId, string contentsHtml)
- public void SetElementContents(string elementId, HtmlDocument htmlDoc)
- public void AppendScript(string scriptUrl)
- public void RemoveScript(string scriptUrl)
- public void AppendLink(string linkUrl)
- public void RemoveLink(string linkUrl)
- public void SetElementContentsByName(string elementName, string contentsHtml)
- public void SetElementContentsByDataId(string dataId, string contentsHtml)
- public void AddElementContents(string elementId, string contentsHtml)
- public void AddElementContents(string elementId, HtmlDocument htmlDoc)
- public void AddElementContentsByName(string elementName, string contentsHtml)
- public void AddElementContentsByDataId(string dataId, string contentsHtml)
- public void RemoveElementContents(string elementId)
- public void RemoveElementContentsByName(string elementName)
- public void RemoveElementContentsByDataId(string dataId)
- public void SetElementStyle(string elementId, string style, string value)
- public void SetElementStyleByName(string elementName, string style, string value)
- public void SetElementStyleByDataId(string dataId, string style, string value)
- public void RemoveElementStyle(string elementId, string style)
- public void RemoveElementStyleByName(string elementName, string style)
- public void RemoveElementStyleByDataId(string dataId, string style)
- public void ReplaceElement(string elementId, string contentsOuterHtml)
- public void ReplaceElement(string elementId, HtmlDocument htmlDoc)
- public void SetIFrameContents(string iFrameId, string contentsHtml)
- public void ReplaceElementByName(string elementName, string contentsOuterHtml)
- public void RemoveElement(string elementId)
- public void RemoveElementByName(string elementName)
- public void ReplaceText(string searchText, string replaceText, string rootElementId = "")
- public void TableToggleRow(string tableCellElementId, string contentsOuterHtml)
- public void ExecuteScript(string jsScripts)
- public void SetVariableData<T>(string name, T obj, Translator? HtmlTranslator = null)
- public void CallAction(string func, string parameter)
- public void CallActionEnc(string func, string parameter)
- public void RemoveFunction(string jsFunctionName)
- public void ServerMethod(string func, string parameter = "")
- public void ServerPageMethod(string type, string func, string parameter = "")
- public void PopOff()
- public void StoreLocalValue(string key, string value)
- public void StoreLocalValue(KeyValuePair<string, string> keyValue)
- public void StoreLocalValue(List<KeyVlu> keyValueList)
- public void StoreGlobalValue(string key, string value)
- public void SetGlobalKey(string keyValue)
- public void ClearStorage()
- public void RemoveLocalValue(string key)
- public void SetCookie(string key, string value, int duration = 0)
- public void SetCookie(List<KeyVlu> keyVluList, int duration = 0)
- public void RemoveCookie(string key)
- public void StoreSessionValue(string key, string value)
- public void ClearSessionValues()
- public void RemoveSessionValue(string key)
- public void PopUpElement(string contentsHtml)
- public void PopUpMenu(string contentsHtml)
- public void PopUpWindow(string contentsHtml, string blockElementId = "")
- public void PopUpWindow(HtmlDocument htmlDoc, string blockElementId = "")
- public void ModalWindow(string titleText, string contentsHtml)
- public void ModalWindow(string titleText, HtmlDocument htmlDoc)
- public void MessageBox(string message)
- public void Navigate(string pagename)
- public void Navigate(string pagename, string paramsValue)
- public void Navigate2(string pagename, string paramsValue)
- public void NewWindow(string pagename)
- public void DownloadFile(string virtualFilepath)
- public void DownloadFile(string saveAsName, string filepath)

---



## 3. Client engine — `skynet.core.js`

Minified as `skynet.core.min.js`(Skynet framework Built-In) populates automatically in "GET" round trip. 
Everything is exposed as global `$`-prefixed functions. 

### The POST envelope (always sent)

`$ApiRequest` appends, on every call:

- `#data` — the JSON payload
- `#offset` — `new Date().getTimezoneOffset()`
- `#time` — `$getDT()` (local `YYYY-M-D H:M:S`)
- `#tzn` — `$getTZN()` (IANA-ish zone label from the Date string)
- **all URL query params** (via `$UrlParams`)
- **all page-relevant localStorage** (via `$getLocalData`)
- **all form field values** (via `$getElmValues`): every `input`/`select`/`textarea`/
  `button` keyed by `id || name`; checkboxes/radios only when checked (value, or `'1'`
  for a bare `on`); `file` inputs append their `File` objects.

### $ApiRequest: $ApiRequest(targetFunction, data, successCallback, errorCallback);
 The $ApiRequest function is the primary client-side mechanism for initiating asynchronous 
 POST requests to the server within the SkyLite framework. 
 It encapsulates the underlying AJAX communication, providing a streamlined interface for 
 sending data and handling the results of the server-side operation. 
 Its asynchronous nature means it does not block the user interface while waiting for the server's response.

-targetFunction: function or page/function in same page class or different class method
-data (JSON String) - Optional
-successCallback (Function) - Optional (built-in function inside)
-errorCallback (Function) - Optional (built-in function inside)

So server methods can read any on-screen field or stored value without it being named
explicitly in `#data`.

### example : myPage.js
function saveProfile() {
    var data = [
        { key: 'name', vlu: userName }
    ];
    
    // when call saveProfile() method in myPage.cs with no parameter -- same method name as in the class 
    $ApiRequest();

    // when call saveProfile() method in myPage.cs with parameter -- same method name as in the class 
    $ApiRequest('saveProfile', JSON.stringify(data)); or  $ApiRequest(null, JSON.stringify(data));

    // when call myProfile() method in myPage.cs with parameter -- different method name as in the class 
    $ApiRequest('myProfile', JSON.stringify(data)); or  $ApiRequest(null, JSON.stringify(data));

    // when call saveProfile() method in anotherPage.cs with no parameter -- same method name as in the class 
    $ApiRequest('anotherPage/saveProfile');

    // when call saveProfile() method in anotherPage.cs with parameter -- same method name as in the class 
    $ApiRequest('anotherPage/saveProfile', JSON.stringify(data), onSaveSuccess, onSaveError);

    // when call myProfile() method in anotherPage.cs with parameter -- different method name and class 
    $ApiRequest('anotherPage/myProfile', JSON.stringify(data));
}

---

### localStorage scoping

Keys are namespaced. A key prefixed `#global.` is global; otherwise it's implicitly
scoped to the current page (`<pagename>.<key>`). `$getLocalData` transmits global keys
as-is and strips the page prefix from page-scoped keys. Set/clear via opcodes 90–92 or
`$` helpers.

---
### Overlays, popups, modals (Built-Ins)

- **`$WaitOn()` / `$WaitOff()`** — the global spinner overlay (`#loading-overlay`).
- **`$PopUp(html, bh)`** — raw positioned popup (auto-fits within the viewport).
- **`$PopOn(body, target)`** — centered card popup; `body` may be **base64** (auto-
  detected via `$isBase64` → `atob`); optionally disables pointer events on `target`.
- **`$PopOff()`** — closes the current page's popup (`$ppId()` = per-page popup id).
- **`$ModalOn(title, content)` / `$ModalOff()`** — the shared `modal_overlay`; title
  and content are base64-aware.

---

## 4. SkyNet Classes

### Webpage class
public HtmlDocument HtmlDoc { get; set; }
public bool Translation { get; set; } = false;
public bool AllowSkyApi { get; set; } = false;

public Translator HtmlTranslator { get; } = new();
public ApiResponse TranslateApiResponse(ApiResponse apiResponse)

public virtual async Task<string> OnInit(string type, string func)
public virtual async Task<string> OnLoad()
public virtual async Task<string> OnPartialPage(string param = "")
public async Task<HtmlDocument> OnPartialDocument(string param = "")
public virtual async Task OnInitialized()
public virtual async Task OnBeforeRender()
public virtual async Task OnAfterRender()
public virtual async Task<ApiResponse?> OnRequest(string type = "", string method = "")
public virtual async Task<ApiResponse> OnResponse(ApiResponse apiResponse)
public async Task<string> OnApiCall()
public virtual async Task<string> ApiCall()
public string ReplaceBodyContents(string htmlText, string replaceWith)

---
### Translator class

private const string Prefix = "{!";
private const string Suffix = "!}";
public string IsoCode { get; set; } = string.Empty;
public List<DictionaryEntry> TDictionary { get; set; } = new();
public Translator() => IsoCode = new WebCore().ClientLanguage;
public static string Format(string key) => Prefix + key + Suffix;
public static string UnFormat(string key)
public void Add(string key, string word, string isoCode = "*")
public void Add(List<DictionaryEntry> entries)
public void Remove(string key)
public string Value(string key)
public string Translate(string text)
- public class DictionaryEntry
{
    public string IsoCode { get; set; } = string.Empty;
    public string DicKey { get; set; } = string.Empty;
    public string DicWord { get; set; } = string.Empty;
}
- usage
 public static async Task<List<Translator.DictionaryEntry>> GetPageDict(string pagename, string clientLanguage = "")
 {
     List<Translator.DictionaryEntry> rlt = new List<Translator.DictionaryEntry>();

     string lang = clientLanguage;
     if (string.IsNullOrEmpty(lang))
     {
         WebCore webcore = new WebCore();
         lang = webcore.ClientLanguage ?? string.Empty;
     }
     if (string.IsNullOrEmpty(lang)) lang = "en-US";

     string sSql;
     if (lang.Contains("-"))
     {
         sSql = @"IF EXISTS (SELECT * FROM XYSDICT WHERE ISOCODE = @isocode)
                  BEGIN
                      SELECT TARGET, ISOCODE, KEYWORD, TRANSLATED FROM XYSDICT
                      WHERE (TARGET = @pageid OR TARGET = N'*')
                        AND (ISOCODE = N'*' OR ISOCODE = @isocode)
                      ORDER BY KEYWORD
                  END
                  ELSE
                  BEGIN
                      SELECT TARGET, ISOCODE, KEYWORD, TRANSLATED FROM XYSDICT
                      WHERE (TARGET = @pageid OR TARGET = N'*')
                        AND (ISOCODE = N'*' OR ISOCODE = N'en-US')
                      ORDER BY KEYWORD
                  END";
     }
     else
     {
         sSql = @"IF EXISTS (SELECT * FROM XYSDICT WHERE LEFT(ISOCODE, 2) = @isocode)
                  BEGIN
                      SELECT TARGET, ISOCODE, KEYWORD, TRANSLATED FROM XYSDICT
                      WHERE (TARGET = @pageid OR TARGET = N'*')
                        AND (ISOCODE = N'*' OR ISOCODE = @isocode)
                      ORDER BY KEYWORD
                  END
                  ELSE
                  BEGIN
                      SELECT TARGET, ISOCODE, KEYWORD, TRANSLATED FROM XYSDICT
                      WHERE (TARGET = @pageid OR TARGET = N'*')
                        AND (ISOCODE = N'*' OR ISOCODE = N'en-US')
                      ORDER BY KEYWORD
                  END";
     }

     List<SqlParameter> parameters = new List<SqlParameter>
     {
         new SqlParameter("pageid",  SqlDbType.NVarChar) { Size = 50, Value = pagename ?? string.Empty },
         new SqlParameter("isocode", SqlDbType.NVarChar) { Size = 10, Value = lang }
     };

     var (dt, emsg) = await GetDataTable(sSql, parameters);
     if (!string.IsNullOrEmpty(emsg) || dt == null || dt.Rows.Count == 0) return rlt;

     foreach (DataRow row in dt.Rows)
     {
         rlt.Add(new Translator.DictionaryEntry
         {
             IsoCode = row["ISOCODE"]?.ToString() ?? string.Empty,
             DicKey = row["KEYWORD"]?.ToString() ?? string.Empty,
             DicWord = row["TRANSLATED"]?.ToString() ?? string.Empty
         });
     }

     return rlt;
 }



---
### HtmlDocument class

public string HtmlTitle => _title;
public string HtmlName => _name;
public string Context { get; set; } = string.Empty;
public string Message { get; set; } = string.Empty;
public string HtmlBodyText { get; set; } = string.Empty;
public string HtmlBodyAddOn { get; set; } = string.Empty;
public List<HtmlElement> HtmlElements { get; set; } = new();
public HtmlModes HtmlMode { get; set; } = HtmlModes.Regular;
public ApiResponse InitialScripts { get; set; } = new();
public bool ContentSecurityPolicy { get; set; } = true;
public HtmlDocument() { }
public HtmlDocument(string typeName) { _title = typeName; _name = typeName; InitHtml(); }
public void SetTitle(string pageTitle)
public void AddJsFile(string jsFileName, List<NameValue>? attributes = null, string innerScript = "")
public void AddCSSFile(string cssFileName)
public void AddCSSScript(string css)
public void AddJSScript(string js)
public void AddMetaTag(List<NameValue> attribs)
public void AddMetaElement(string name, string contents)
public void AddHeaderScript(string script, List<NameValue> attributeList)
public void AddHeaderScript(string script, string attributes = "")
public void AddBodyScript(string script, string attributes = "")

---
### WebCore class
public string WebAppName 
public string WebAppVersion
public string WebAppRelease
public string WebAppRunMode
public string WebAppRegisterNo
public string WebAppStartUp
public string UriAuthority
public static string AppName
public string EncryptKey
public Encrypt? Encryptor
public string VirtualPath
public string PhysicalFolder
public string ConfigFolder
public string IsoCode
public string PageTimeout
public string WaitImageUri
public string CodeFolder
public string HtmlFolder
public string ScriptFolder
public string StyleFolder
public string ImageFolder
public static string LogFolder
public string TempFolder
public string DataFolder 
public string BinFolder 
public string CoreFile 
public string HtmlPath 
public string ScriptPath 
public string StylePath 
public string ImagePath 
public string LogPath 
public string TempPath 
public string DataPath 
public string BinPath 
public string ScriptAliasPath 
public string StyleAliasPath 
public string ImageAliasPath 
public string LogAliasPath 
public string CoreAuth 
 public string GlobalParam 
 public string GlobalAuth 
 public string GlobalAuthName 
 public string GlobalKey 
 public string AuthToken 
 public string RequestPath 
 public string RequestType 
 public string ContextData 
 public string PartialData 
 public string RequestData 
 public string RequestReferrer 
 public string ClientIPAddress 
 public string ClientLanguage 
 public string ClientTimeZone 
 public int ClientTimeOffset 
 public string ClientTime 
 public string ClientAgent 
 public CultureInfo? ClientCulture 
 public int CSTimeOffset 
 public int ServerTimeOffset 
 public string ServerTimeZone 
 public string ServerTime 

public int RandNUM(int min = 0, int max = 99999)
public async Task<string> PartialPage(string typeName, string param = "")
public async Task<HtmlDocument> PartialDocument(string typeName, string param = "")
public string ReadHtmlFile(string htmlname, string htmlFilePath = "")
public string ReadTextFile(string filePath)
public string ByPassCall(string functionName, string paramsValue = "", bool encryptFunc = true)
public string CallAction(string functionName, string paramsValue = "")
public string CallActionEnc(string functionName, string param = "")
public string GetWebEnv(string key)
public static string NewID(int Lower = 0, bool Dash = true)
public string AbbrOfTimeZone(string timeZoneName)
public string EncryptString(Encrypt encryptor, string data, bool encode = false)
public string EncryptString(string data, bool encode = false)
public string DecryptString(Encrypt encryptor, string encdata, bool decode = false)
public string DecryptString(string encdata, bool decode = false)
public string EncryptPair(KeyVlu keyVluPair, bool encode = false)
public KeyVlu? DecryptPair(string encdata, bool decode = false)
public string EncryptListOfPair(List<KeyVlu> keyValues, bool encode = false)
public List<KeyVlu>? DecryptListOfPair(string encdata, bool decode = false)
public string JsonSerializeObject(object obj, Type? type = null)
public string JsonSerializeObjectEnc(object obj, Type? type = null)
public T? JsonDeserializeObject<T>(string objdata)
public T? JsonDeserializeObjectEnc<T>(string encdata)
public string SerializeObject(object obj, Type objtype, bool encode = false)
public string SerializeObjectEnc(object obj, Type objtype, bool encode = false)
public object? DeserializeObject(string objdata, Type objtype, bool decode = false)
public object? DeserializeObjectEnc(string encdata, Type objtype, bool decode = false)
public T? DeserializeKeyValue<T>(string serializedKeyValue)
public T? CloneObject<T>(T originObj)
public List<T> DataTableListT<T>(DataTable dt)
public T DataTableT<T>(DataTable dt)
public DataTable ListToDataTable<T>(List<T> tList)
public string ParamX()
public string PageLinkX(string pagename, string xparam = "")
public string PageLinkX(string pagename, List<KeyVlu> xparam)
public string FileSteamLink(string paramsValue)
public string FileSteamParam(string filename, string filepath)
public string DownLoadFileLink(string filename, string filepath)
public string PageLink(string pagename, string paramsValue = "")
public string PageLink(string pagename, List<KeyVlu> paramsValue)
public string RemoveXSS(string value)
public string QueryValue(string key, bool decode = false)
public Dictionary<string, string> QryValueAll()
public List<KeyVlu> QueryValueList(string queryValue)
public string HeaderValue(string key)
public string[] HeaderValues(string key)
public bool IsCookieKey(string key)
public string CookieValue(string key, bool decode = false)
public bool IsParamKey(string key)
public string ParamValue(string key, bool decode = false)
public void SetKeyValue(ref List<KeyVlu> keyValueList, string key, string vlu)
public string GetKeyValue(List<KeyVlu> keyValueList, string key)
public string GetKeyValue(string keyValueData, string key)
public string GetKeyValueEnc(string keyValueData, string key)
public List<KeyVlu>? DecryptActionData(bool decode = false)
public string GetActDataEnc(string key, bool decode = false)
public string GetActData(string key)
public string GetDataValue(string key)
public string GetDataValue()
public string[] GetDataValues(string key)
public List<KeyVlu>? GetDataValueList()
public static int ExtractNumber(string inputString)
public static bool IsAnySpecialChar(string s)
public static string RemoveSpecialChar(string s, string replaceWith = "", bool includeSpace = false)
public static string ValidatePassword(string pwd, int minLength = 8, int minUpper = 1, int minLower = 1, int minNumber = 1, bool minSpecial = true)
public string GetMonthText(DateTime dte)
public string GetWeekDay(DateTime dte)
public string CreateWorkBook(DataTable dt, string xfilepath, string xfileowner = "xmlxls")
public string CreateCsv(DataTable dt, string filePath)
public string StrFromNameValueList(List<NameValue> nameValueList, string seperator = ",", string delimiter = ".")
public static string HtmlUrlEncode(string value)
public static string HtmlUrlDecode(string value)
public static string HtmlEncode(string value)
public static string HtmlDecode(string value)
public static string FormattedValue(string value, string originValue = "")
public static double GeoDistanceBetween(double lat1, double lon1, double lat2, double lon2, Units unit = Units.Kilometers)
public enum Units { Kilometers = 0, Miles = 1, NauticalMiles = 2 }
public Type? CallingAssemblyType(string typeName)
public Assembly? DomainAssembly()
public bool IsDomainAssemblyType(string typeName)
public List<Type> AssemblyTypes() => Assembly.GetExecutingAssembly().GetTypes().ToList();
public Type? AssemblyType(string typeName)
public object? TypeInstance(Type objtype, string objName)
public object? TypePropertyGetValue(string classFullName, string propertyName)
public object? TypePropertyGetValue(object obj, string propertyName)
public void TypePropertySetValue(object obj, string propertyName, object value)
public List<string> TypePropertyNames(Type objtype)
public List<string> TypeClasses(string nameSpaceText, bool fullName = false)
public List<string> NestedTypes(string classFullName)
public List<Type> NestedTypes(Type objtype) => objtype.GetNestedTypes().ToList();
public List<string> NestedTypeNames(Type objtype) => objtype.GetNestedTypes().Select(p => p.Name).ToList();
public List<PropAttribute> TypeMethods(Type objtype)
public List<PropAttribute> TypeMethods(string classFullName)
public List<PropAttribute> TypeFields(string classFullName)
public List<PropAttribute> TypeFields(Type objtype)
public List<string> TypeFieldNames(Type objtype)
public List<NameValue> TypeFieldNameValues(Type objtype, object instanceObj)
public FieldInfo? TypeFieldInfo(Type objtype, string fieldName)
public object? TypeFieldGetValue(Type objtype, string fieldName, object instanceObj)
public void TypeFieldSetValue(Type objtype, string fieldName, object instanceObj, object? value)
public bool IsTypeProperty(Type objType, string propertyName)
public bool IsTypeField(Type objType, string fieldName)
public static Type? EnumType(string objTypeName) => Type.GetType(objTypeName);
public static List<NameValue> EnumList(Type objType)
public string EnumString(Type objType, string delimiter = ".", string separator = ",")
public string EnumOptionized(Type objType)
public List<string> EnumNames(Type objType)
public string? EnumName(Type enumType, int enumValue) => Enum.GetName(enumType, enumValue);
public Type CreateDynamicType(string typeName, Dictionary<string, Type> properties)

---
### FileHandler class
public string[]? SearchFiles(string folder, string filePattern)
public List<string>? GetDirectoryFiles(string rootFolder, int opt = 0)
public string CreateZipFromDir(string srcPath, string dstZipPath)
public string WriteByteToFile(string filepath, byte[] filebyte)
public byte[] ReadByteFromFile(string filepath) => File.ReadAllBytes(filepath);
public object? ReadObjectFromFile(string filePath)
public string WriteObjectToFile(string filePath, object obj)
public string DeleteFiles(string srcPath, int daysBefore)
public Dictionary<string, string> GetDirectoryFiles(string path)

---
### ImageHandler class
public int ImageRotated(Bitmap bitmap)
public Bitmap RotateImage(Bitmap bitmap)
public string RotateImage(string srcPath, string targetPath)
public Bitmap ResizeImageH(Bitmap imageIn, int toHeight)
public Bitmap ResizeImageW(Bitmap imageIn, int toWidth)
public Bitmap ResizeImage(Bitmap imageIn, int reducingRate, int maxWidth = -1, int imgRotation = 0, int imgTransparent = 0)
public static string ImageBase64(Bitmap bitmap, int width = 0, int height = 0, int transparent = 0)

---
### XmlTool class
public string[] GetElementValueFromXml(string xmlString, string descendants, string elementKey)
public string RemoveWhiteSpace(string source, string search, string replace)
public string GetXmlString(string xmlString, string descendant)
public int GetXmlTableColumnCount(string xmlString, string beginRow, string endRow)
public string GetXmlTableBindedString(string xmlString, string beginRow, string endRow)
public string GetXmlElementProperty(string xmlString, string descendant, string attributeName)

---
### SQLData class

Data access is the `SQLData` class (namespace `SkyNet`). It reads connection info from
config (`SQLInfo`) — no null concerns.

public SQLData(SQLInfo sqlInfo)
public async Task<(DataTable dt, string emsg)> GetDataAsync(string sSql, List<SqlParameter>? parameters = null)
public async Task<(DataSet ds, string emsg)> GetDataSetAsync(List<string> sSql, List<SqlParameter>? parameters = null)
public async Task<string> PutDataAsync(List<string> sSql, List<SqlParameter>? parameters = null)
public static (DataTable dt, string rlt) SQLDataTable(string sSQL)
public bool DataGetSet(string sql, SQLInfo sqlInfo, out DataSet sqlDataSet, out string eMsg)
public string PutData(List<string> sSql, List<SqlParameter>? parameters = null)

    public class SQLInfo
    {
        private const string ConnTemplate =
            "data source={0};initial catalog={1};integrated security=false;" +
            "persist security info=False;user id={2};password={3};" +
            "packet size=4096;TrustServerCertificate=True;";

        public string DataSource { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int TimeOut { get; set; } = 120;
        public string ConnectionString { get; set; } = string.Empty;

        public SQLInfo()
        {
            WebCore wc = new WebCore();
            DataSource = wc.GetWebEnv(AppConfig.SQLDatabase.Source);
            DatabaseName = wc.GetWebEnv(AppConfig.SQLDatabase.Catalog);
            UserId = wc.GetWebEnv(AppConfig.SQLDatabase.Id);
            Password = wc.GetWebEnv(AppConfig.SQLDatabase.Password);
            TimeOut = (int)Common.Val(wc.GetWebEnv(AppConfig.SQLDatabase.Timeout));
            ResetConnection();
        }

        public SQLInfo(string dataSource, string databaseName,
                       string userId, string password, string timeout)
        {
            DataSource = dataSource;
            DatabaseName = databaseName;
            UserId = userId;
            Password = password;
            TimeOut = (int)Common.Val(timeout);
            ResetConnection();
        }

        public void ResetConnection() =>
            ConnectionString = ConnTemplate.Replace("{0}", DataSource).Replace("{1}", DatabaseName).Replace("{2}", UserId).Replace("{3}", Password);
    }

---
### MathEval class
public string DoMath(string expression)

//'<string1> == <string2>
//    'NOT <string1> == <string2>
//    'EQU : Equal
//    'NEQ : Not equal
//    'LSS : Less than <
//    'LEQ : Less than or Equal <=
//    'GTR : Greater than >
//    'GEQ : Greater than or equal >=

//'Expression examples
//'1.Using IF
//'EQU : Equal
//'NEQ : Not equal
//'LSS : Less than <
//'LEQ : Less than or Equal <=
//'GTR : Greater than >
//'GEQ : Greater than or equal >=

//' if 3 EQU 3, return True, return false
//' if 3 EQU 3, 1 , 3*4/3

//' U can use "==" but can't use > < <> !=


//'2. Using Generic expression
//' 32 * 3 * 2/ 2.15

//'using System;
//'using System.Windows.Forms;
//'using MathExpression;

//'Namespace CSharpEval
//'{
//'    public partial class Form1 : Form
//'    {
//'        private void button1_Click(object sender, EventArgs e)
//'        {
//'            MathExpression.Eval meval=new MathExpression.Eval();
//'            string expression = "if 3==4,10,36*(35+24)/31.36";
//'            //string expression = "32 * 3 * 2/ 2.15";
//'            textBox2.Text = meval.DoMath(expression);
//'        }
//'    }
//'}

---
### Mail class

private MailInfo _mailInfo;
public string[] ToAddr { get; set; } = Array.Empty<string>();
public string Subject { get; set; } = string.Empty;
public string Body { get; set; } = string.Empty;
public string Display { get; set; } = string.Empty;
public const string TempMailBody = "<html><head><meta charset=\"utf-8\" /></head><body>{0}</body></html>";
public Mail() => _mailInfo = new MailInfo();
public Mail(MailInfo mailInfo) => _mailInfo = mailInfo;
public string SendMail()

    public class MailInfo
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string SenderId { get; set; } = string.Empty;
        public string SenderPwd { get; set; } = string.Empty;
        public string SenderAddr { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public int Credential { get; set; } = 0;

        public MailInfo()
        {
            WebCore wc = new WebCore();
            Server = wc.GetWebEnv(AppConfig.Mail.Server);
            SenderAddr = wc.GetWebEnv(AppConfig.Mail.Addr);
            SenderId = wc.GetWebEnv(AppConfig.Mail.Id);
            SenderPwd = wc.GetWebEnv(AppConfig.Mail.Password);
            Display = wc.GetWebEnv(AppConfig.Mail.Title);

            int.TryParse(wc.GetWebEnv(AppConfig.Mail.Port), out int port);
            int.TryParse(wc.GetWebEnv(AppConfig.Mail.Credentials), out int cred);
            Port = port;
            Credential = cred;
        }

        public MailInfo(string server, int port, string senderAddr,
                        string userId, string password, string title, int credentials = 0)
    }

---
### JsonHandler class

public static string DataTableToJson(DataTable dt)
public static string DataTableToJsonTree(
            DataTable dt,
            string idCol,
            string parentCol,
            string rootVal = "",
            string childKey = "children")
private static void CleanEmpty(List<Dictionary<string, object?>> nodes, string childKey)
public static DataTable JsonToDataTable(string json)

---
### FileDownload class

private readonly string _flname = string.Empty;
private readonly string _flpath = string.Empty;
private readonly string _fltype = string.Empty;
        public FileDownload()
        {
            string fileParam = QueryValue(Streaming.ParamKey, true);
            if (string.IsNullOrEmpty(fileParam)) return;

            string fldate = GetKeyValueEnc(fileParam, Streaming.Params.datetime);
            _flname = GetKeyValueEnc(fileParam, Streaming.Params.Name);
            _flpath = GetKeyValueEnc(fileParam, Streaming.Params.Path);

            if (DateTime.TryParse(fldate, out DateTime parsedDate))
            {
                if ((DateTime.Now - parsedDate).TotalMinutes > 1 || string.IsNullOrEmpty(_flpath))
                    throw new Exception(WebEnv.Errors.Forbidden.ToString());
            }

            string ext = Path.GetExtension(_flpath).ToLower();
            _fltype = (ext == WebEnv.FileExts.doc || ext == WebEnv.FileExts.docx)
                ? WebEnv.Contents.doc
                : WebEnv.Contents.stream;
        }
public async Task DownloadStream()

---
###  DynamicModel(: DynamicObject) class

public Dictionary<string, object> data { get; set; } = new();
public int Count => data.Count;
public override bool TryGetMember(GetMemberBinder binder, out object? result)
public override bool TrySetMember(SetMemberBinder binder, object? value)

---
### Encrypt class

public Encrypt(string key)
public string EncryptData(string plaintext)
public string DecryptData(string encryptedtext)

---
### HtmlHandler class

public List<HtmlElement> HtmlElements { get; set; } = new();
public bool IsElementId { get; set; } = false;
public HtmlHandler(bool includeElementId = false)
public string HtmlText()

public class HtmlElement
public class HtmlTag

---
### HtmlTag class [Serializable]

public enum Types { Regular = 0, Empty = 1 }
private const string TagRegular = "<[tag][0][1]>[2]</[tag]>[3]";
private const string TagEmpty = "<[tag][0][1]/>[2]";
private Types TagTemplate { get; set; } = Types.Regular;
private string _TagName { get; set; } = HtmlTags.div;
public string Tag => _TagName;
public string IDTag { get; set; } = string.Empty;
public string InnerText { get; set; } = string.Empty;
public List<NameValue> Attributes { get; set; } = new();
public List<NameValue> Styles { get; set; } = new();
public string Css { get; set; } = string.Empty;
public virtual string HtmlText => Html();
public bool IsAttribute(string name) => (Attributes ??= new()).Any(x => x.name == name.Trim());
public string GetAttribute(string name) => Attributes?.Find(x => x.name == name)?.value ?? string.Empty;
public void RemoveAttribute(string name) => Attributes?.RemoveAll(x => x.name == name);
public void ClearAttributes() => Attributes?.Clear();

public HtmlTag(string tagName, Types tagType = Types.Regular)
public HtmlTag(string tagName, string tagAttributes, string tagStyles, Types tagType = Types.Regular)
public void SetTag(string tagName, Types tagType = Types.Regular)
public void SetTagType(Types tagType) => TagTemplate = tagType;
public void SetId(string id = "")
public void SetAttribute(string name, string value)
public void SetAttributes(List<NameValue> nameValues)
public void SetAttributes(string attributeText)
public string GetAttributes()
public void SetStyle(string name, string value)
public void SetStyles(List<NameValue> nameValues)
public void SetStyles(string styleText)
public string GetStyles()
public void ClearAll()
public virtual string Html()

---
### ToolKit

public enum Alignments { Horizontal = 0, Vertical = 1, Reverse = 2 }
public enum HorizontalAligns { Left = 0, Center = 1, Right = 2 }
public enum Flows { Left = 0, Right = 1 }
public enum TextTypes
    {
        datetimelocal = 0, image = 1, search = 2, time = 3, url = 4,
        week = 5, range = 6, tel = 7, number = 8, email = 9, color = 10,
        month = 11, @date = 12, text = 13, password = 14
    }
- usage
    [Serializable]
    public class LinePitch : HtmlTag
    {
        public override string HtmlText => Html();

        public LinePitch(int lineHeight = 0)
        {
            SetStyle(HtmlStyles.display, "block");
            SetStyle(HtmlStyles.height, lineHeight + "px");
        }
    }

    [Serializable]
    public class Label
    {
        public string IDTag { get; set; } = string.Empty;
        public HtmlTag Wrap { get; set; } = new HtmlTag();

        public string HtmlText => Wrap.Html();

        public Label() => InitStyle();
        public Label(string labelText) { InitStyle(); Wrap.InnerText = labelText; }

        private void InitStyle()
        {
            Wrap.SetAttribute(HtmlAttributes.data_controlname, "Label.Wrap");
            Wrap.SetStyle(HtmlStyles.overflow, "hidden");
        }
    }

AI is best frontend writer. so probably not but, if you need tool's html creator in serverside, let me know
- public class ToolKit.Button 
- public class ToolKit.Buttons 
- public class ToolKit.CheckBox 
- public class ToolKit.ContentsBox 
- public class ToolKit.DataGrid 
- public class ToolKit.DataGrid.Container 
- public class ToolKit.DataGrid.TableColumn 
- public class ToolKit.DataGrid.TableRow 
- public class ToolKit.DataGrid.TD 
- public class ToolKit.DataGrid.TH 
- public class ToolKit.DataGrid.TR 
- public class ToolKit.DataList 
- public class ToolKit.DialogBox 
- public class ToolKit.Dropdown 
- public class ToolKit.FileUpload 
- public class ToolKit.Grid 
- public class ToolKit.Grid.Column 
- public class ToolKit.Grid.Row 
- public class ToolKit.Grid.TD 
- public class ToolKit.Grid.TH 
- public class ToolKit.Grid.TR 
- public class ToolKit.Hidden 
- public class ToolKit.HtmlContentsBox 
- public class ToolKit.HtmlElementBox 
- public class ToolKit.HtmlWrapper 
- public class ToolKit.iFrame 
- public class ToolKit.ImageBox 
- public class ToolKit.ImageButton 
- public class ToolKit.ItemList 
- public class ToolKit.ItemList.Item 
- public class ToolKit.ItemPanel 
- public class ToolKit.Label 
- public class ToolKit.LinePitch 
- public class ToolKit.MenuIcon 
- public class ToolKit.MenuList 
- public class ToolKit.MenuPanel 
- public class ToolKit.MenuPanel.Column 
- public class ToolKit.MenuPanel.Column.Item 
- public class ToolKit.Paging 
- public class ToolKit.Parallax 
- public class ToolKit.Parallax.Section 
- public class ToolKit.Progress 
- public class ToolKit.Progress.Item 
- public class ToolKit.Radio 
- public class ToolKit.Stacker 
- public class ToolKit.Stacker.Column 
- public class ToolKit.TabStrip 
- public class ToolKit.TextArea 
- public class ToolKit.Texts 
- public class ToolKit.Timer 
- public class ToolKit.TreeView 
- public class ToolKit.TreeView.TreeItem 
- public class ToolKit.TreeView2 
- public class ToolKit.TreeView2.TreeItem 
- public class ToolKit.Wrap 
- public class ToolKit.Alignments 
- public class ToolKit.Button.ButtonTypes 
- public class ToolKit.DataGrid.ContainerType 
- public class ToolKit.Flows 
- public class ToolKit.HorizontalAligns 
- public class ToolKit.MenuIcon.Types 
- public class ToolKit.TextTypes 
- public class ToolKit.TreeView.ItemStatus

usage:
public class MyPage : WebPage
{
    public override void OnInitialized()
    {
        HtmlTag Hello = new HtmlTag(HtmlTags.h3);
        Hello.InnerText = "Hello World3";

	HtmlDoc.HtmlBodyText = Hello.HtmlText();
    }
}

---


# Generic Web Appication Development Architecture : NOT Part of SkyNet, Web Application created based on this architecture.
It can be changed depens on project size/requirement

## 1. Page anatomy — self-contained, four files

Every page is four co-located files sharing a base name (e.g. `Report_Financial`):

| File            | Holds                                                             |
|-----------------|------------------------------------------------------------------|
| `Name.cs`       | The page class (`: WebBase`), its DTOs, and its server methods   |
| `Name.js`       | One IIFE namespace `NameJs = (function(){ ... })()` + `reveal()` |
| `Name.css`      | Styles, every selector under a **short page-unique prefix**      |
| `Name.html`     | Markup with `{placeholder}` tokens the `.cs` replaces            |

Rules that hold across the framework:

- **Self-contained, duplicate-don't-abstract.** A page owns its own CSS prefix, its
  own JS IIFE, and its own C# methods/DTOs. Prefer copying a block into a second page
  over introducing a shared helper. The **only** cross-page C# sharing is `*Model.cs`
  files (plain DTO/enum holders in a `Models` namespace).
- **Prefix discipline.** CSS classes and ids are prefixed per page (`finrp-`, `cfgms-`,
  …) so pages never collide when several are open at once.
- **JS namespace.** The page script is a single IIFE assigned to `NameJs`, exposing a
  small surface (typically `reveal` plus event handlers) via its return object; it
  ends by calling `NameJs.reveal()`.
- **Placeholders.** The `.cs` builds HTML fragments and injects them with
  `HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText.Replace("{token}", built)`.
- Assets are served from **root folders** (`scripts/`, `styles/`, `images/`,
  `photos/`, `htmls/`), not `wwwroot`.

Standard error surfacing inside a method:

```csharp
var (dt, emsg) = await Helper.GetDataTable(sql, parms);
if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }
```

## 2. Configuration & hosting

- **`application.cfg`** (under the app-config folder) is **pipe-delimited**:
  `app.app.name | ServiceNet`, one `key | value` per line, with a `System` and a
  `Custom` section. Read any value with `Helper.GetWebEnv("app.sqldb.catalog")` /
  `GetWebEnv("user.sso.uri")`. Never expose this folder over HTTP.
- **Run mode** is a separate switch stored in the DB (`XYSOPTION`, `ORIGINE/RunMode`),
  not the `.cfg` value — `Helper.AppRunMode()` reads it; Archived (2) makes the whole
  app read-only via `Helper.PutData`.
- **Hosting:** in-process ASP.NET Core Module (IIS), static assets served from the
  **content-root folders**. Because the whole content root is served, harden
  `web.config` `requestFiltering`: `hiddenSegments` for server-internal folders
  (config, htmls, data, logs) **plus** a `fileExtensions` deny-list
  (`.config .json .dll .exe .pdb .cfg`) so root files (appsettings, assemblies) can't
  be downloaded. Never block `.js`/`.css`/image extensions — the client fetches those
  by URL and blocking them renders pages unstyled.

---


---

## 3. Server — `WebBase` and `Helper`

**`WebBase : WebPage`** is the page base class.

- `OnInit` authenticates via `Helper.GetAuthData()` (a role-less / unauthenticated
  caller is stopped here).
- `OnInitialized` is where a page loads its data, builds fragments, and injects them
  into `HtmlDoc.HtmlBodyText`.
- Google translation is handled centrally in `OnAfterRender` + `OnResponse`, targeting
  `SetElementContents` and server page-method actions.

**`Helper`** — the server-side utility surface. Commonly used:

- `Helper.GetAuthData()` — current user / session.
- `Helper.GetDataTable(sql, parms)` — parameterized read → `(DataTable, string emsg)`.
- `Helper.GetTabActions(parms)` — the current user's **granted** tab-actions (see §9).
- `Helper.PutData(...)` — the standard write path; **hard-blocks all writes when the
  app run mode is Archived (2)**, returning a "locked in archive mode" message.
- `Helper.AppRunMode()` — the current run mode as an int (0 Initialize, 1 Normal,
  2 Archived).
- `Helper.GetWebEnv("area.key")` — read a config value (see §10).
- `Helper.IssueFound`, `Helper.ErrContentHtml(text)` — standard error modal pieces.
- CSV/download helpers (`CreateCsv`, `TempFolder`, `DownLoadFileLink`) used by export
  methods together with `response.DownloadFile`.

---

## 8. CS → JS data transfer

Preferred, current pattern for handing structured data to a page's JS:

- **Server:** `response.SetVariableData<T>(name, obj)` — serializes `obj` and ships it.
- **Client:** `var v = $GetVarData(name)` — retrieves it. Under the hood
  `$SetVarData(name, b64)` does `JSON.parse(decodeURIComponent(escape(atob(b64))))`
  into `window.$VarData`; `$GetVarData(name, remove=true)` returns the value and
  **deletes it by default** (pass `false` to keep it).

Legacy pages that embed a JSON blob inline in the HTML are **left in place** — do not
retrofit them to the variable-data mechanism.

---

## 9. Permissions & navigation (XFN + XYS* tables)

Method-level authorization runs through the SQL function
**`XFN_UserMethodPermission(@userid, @type, @method)`** on every page-method call, in
the native/IIS layer before the managed handler:

- A method that is **not registered** as any action's method is **permissive**
  (callable by anyone with page access).
- A method that **is registered** requires the caller to hold **all three grants**
  (role→page, role→tab, role→tab-action).
- A user with **no role** is blocked entirely.

**Navigation / action tables (`XYS*`):**

- `XYSPAGE` — `PAGE_ID, PAGE_AREA, PAGE_SORT, …`
- `XYSPAGETAB` — `TAB_ID, PAGE_ID, TAB_NAME, …`
- `XYSTABACTION` — `TABACTION_ID, TAB_ID, TABACTION_NAME, TABACTION_LABEL,
  TABACTION_METHOD, SYSDTE, SYSUSR`
- `XYSROLE` — `ROLE_ID, ROLE_NAME`
- `XYSROLEPAGE`, `XYSROLETAB`, `XYSROLETABACTION` — the three grant tables
  (`XYSROLETABACTION` = `ROLETABACTION_ID, ROLE_ID, TABACTION_ID, SYSDTE, SYSUSR`)

**Two conventions that are easy to get wrong:**

1. **Action-name composition.** `Helper.GetTabActions` returns each action's
   `TABACTION` as the **dotted path** `PAGE_AREA + '.' + TAB_NAME + '.' +
   TABACTION_NAME`. So a permission check compares
   `x.TABACTION == "Area.Tab.ActionName"`, while the stored `TABACTION_NAME` is just
   the **short** `ActionName`. (Compose permission keys as
   `PAGE_AREA.TAB_NAME.TABACTION_NAME`; store only the short name in the row.)
2. **Method matching is a comma-joined LIKE.** `TABACTION_METHOD` may hold **several**
   method names joined by commas, and `XFN_UserMethodPermission` matches `@method`
   against that list. The idiom: a data/view method (`GetX`) gets its **own** action,
   but all **`DownloadX` / export methods are pooled under a single `Export` action**
   whose `TABACTION_METHOD` is the comma-joined list of every download method. When you
   add a new export, **append its method to the `Export` action's list** — registering
   only the `Get` action leaves the download unauthorized (or, on a permissive setup,
   inconsistently gated).

**Registration SQL is idempotent:** look up `PAGE_ID` by `PAGE_AREA`, `TAB_ID` by
`TAB_NAME`, then `INSERT ... WHERE NOT EXISTS`; grant by inserting `XYSROLETABACTION`
rows (commonly copied from an existing peer action so the same roles see the new one).

**Page-side gate for exports:** a download method typically also checks
`HasAction("Area.Tab.Export")` in code before streaming.

---


## Quick gotchas

- Server pushes DOM ops; it does **not** return pages. Think in `ApiResponse` builder
  calls, not HTML responses.
- After a server mutation, re-run page JS with `ExecuteScript("NameJs.xxx();")`.
- Minifying `skynet.core.js`: preserve every function name and global — the engine
  reads function names at runtime and HTML calls `$`-globals by name.
- New export? Register the `Get` action **and** add the download method to the shared
  `Export` action's comma-joined `TABACTION_METHOD`.
- `TABACTION_NAME` stores the short name; permission keys are the dotted
  `PAGE_AREA.TAB_NAME.TABACTION_NAME`.
- Writes are blocked in Archived mode via `Helper.PutData`; go through
  `new SQLData().PutDataAsync(...)` only when a write must bypass that.
