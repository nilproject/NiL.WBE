using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.WBE.HTML;
using System.Net.Sockets;
using System.Web;

namespace NiL.WBE.Logic
{
    public class DummyLogic : LogicProvider
    {
        public DummyLogic()
        {

        }

        public override void Process(HttpRequest request, HttpResponse response)
        {
            var encoding = Encoding.UTF8;
            int visitCount = 0;
            var t = request.Cookies["visitcount"];
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
", false)
            });
            page.Head.Add(new HtmlElement("title") { new Text("NiL.WBE") });
            response.ContentType = page.ContentType;
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.BinaryWrite(encoding.GetBytes(page.ToString()));
            response.Cookies.Add(new HttpCookie("visitcount", visitCount.ToString()));
            response.Status = "200 ALLRIGHT";
            response.End();
        }
    }
}
