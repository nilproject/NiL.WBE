using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public sealed class Text : HtmlElement
    {
        public override string ContentType
        {
            get
            {
                return "text/plain; charset=utf-8";
            }
        }

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

        public string Value { get; set; }
        public bool Encode { get; set; }

        public Text()
            : base(false)
        {

        }

        public Text(string value)
            : base(false)
        {
            Value = value;
            Encode = true;
        }

        public Text(string value, bool encode)
            : base(false)
        {
            Value = value;
            Encode = encode;
        }

        public override string ToString()
        {
            if (!Encode)
                return Value;
            return System.Web.HttpUtility.HtmlEncode(Value);
        }

        public static implicit operator Text(string value)
        {
            return new Text(value);
        }
    }
}
