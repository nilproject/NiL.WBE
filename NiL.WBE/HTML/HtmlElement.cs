using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public class HtmlElement : IEnumerable<HtmlElement>
    {
        public virtual string ContentType { get { return "text/html"; } }
        public virtual string Name { get; protected set; }
        public virtual Dictionary<string, string> Attributes { get; protected set; }
        public virtual List<HtmlElement> Subnodes { get; protected set; }

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
                for (int i = 0; i < Subnodes.Count; i++)
                {
                    if (Subnodes[i].Name == name && index-- == 0)
                    {
                        return Subnodes[i];
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
                Attributes = new Dictionary<string, string>();
                Subnodes = new List<HtmlElement>();
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
            Attributes.Add("id", id);
        }

        public HtmlElement(string name, string id, string className)
            : this(true)
        {
            Name = name;
            Attributes.Add("id", id);
            Attributes.Add("class", className);
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append("<").Append(Name);
            foreach (var p in Attributes)
            {
                var bound = p.Value.IndexOf('"') == -1 ? '"' : '\'';
                res.Append(' ').Append(p.Key).Append("=").Append(bound).Append(p.Value).Append(bound);
            }
            res.Append('>');
            for (int i = 0; i < Subnodes.Count; i++)
                res.Append(Subnodes[i].ToString());
            res.Append("</").Append(Name).Append('>');
            return res.ToString();
        }

        public virtual HtmlElement GetElementById(string id)
        {
            if (Subnodes != null)
                for (int i = 0; i < Subnodes.Count; i++)
                {
                    string cid = null;
                    if (Subnodes[i].Attributes.TryGetValue("id", out cid) && cid == id)
                        return Subnodes[i];
                    else
                    {
                        var temp = Subnodes[i].GetElementById(id);
                        if (temp != null)
                            return temp;
                    }
                }
            return null;
        }

        public virtual IEnumerator<HtmlElement> GetEnumerator()
        {
            return Subnodes.GetEnumerator();
        }

        public virtual void Add(HtmlElement element)
        {
            if (Subnodes == null)
                throw new InvalidOperationException();
            Subnodes.Add(element);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<HtmlElement>)this).GetEnumerator();
        }

        private static KeyValuePair<string, string> parseAttribute(string html, ref int pos)
        {
            string name = "";
            string value = "";
            int index = pos;
            while (char.IsLetterOrDigit(html[pos])) pos++;
            name = html.Substring(index, pos - index);
            index = pos;
            while (char.IsWhiteSpace(html[index])) index++;
            if (html[pos] == '=')
            {
                pos = index++;
                while (char.IsWhiteSpace(html[pos])) pos++;
                char c = html[pos];
                if (c != '"' && c != '\'')
                    throw new ArgumentException();

            }
            return new KeyValuePair<string, string>(name, value);
        }

        public static HtmlElement Parse(string html, ref int pos)
        {
            HtmlElement res = new HtmlElement();
            if (html[pos] != '<')
                throw new ArgumentException("Invalid char at position " + pos);
            pos++;
            int start = pos;
            while (char.IsLetterOrDigit(html[pos])) pos++;
            res.Name = html.Substring(start, pos - start);
            do
            {
                while (char.IsWhiteSpace(html[pos])) pos++;
                if (html[pos] == '>')
                    break;
                var t = parseAttribute(html, ref pos);
            }
            while (true);
            pos++;
            return res;
        }
    }
}
