using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.WBE.HTML;

namespace NiL.WBE.HTTP
{
    public sealed class ErrorPage
    {
        public ResponseCode Code { get; private set; }
        public string Message { get; private set; }

        public ErrorPage(ResponseCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            var page = new HtmlPage()
            { 
                new HtmlElement("div", "content")
                {
                    new HtmlElement("div", "title")
                    {
                        new Text(((int)Code).ToString())
                    },                
                    new HtmlElement("div", "bottomtext")
                    {
                        new Text(Message)
                    },
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
        font-family: Verdana, sans-serif;
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
")
            });
            var res = new HTTP.HttpPack(page.ToString());
            res.Fields.Add("Content-type", page.ContentType);
            return res.ToString(Code);
        }
    }
}
