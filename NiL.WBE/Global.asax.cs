using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace NiL.WBE
{
    public class Global : System.Web.HttpApplication
    {
        private static string rootDirectory;
        public static string RootDirectory { get { return rootDirectory; } }

        private Logic.LogicProvider _logic;
        private Logic.LogicProvider logic { get { return _logic ?? (_logic = new Logic.JavaScriptLogic()); } }

        void Application_Start(object sender, EventArgs e)
        {
            rootDirectory = Server.MapPath("~/");
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                logic.Process(Request, Response, this);
            }
            catch (Exception ex)
            {
                Server.ClearError();
                Response.Clear();
                var page = new Http.ErrorPage(Http.ResponseCode.INTERNAL_SERVER_ERROR, "Internal error. (" + ex.Message + ")");
                Response.BinaryWrite(System.Text.Encoding.Default.GetBytes(page.ToString()));
                Response.StatusCode = (int)Http.ResponseCode.INTERNAL_SERVER_ERROR;
                Response.Flush();
                CompleteRequest();
            }
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            Server.ClearError();
        }
    }
}
