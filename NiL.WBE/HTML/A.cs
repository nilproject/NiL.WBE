using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public class A : HtmlElement
    {
        public override string Name
        {
            get
            {
                return "a";
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }

        public A(string href)
            : base(false)
        {
            Subnodes = new List<HtmlElement>();
            Attributes = new Dictionary<string, string>();
            Attributes.Add("href", href);
        }
    }
}
