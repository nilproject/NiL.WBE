mapRoute("/error", function (request, response, app) {
    response.Write(NiL.WBE.Http.ErrorPage(System.Int32.Parse(request.QueryString.ToString()), request.QueryString.ToString()).ToString());
    app.CompleteRequest();
});