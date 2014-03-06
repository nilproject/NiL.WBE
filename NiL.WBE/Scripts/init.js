mapRoute("/styles/style.css", function (request, response, app) {
    response.TransmitFile(validateFileName("/styles/style.css"));
    app.CompleteRequest();
});