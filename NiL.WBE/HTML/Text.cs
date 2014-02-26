using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.HTML
{
    public sealed class Text : HtmlElement
    {
        public override string Name
        {
            get
            {
                return "Text";
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

        public string Value { get; private set; }

        public Text()
            : base(false)
        {

        }

        public Text(string value)
            : base(false)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
