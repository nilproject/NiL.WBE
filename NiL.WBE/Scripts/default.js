

try
{
    var temp = loadTemplate("Default");
    temp.GetSubElementsBy("id", "test")[0].Subnodes.Add(NiL.WBE.Html.Text("Hello from script"));
    response.Write(temp.toString());
}
catch(e)
{
    response.Write(e.toString());
}
response.Flush();
application.CompleteRequest();