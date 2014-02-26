using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.HTML
{
    public class HtmlElement : IEnumerable<HtmlElement>
    {
        public virtual string Name { get; protected set; }
        public virtual Dictionary<string, string> Properties { get; protected set; }
        public virtual List<HtmlElement> Subelements { get; protected set; }

        protected HtmlElement(bool initFields)
        {
            if (initFields)
            {
                Name = "";
                Properties = new Dictionary<string, string>();
                Subelements = new List<HtmlElement>();
            }
        }

        public HtmlElement()
            : this(true)
        {
        }

        public HtmlElement(string name)
            : this(true)
        {
            Name = name;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append("<").Append(Name);
            foreach (var p in Properties)
            {
                var bound = p.Value.IndexOf('"') == -1 ? '"' : '\'';
                res.Append(' ').Append(p.Key).Append("=").Append(bound).Append(p.Value).Append(bound);
            }
            res.Append('>');
            for (int i = 0; i < Subelements.Count; i++)
                res.Append(Subelements[i].ToString());
            res.Append("</").Append(Name).Append('>');
            return res.ToString();
        }

        public virtual IEnumerator<HtmlElement> GetEnumerator()
        {
            return Subelements.GetEnumerator();
        }

        public virtual void Add(HtmlElement element)
        {
            if (Subelements == null)
                throw new InvalidOperationException();
            Subelements.Add(element);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<HtmlElement>)this).GetEnumerator();
        }
    }
}
