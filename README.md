<h2>SkyNet Framework - Visual Studio Project Template</h2>

<h3>Modern ASP.NET Core Implementation | .NET 10</h3>

- SKYNET framework (C# only)<br>
Platform: ASP.NET Core / .NET 10<br>
Architecture: Middleware-based <br>

GitHub: https://github.com/hkim6000/ASPNETCoreEmpty.SkyNet<br>
YouTube: https://www.youtube.com/@hckim3948<br>
Developer Guide: https://www.theskylite.com/documents/SkyNet_Developer_Guide.html<br><br>

<h3>Project Structure</h3><br>
Project Root/<br>
├── Codes/              # web page classes (C#)<br>
│   ├── Login.cs       # example filename<br>
│   ├── Reset.cs       # example<br>
│   ├── Configuration / # example code folder      <br>
│       └── Configuration.cs  <br>
│       └── Configuration_Users.cs  <br>
│       └── Models <br>
│           └── ConfigurationModel.cs <br>
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
<b>1.</b> In Visual Studio, create a empty Asp.Net.core project<br>
<b>2.</b> Install SkyNet Reference <br>
In NuGet Package Console <br>
```<br>
Install-Package TheSkyLite.SkyNet<br>
or<br>
dotnet add package TheSkyLite.SkyNet<br>
<br>
<b>3.</b>If it needed, Install other packages<br>
      dotnet add package Microsoft.Data.SqlClient: for MS-Sql server<br>
      dotnet add package System.Drawing.Common<br>
<br>
<b>4.</b> Add option to Properties/launchsetting.json file  : <b>"hotReloadEnabled":false</b><br>
("hotReloadEnabled=true" could interrupt page display while development)<br><br>

<b> ⭐ 5. program.cs for Asp.Net Core</b><br>
<br>
------------------------------------------------------------------------------<br>
using SkyNet;<br>
<br>
var builder = WebApplication.CreateBuilder(args);<br>
<br>
</b>
<b>builder.Services.AddHttpContextAccessor(); // 1. Add HttpContext Service</b><br>
<b>var app = builder.Build();</b><br>
<b>app.UseMiddleware<IHandler>();  // 2. use SKYNET.IHANDLER as middleware service</b><br>
<b>app.UseStaticHttpCurrent();     // 3. use static http class service</b><br> 
<b>app.Run();</b><br>
------------------------------------------------------------------------------<br>
<br>
<br>
<b> ⭐ 6. Edit Project File (yourproject.proj)</b><br>
------------------------------------------------------------------------------<br>
<Project Sdk="Microsoft.NET.Sdk.Web"><br>
  <PropertyGroup><br>
    <TargetFramework>net10.0</TargetFramework><br>
    <Nullable>enable</Nullable><br>
    <ImplicitUsings>enable</ImplicitUsings><br>
  </PropertyGroup><br>
  <ItemGroup><br>
    <PackageReference Include="TheSkyLite.SkyNet" Version="1.0.5" /><br>
  </ItemGroup><br>
  <ItemGroup><br>
    <Content Include="appConfig\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="htmls\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="images\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="logs\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="scripts\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="styles\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="temp\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
    <Content Include="data\**"><br>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory><br>
      <CopyToPublishDirectory>Always</CopyToPublishDirectory><br>
    </Content><br>
  </ItemGroup><br>
</Project><br>
//////////////////////////////////////////////////////////<br><br>

<h3>AI Vibe-Coding Showcases </h3>
1. ServiceNet - AI powered (Claude Opus 4.8) <br>
Demo.Website Link: https://www.theskylite.com/ServiceNet <br>
Online User's Manual: https://www.theskylite.com/servicenet.html <br>
YouTube: https://www.youtube.com/watch?v=0hEywq6Om2o<br>
<br>
2. BizJournal - AI powered (Claude Opus 4.8)<br>
Demo.Website Link: https://www.theskylite.com/BizJournal<br>
Online User's Manual: https://www.theskylite.com/BizJournal_User_Manual.html<br>
YouTube: https://www.youtube.com/watch?v=IVFX0slGTAs<br>
<br>
<br>

<h3>Framework Philosophy</h3>
<b>With full AI Supporting, Server-Centric, No FrontEnd Javascript Framework</b><br>
<b>SkyNet embraces a server-centric architecture where:</b><br>
•	Minimizing Errors in AI-Assisted Coding<br>
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
SkyNet brings the proven patterns of SKYLITE to the modern .NET ecosystem, providing a clear migration path for legacy applications while enabling new projects to benefit from cross-platform, cloud-ready ASP.NET Core.<br><br><br>

© 2026 The SkyLite, HC Kim. All rights reserved.
SkyNet Framework is proprietary software, free to use under the terms in [LICENSE.txt](LICENSE.txt).

