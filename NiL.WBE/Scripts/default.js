

var document = loadTemplate("Default");

var test = NiL.WBE.Html.HtmlElement("a");
test.Attributes.set_Item("href", "http://vk.com/");
test.Subnodes.Add(NiL.WBE.Html.Text("vk"));
document.getElementById("test").innerHTML = test.toString();

response.Write("<!DOCTYPE html>");
response.Write(document.documentElement.outerHTML);
application.CompleteRequest();