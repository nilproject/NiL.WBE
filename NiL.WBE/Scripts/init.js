mapRoute("/styles/style.css", function (request, response, app) {
    response.ContentType = "text/css";
    response.TransmitFile(validateFileName("/styles/style.css"));
    app.CompleteRequest();
});