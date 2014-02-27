using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.HTML
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
            Subelements = new List<HtmlElement>();
            Properties = new Dictionary<string, string>();
            Properties.Add("href", href);
        }
    }
}
