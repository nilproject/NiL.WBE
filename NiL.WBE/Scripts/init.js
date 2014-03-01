mapRoute("/test", function (request, response, app) {
    response.Write("it's work!");
    app.CompleteRequest();
});
mapRoute("/test2", "Scripts/test.js");