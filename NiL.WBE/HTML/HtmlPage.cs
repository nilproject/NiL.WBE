using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
