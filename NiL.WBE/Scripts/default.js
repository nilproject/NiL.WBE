

try
{
    var temp = loadTemplate("Default");
    temp.GetSubElementBy("id", "header-content")
        .GetSubElementBy("id", "home")
        .Attributes.Add("onclick", "document.location.href='/ipsum',false");
    if (request.Path == "/ipsum")
    {
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "text")
            .Subnodes.Add(NiL.WBE.Html.Text(ipsum));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "avatar-holder")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c608630/v608630405/2f2b/WEVMUiIpHik.jpg"));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "autor")
            .Subnodes.Add(NiL.WBE.Html.Text("Lorem Ipsum"));
    }
    response.Write(temp.toString());
}
catch(e)
{
    response.Write(e.toString());
}
response.Flush();
application.CompleteRequest();