using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.WBE.HTML;
using System.Net.Sockets;

namespace NiL.WBE.Logic
{
    public class DummyLogic : LogicProvider
    {
        public DummyLogic()
        {

        }

        private string sendResource(string id)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(id);
            byte[] buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);
            stream.Close();
            var res = new HTTP.HttpPack(Encoding.Default.GetString(buf));
            res.ContentType = "application/octet-stream";
            return res.ToString(HTTP.ResponseCode.OK);
        }

        public override string Process(HTTP.HttpServer server, HTTP.HttpPack pack, Socket client)
        {
            if (pack.Path == "/resource")
                return sendResource(pack.Request);
            if (pack.Path == "/error")
            {
                int errorCode = 400;
                int.TryParse(pack.Request, out errorCode);
                if (errorCode != 404)
                    return new HTTP.ErrorPage((HTTP.ResponseCode)errorCode, ((HTTP.ResponseCode)errorCode).ToString().Replace('_', ' ')).ToString();
            }
            if (pack.Path != "/" && pack.Path != "")
                return new HTTP.ErrorPage(HTTP.ResponseCode.NOT_FOUND, "Oops!").ToString();
            int visitCount = 0;
            var t = pack.Cookies["visitcount"];
            if (t != null)
                int.TryParse(t.Value, out visitCount);
            visitCount++;
            var page = new HtmlPage()
            { 
                new HtmlElement("div", "content")
                {
                    new HtmlElement("div", "toptext")
                    {
                        new Text("if you see it, then")
                    },
                    new HtmlElement("div", "title")
                    {
                        new Text("NiL.WBE")
                    },                
                    new HtmlElement("div", "bottomtext")
                    {
                        new Text("working")
                    },
                    new Text("you was here " + visitCount + " times")
                }
            };
            page.Head.Add(new HtmlElement("style")
            {
                new Text(
@"
    html {
        height: 100%;
    }
    * {
        text-align: center;
        font-family: Lobster;
    }
    body {
        height: 100%;
    }
    #content {
        position: relative;
        top: 25%;
    }
    #title {
        font-size: 80px
    }
    @font-face {
        font-family: 'Lobster';
        font-style: normal;
        font-weight: 400;
        src: local('Lobster'), url(http://themes.googleusercontent.com/static/fonts/lobster/v5/9eID_a1kLfzp_BP9s4L15g.woff) format('woff');
    }
")
            });
            var res = new HTTP.HttpPack(page.ToString());
            res.ContentType = page.ContentType;
            res.Cookies.Add(new System.Net.Cookie("visitcount", visitCount.ToString()));
            return res.ToString(HTTP.ResponseCode.OK);
        }
    }
}
