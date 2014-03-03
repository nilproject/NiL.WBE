

try
{
    temp = loadTemplate("Default");
    response.Write("<!DOCTYPE html>");
    var test = NiL.WBE.Html.HtmlElement("a");
    test.Attributes.set_Item("href", "http://vk.com/");
    test.Subnodes.Add(NiL.WBE.Html.Text("vk"));
    response.Write(test.ToString());
}
catch(e)
{
    response.Write(e.toString());
}
response.Flush();
application.CompleteRequest();