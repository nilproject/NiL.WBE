using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Logic
{
    public class DummyLogic : LogicProvider
    {
        public DummyLogic()
        {

        }

        public override string Process(HTTP.HttpServer server, HTTP.HttpPack pack)
        {
            var res = new HTTP.HttpPack(new HTML.HtmlPage()
            { 
                new HTML.Text("OS: " + Environment.OSVersion), new HTML.Br(),
                new HTML.Text("Server time: " + DateTime.Now)
            }.ToString());
            res.Fields.Add("Content-type", HTML.HtmlPage.ContentType);
            return res.ToString(HTTP.ResponseCode.OK);
        }
    }
}
