

try
{
    var temp = loadTemplate("Default");
    response.Write(temp.toString());
}
catch(e)
{
    response.Write(e.toString());
}
response.Flush();
application.CompleteRequest();