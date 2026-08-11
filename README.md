<h2>SkyNet Framework Visual Studio Empty Template</h2>

<h3>Modern ASP.NET Core Implementation | .NET 10</h3>

- SKYNET framework (C# only)<br>
Platform: ASP.NET Core / .NET 10<br>
Architecture: Middleware-based <br>

- Technical Documents : https://www.theskylite.com/document<br>
- Showcase Demo. Site: https://www.theskylite.com/SkyLiteProject<br>
- Download GitHub: https://github.com/hkim6000/SkyNetDemo-AspNetCore<br><br>

<h4>unzip "ASPNETCoreEmpty.zip" and execute solution file(ASPNETCoreEmpty.slnx) to build your project from scratch<br></h4>

<h3>Project Structure</h3><br>
ASPNETCoreWeb/<br>
├── Codes/              # web page classes (C#)<br>
│   ├── XysUser         # no routing rule, free webpage class location<br>
│   ├── XysUserMV       # web page class can be anywhere in the project folder<br>
│   ├── XysUserEV       <br>
│   ├── XysRole         <br>
│   ├── XysPermission   <br>
│   └── ...<br>
├── bin\Debug\net10.0<br>
│      └── <b>SkyNet.dll ++ # ⭐ SKYNET framework file(single dependency)</b><br>
├── appConfig/<br>
│   └── application.cfg    # Application configuration<br>
├── data/                  # Data storage folder<br>
├── htmls/                 # HTML email templates<br>
├── images/                # Static images<br>
├── logs/                  # Application logs<br>
├── scripts/               # JavaScript files<br>
│   ├── Home.js<br>
│   └── WebScript.js<br>
├── styles/                # CSS stylesheets<br>
│   ├── Home.css<br>
│   └── WebStyle.css<br>
├── temp/                  # Temporary files<br>
├── Program.cs             # ASP.NET Core startup<br><br>

 ----------------------------------------------------------------------------------------<br>

<h3>Getting Started for Your Own Asp.Net Core Project</h3><br>
//////////////////////////////////////////////////////////<br>
<b>1.</b> Create a empty web project<br>
<b>2.</b> Add Project Reference : <b>SKYNET.dll</b><br>
(the SKYNET.dll file is in the bin\Debug\net10.0 foler in this demo project)<br>
//////////////////////////////////////////////////////////<br>
Prerequisite : install thru menu-view-terminal in Visual Studio<br>
<b>3.</b> Excute command in the terminal:<br>
      &nbsp;&nbsp;&nbsp;(<b>dotnet add package Microsoft.Data.SqlClient</b>)<br><br>
<b>4.</b> Excute command in the terminal:<br>
      &nbsp;&nbsp;&nbsp;(<b>dotnet add package System.Drawing.Common</b>)<br><br>
<b>5.</b> Add option to Properties/launchsetting.json file  : <b>"hotReloadEnabled":false</b><br>
("hotReloadEnabled=true" could interrupt page display while development)<br><br>
<b>6.</b> Add some folders in "Edit Project File" menu<br>
├── appConfig/<br>
├── data/                  # Data storage folder<br>
├── htmls/                 # HTML email templates<br>
├── images/                # Static images<br>
├── logs/                  # Application logs<br>
├── scripts/               # JavaScript files<br>
├── styles/                # CSS stylesheets<br>
├── temp/                  # Temporary files<br>
 
//////////////////////////////////////////////////////////<br><br>

<b> ⭐ 7. program.cs for Asp.Net Core</b><br>
<br>
------------------------------------------------------------------------------<br>
using SkyNet;<br>
<br>
var builder = WebApplication.CreateBuilder(args);<br>
<br>
</b>

//////////////////////////////////////////////////////////<br>
<b>builder.Services.AddHttpContextAccessor();  // 1. Add HttpContext Service</b><br>
//////////////////////////////////////////////////////////<br>

<b>var app = builder.Build();</b><br>

//////////////////////////////////////////////////////////<br>
<b>app.UseMiddleware<IHandler>();  // 2. use SKYNET.IHANDLER as middleware service</b><br>
<b>app.UseStaticHttpCurrent();     // 3. use static http class service</b><br> 
//////////////////////////////////////////////////////////<br>
<br>
<b>app.Run();</b>b><br>
------------------------------------------------------------------------------<br>
<br>

<h3>Framework Philosophy</h3>
<b>Server-Centric, AJAX-Driven</b>br>
<b>SkyNet embraces a server-centric architecture where:</b><br>
•	Business logic stays on the server (secure, maintainable)<br>
•	Client makes lightweight AJAX calls via $ApiRequest<br>
•	Server responds with ApiResponse commands (Navigate, SetElementContents, PopUpWindow, etc.)<br>
•	Result: 80% less JavaScript, 100% C# for most logic<br>
Less JavaScript, More C#<br>
•	Write your application logic in C#<br>
•	Use JavaScript only for UI interactions and DOM manipulation<br>
•	Framework handles the communication layer<br>
•	Stay in your comfort zone as a .NET developer<br>
Rapid Development<br>
•	Pre-built enterprise components (auth, permissions)<br>
•	Minimal boilerplate code<br>
•	Consistent patterns across entire application<br>
•	From idea to production in days, not months<br><br>

 
<h3>Technology Stack</h3>
•	Framework: ASP.NET Core (.NET 10)<br>
•	Middleware: Custom SkyNet IHandler<br>
•	Frontend: HTML5, CSS3, Minimal JavaScript<br>
•	Authentication: Customizable, Cookie-based with encrypted AppKey in Showcase version<br>
•	Deployment: IIS, Kestrel, Docker-ready<br>
  
<h3>Use Cases</h3>
<b>SkyNet is perfectly suited for:</b><br>
•	✅ Enterprise internal portals<br>
•	✅ Business management systems (ERP, CRM)<br>
•	✅ Admin dashboards and back-office applications<br>
•	✅ Line-of-business (LOB) applications<br>
•	✅ Data-heavy CRUD applications<br>
•	✅ Multi-tenant SaaS platforms<br>
•	✅ Applications requiring strong RBAC<br>
•	✅ Multi-language enterprise applications<br><br>

<h3>Conclusion</h3><br>
The SkyNetDemo project is a masterclass in building a secure, scalable, and maintainable web application with the SkyNet framework on modern ASP.NET Core. Its architecture is perfectly suited for complex business applications like ERPs, CRMs, or internal admin portals where data integrity, role-based security, and rapid development of standardized forms are paramount.<br>
SkyNet brings the proven patterns of SKYLITE to the modern .NET ecosystem, providing a clear migration path for legacy applications while enabling new projects to benefit from cross-platform, cloud-ready ASP.NET Core.<br>
