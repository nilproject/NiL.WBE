using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.Html
{
    public class HtmlElement : IEnumerable<HtmlElement>, ICloneable
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
                    if (Subnodes[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase) && index-- == 0)
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
                Attributes = new Dictionary<string, string>(StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, true));
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

        public virtual HtmlElement GetSubElementBy(string attributeName, string value)
        {
            if (Subnodes != null)
            {
                for (int i = 0; i < Subnodes.Count; i++)
                {
                    string cid = null;
                    if (Subnodes[i].Attributes != null && Subnodes[i].Attributes.TryGetValue(attributeName, out cid) && cid == value)
                        return Subnodes[i];
                    else
                    {
                        var temp = Subnodes[i].GetSubElementBy(attributeName, value);
                        if (temp != null)
                            return temp;
                    }
                }
            }
            return null;
        }
        
        public virtual HtmlElement[] GetSubElementsBy(string attributeName, string value)
        {
            if (Subnodes != null && Subnodes.Count != 0)
            {
                var res = new List<HtmlElement>();
                for (int i = 0; i < Subnodes.Count; i++)
                {
                    string cid = null;
                    if (Subnodes[i].Attributes != null && Subnodes[i].Attributes.TryGetValue(attributeName, out cid) && cid == value)
                        res.Add(Subnodes[i]);
                    var temp = Subnodes[i].GetSubElementsBy(attributeName, value);
                    if (temp != null)
                        res.AddRange(temp);
                }
                return res.Count == 0 ? null : res.ToArray();
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

        public virtual object Clone()
        {
            var res = this.MemberwiseClone() as HtmlElement;
            if (res.Subnodes != null)
                res.Subnodes = new List<HtmlElement>(res.Subnodes);
            if (res.Attributes != null)
                res.Attributes = new Dictionary<string, string>(res.Attributes);
            return res;
        }

        private static KeyValuePair<string, string> parseAttribute(string html, ref int pos)
        {
            string name = "";
            string value = "";
            int index = pos;
            while (char.IsLetterOrDigit(html[pos])
                || ((html[pos] == '-' || html[pos] == '.') && pos != index)
                || html[pos] == '_'
                || html[pos] == ':') pos++;
            name = html.Substring(index, pos - index);
            index = pos;
            while (char.IsWhiteSpace(html[index])) index++;
            if (html[index] == '=')
            {
                do index++; while (char.IsWhiteSpace(html[index]));
                while (char.IsWhiteSpace(html[index])) index++;
                char c = html[index];
                if (c != '"' && c != '\'')
                    throw new ArgumentException();
                pos = ++index;
                while (html[pos] != c) pos++;
                value = html.Substring(index, pos - index);
                pos++;
            }
            return new KeyValuePair<string, string>(name, value);
        }

        public static HtmlElement Parse(string html)
        {
            int p = 0;
            return Parse(html, ref p);
        }

        internal static HtmlElement Parse(string html, ref int pos)
        {
#if DEBUG
            try
            {
#endif
                HtmlElement res = new HtmlElement();
                if (html[pos] != '<')
                    throw new ArgumentException("Invalid char at position " + pos);
                pos++;
                int start = pos;
                while (char.IsLetterOrDigit(html[pos])
                        || ((html[pos] == '-' || html[pos] == '.') && pos != start)
                        || html[pos] == '_'
                        || html[pos] == ':') pos++;
                res.Name = html.Substring(start, pos - start);
                do
                {
                    while (char.IsWhiteSpace(html[pos])) pos++;
                    if (html[pos] == '>')
                        break;
                    if (html[pos] == '/')
                    {
                        if (html[++pos] != '>')
                            throw new ArgumentException();
                        pos++;
                        return res;
                    }
                    var t = parseAttribute(html, ref pos);
                    if (t.Key == "")
                        throw new ArgumentException("Invalid char \"" + html[pos] + "\" in element description");
                    res.Attributes.Add(t.Key, t.Value);
                }
                while (true);
                string finalSubS = "</" + res.Name;
                pos++;
                do
                {
                    while (char.IsWhiteSpace(html[pos])) pos++;
                    if (html[pos] == '<')
                    {
                        if (html.IndexOf(finalSubS, pos, StringComparison.OrdinalIgnoreCase) == pos)
                        {
                            pos += finalSubS.Length;
                            while (char.IsWhiteSpace(html[pos])) pos++;
                            if (html[pos] != '>')
                                throw new ArgumentException("Invalid close tag for \"" + res.Name + "\"");
                            pos++;
                            break;
                        }
                        else if (html[pos + 1] == '!')
                        {
                            pos += 3;
                            pos = html.IndexOf("-->", pos);
                            if (pos == -1)
                                throw new ArgumentException();
                            pos += 3;
                        }
                        else
                        {
                            res.Subnodes.Add(HtmlElement.Parse(html, ref pos));
                        }
                    }
                    else
                    {
                        start = pos;
                        while (html[pos] != '<') pos++;
                        res.Subnodes.Add(new Text(html.Substring(start, pos - start).TrimEnd())
                        {
                            Encode = res.Name != "script"
                        });
                    }

                }
                while (true);
                return res;
#if DEBUG
            }
            catch(Exception)
            {
                throw;
            }
#endif
        }
    }
}
