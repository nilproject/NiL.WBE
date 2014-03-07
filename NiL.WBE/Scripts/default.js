

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
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "date")
            .Subnodes.Add(NiL.WBE.Html.Text(Date().substring(0, Date().indexOf(" GMT"))));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "photos")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c413428/v413428111/a1f0/pBssLkiiO6c.jpg"));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "photos")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c413428/v413428818/71e2/gW3JphItD80.jpg"));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "photos")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c607127/v607127929/160b/knIPiQ-dGuI.jpg"));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "photos")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c607125/v607125333/1387/RJbio33uISs.jpg"));
        temp.GetSubElementBy("class", "post")
            .GetSubElementBy("class", "photos")
            .Subnodes.Add(NiL.WBE.Html.Img("https://pp.vk.me/c313322/v313322492/9a7c/ZMJUdAibwYE.jpg"));
    }
    response.Write(temp.toString());
}
catch(e)
{
    response.Write(e.toString());
}
response.Flush();
application.CompleteRequest();