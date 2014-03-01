using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public sealed class Img : HtmlElement
    {
        public override string Name
        {
            get
            {
                return "img";
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }

        public string Src { get; private set; }

        public Img(string src)
            : base(false)
        {
            Src = src;
        }

        public override string ToString()
        {
            return "<img src=\"" + Src + "\">";
        }
    }
}
