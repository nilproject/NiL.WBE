using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NiL.WBE.HTML
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

        public override Dictionary<string, string> Properties
        {
            get
            {
                return null;
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }

        public override List<HtmlElement> Subelements
        {
            get
            {
                return null;
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

        private static HtmlElement parseElement(string html, ref int pos)
        {
            int start = pos;
            while (char.IsLetterOrDigit(html[pos])) pos++;
            string tagName = html.Substring(start, pos - start);
            while (char.IsWhiteSpace(html[pos])) pos++;
            while (html[pos] != '>')
            {
                start = pos;
                while (char.IsLetterOrDigit(html[pos])) pos++;
                while (char.IsWhiteSpace(html[pos])) pos++;
            }
        }

        public static HtmlPage LoadFromString(string html)
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
            var temp = parseElement(html, ref i);
            if (temp.Name != "html")
                throw new ArgumentException("Invalid root tag.");
            res.Body = temp["body"];
            res.Head = temp["head"];
            return res;
        }
    }
}
