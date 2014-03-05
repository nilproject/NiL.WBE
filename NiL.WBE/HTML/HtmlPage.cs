using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NiL.WBE.Html
{
    public sealed class HtmlPage : HtmlElement
    {
        public override string Name
        {
            get
            {
                return "html";
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }

        public override HtmlElement this[string name, int index]
        {
            get
            {
                if (index != 0)
                    return null;
                switch (name)
                {
                    case "body": return Body;
                    case "head": return Head;
                    default: return null;
                }
            }
        }

        public HtmlElement Head { get; private set; }
        public HtmlElement Body { get; private set; }

        public HtmlPage()
            : base(false)
        {
            Head = new HtmlElement("head");
            Body = new HtmlElement("body");
        }

        public override void Add(HtmlElement element)
        {
            Body.Add(element);
        }

        public override IEnumerator<HtmlElement> GetEnumerator()
        {
            return Body.GetEnumerator();
        }

        public override string ToString()
        {
            return 
@"<!DOCTYPE html>
<html>
" + Head + "\n"
  + Body + @"
</html>";
        }

        public override object Clone()
        {
            var res = base.Clone() as HtmlPage;
            res.Body = res["body"];
            res.Head = res["head"];            
            return res;
        }

        public static HtmlPage Parse(string html)
        {
            HtmlPage res = new HtmlPage();
            int i = 0;
            while (char.IsWhiteSpace(html[i])) i++;
            if (html[i] != '<')
                throw new ArgumentException("Invalid char.");
            i++;
            if (html[i] == '!')
            {
                while (html[i++] != '>') ;
                while (char.IsWhiteSpace(html[i++])) ;
            }
            i--;
            var temp = HtmlElement.Parse(html, ref i);
            if (temp.Name != "html")
                throw new ArgumentException("Invalid root tag.");
            res.Body = temp["body"];
            res.Head = temp["head"];
            if (res.Body == null || res.Head == null)
                throw new ArgumentException();
            if (temp.Subnodes.Count != 2)
                throw new ArgumentException();
            res.Subnodes = temp.Subnodes;
            res.Attributes = temp.Attributes;
            return res;
        }
    }
}
