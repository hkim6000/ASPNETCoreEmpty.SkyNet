//////////////////////////////////////////////////////////
//  prerequisite : installed - view terminal & Edit project file
//  - dotnet add package Microsoft.Data.SqlClient
//  - dotnet add package System.Drawing.Common
//  - Add option to Properties/launchsetting.json file  : "hotReloadEnabled": false
//////////////////////////////////////////////////////////

using SkyNet;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////////////////////
builder.Services.AddHttpContextAccessor();    //1.Add HttpContext Service
//////////////////////////////////////////////////////////

var app = builder.Build();

//////////////////////////////////////////////////////////
app.UseMiddleware<IHandler>();  //2. use SKYNET.IHANDLER as middleware service
app.UseStaticHttpCurrent();    //3. use static http class service
//////////////////////////////////////////////////////////

app.Run();

