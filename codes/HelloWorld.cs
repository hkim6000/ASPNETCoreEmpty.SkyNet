using SkyNet;

public class HelloWorld : WebPage
{
    public HelloWorld() { }

    public override async Task OnInitialized() {
        string techRef = @"Tech_Document() => https://www.theskylite.com/ => https://github.com/hkim6000/SkyNet.AspNetCore.EmptyTemplate";

        HtmlTag Hello = new HtmlTag(HtmlTags.h4);
        Hello.InnerText = "Hello World<br><br>" + techRef;
        HtmlDoc.HtmlBodyText = Hello.HtmlText;
    }
    public override Task<string> OnInit(string type, string func)
    {
        return base.OnInit(type, func);
    }
    
    public override async Task OnBeforeRender()
    {
    }
    public override async Task OnAfterRender()
    {
    }
    public override Task<ApiResponse?> OnRequest(string type = "", string method = "")
    {
        return base.OnRequest(type, method);
    }
    public override Task<ApiResponse> OnResponse(ApiResponse apiResponse)
    {
        return base.OnResponse(apiResponse);
    }
}