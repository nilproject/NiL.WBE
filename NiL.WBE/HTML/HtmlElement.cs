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
        public virtual string ContentType { get { return "text/html"; } }
        public virtual string Name { get; protected set; }
        public virtual Dictionary<string, string> Properties { get; protected set; }
        public virtual List<HtmlElement> Subelements { get; protected set; }

        public HtmlElement this[string name]
        {
            get
            {
                return this[name, 0];
            }
        }

        public virtual HtmlElement this[string name, int index]
        {
            get
            {
                for (int i = 0; i < Subelements.Count; i++)
                {
                    if (Subelements[i].Name == name && index-- == 0)
                    {
                        return Subelements[i];
                    }
                }
                return null;
            }
        }

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

        public HtmlElement(string name, string id)
            : this(true)
        {
            Name = name;
            Properties.Add("id", id);
        }

        public HtmlElement(string name, string id, string className)
            : this(true)
        {
            Name = name;
            Properties.Add("id", id);
            Properties.Add("class", className);
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

        public virtual HtmlElement GetElementById(string id)
        {
            if (Subelements != null)
                for (int i = 0; i < Subelements.Count; i++)
                {
                    string cid = null;
                    if (Subelements[i].Properties.TryGetValue("id", out cid) && cid == id)
                        return Subelements[i];
                    else
                    {
                        var temp = Subelements[i].GetElementById(id);
                        if (temp != null)
                            return temp;
                    }
                }
            return null;
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
