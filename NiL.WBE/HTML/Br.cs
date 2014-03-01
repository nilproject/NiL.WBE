using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public sealed class Br : HtmlElement
    {
        public override string Name
        {
            get
            {
                return "Line break";
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }

        public override Dictionary<string, string> Attributes
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

        public override List<HtmlElement> Subnodes
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

        public Br()
            : base(false)
        {

        }

        public override string ToString()
        {
            return "\n<br/>\n";
        }
    }
}
