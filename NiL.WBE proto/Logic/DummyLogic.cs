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

        private void sendResource(HTTP.HttpPack pack, Socket client)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(pack.Request);
            byte[] buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);
            stream.Close();
            var encoding = Encoding.GetEncoding(1251);
            var res = new HTTP.HttpPack(encoding.GetString(buf));
            res.ContentType = "application/octet-stream";
            client.Send(encoding.GetBytes(res.ToString(HTTP.ResponseCode.OK)));
        }

        public override void Process(HTTP.HttpServer server, HTTP.HttpPack pack, Socket client)
        {
            if (pack.Path == "/resource")
                sendResource(pack, client);
            var encoding = Encoding.UTF8;
            if (pack.Path == "/error")
            {
                int errorCode = 400;
                int.TryParse(pack.Request, out errorCode);
                if (errorCode != 404)
                    client.Send(encoding.GetBytes(new HTTP.ErrorPage((HTTP.ResponseCode)errorCode, ((HTTP.ResponseCode)errorCode).ToString().Replace('_', ' ')).ToString()));
            }
            if (pack.Path != "/" && pack.Path != "")
                client.Send(encoding.GetBytes(new HTTP.ErrorPage(HTTP.ResponseCode.NOT_FOUND, "Oops!").ToString()));
            int visitCount = 0;
            var t = pack.Cookies["visitcount"];
            if (t != null)
                int.TryParse(t.Value, out visitCount);
            visitCount++;
            var page = new HtmlPage()
            { 
                new HtmlElement("div", "content")
                {
                    new HtmlElement("div", "toptext") { new Text("if you see it, then") },
                    new HtmlElement("div", "title") { new Text("NiL.WBE") },
                    new HtmlElement("div", "bottomtext") { new Text("working") },
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
            page.Head.Add(new HtmlElement("title") { new Text("NiL.WBE") });
            var res = new HTTP.HttpPack(page.ToString());
            res.ContentType = page.ContentType;
            res.Cookies.Add(new System.Net.Cookie("visitcount", visitCount.ToString()));
            client.Send(encoding.GetBytes(res.ToString(HTTP.ResponseCode.OK)));
            var connection = pack.Fields["connection"];
            if (string.Compare("keep-alive", connection, StringComparison.OrdinalIgnoreCase) == 0
                || (string.IsNullOrWhiteSpace(connection) && pack.Version == "HTTP/1.1"))
                return;
            client.Close();
        }
    }
}
