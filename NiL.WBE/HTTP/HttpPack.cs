using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Text;

namespace NiL.WBE.HTTP
{
    public sealed class HttpPack
    {
        private static bool isCTL(char c)
        {
            return c < 32 || c == 127;
        }

        private static bool isSeparator(char c)
        {
            return c == '(' || c == ')' || c == '<' || c == '>' || c == '@'
                      || c == ',' || c == ';' || c == ':' || c == '\\' || c == '"'
                      || c == '/' || c == '[' || c == ']' || c == '?' || c == '='
                      || c == '{' || c == '}' || c == ' ' || c == '\x9';
        }

        public string Version { get; private set; }
        public string Path { get; private set; }
        public string Host { get; private set; }
        public Method Method { get; private set; }
        public CookieCollection Cookies { get; private set; }
        public HeaderFields Fields { get; private set; }
        public string Body { get; private set; }

        public HttpPack()
            : this("")
        {
        }

        public HttpPack(string body)
        {
            Version = "HTTP/1.1";
            Cookies = new CookieCollection();
            Fields = new HeaderFields();
            Body = body;
        }

        public static HttpPack Parse(string text)
        {
            HttpPack pack = new HttpPack();
            bool work = true;
            int startIndex = 0;
            int endIndex = 0;
            while (!isCTL(text[endIndex]) && !isSeparator(text[endIndex]))
                endIndex++;
            switch (text.Substring(0, endIndex))
            {
                case "GET":
                    {
                        pack.Method = Method.GET;
                        break;
                    }
                case "POST":
                    {
                        pack.Method = Method.POST;
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
            while (char.IsWhiteSpace(text[endIndex])) endIndex++;
            startIndex = endIndex;
            while (!char.IsWhiteSpace(text[endIndex])) endIndex++;
            pack.Path = text.Substring(startIndex, endIndex - startIndex);
            while (char.IsWhiteSpace(text[endIndex])) endIndex++;
            startIndex = endIndex;
            while (!char.IsWhiteSpace(text[endIndex])) endIndex++;
            pack.Version = text.Substring(startIndex, endIndex - startIndex);
            while (char.IsWhiteSpace(text[endIndex])) endIndex++;
            startIndex = endIndex;
            while (work)
            {
                endIndex = text.IndexOf("\n", startIndex + 1, StringComparison.Ordinal);
                if (endIndex == -1)
                    break;
                int i = startIndex;
                while (!isCTL(text[i]) && !isSeparator(text[i]))
                    i++;
                string name = text.Substring(startIndex, i - startIndex).ToLower();
                if (name == "" || name == "\r")
                    break;
                while (char.IsWhiteSpace(text[i])) i++;
                if (text[i] != ':')
                    throw new ArgumentException();
                do i++; while (char.IsWhiteSpace(text[i]));
                if (name == "cookie")
                {
                    var c = text.Substring(i, endIndex - i).Trim().Split('=');
                    pack.Cookies.Add(new Cookie(c[0], c.Length > 1 ? c[1] : ""));
                }
                else
                {
                    switch (name)
                    {
                        case "host":
                            {
                                pack.Host = text.Substring(i, endIndex - i).Trim();
                                break;
                            }
                        default:
                            {
                                pack.Fields[name] = text.Substring(i, endIndex - i).Trim();
                                break;
                            }
                    }
                }
                startIndex = endIndex + 1;
            }
            pack.Body = text.Substring(endIndex + 1);
            return pack;
        }

        public override string ToString()
        {
            return ToString(ResponseCode.None);
        }

        public string ToString(ResponseCode code)
        {
            List<string> headers = new List<string>();
            bool ctdef = false;
            foreach (var f in Fields)
            {
                ctdef = ctdef || f.Key.ToLower() == "content-type";
                headers.Add(f.Key + ": " + f.Value);
            }
            if (!ctdef)
                headers.Add("Content-type: text/plain");
            if (code != ResponseCode.None)
                for (var i = 0; i < Cookies.Count; i++)
                    headers.Add("Set-Cookie: " + Cookies[i]);
            else
                for (var i = 0; i < Cookies.Count; i++)
                    headers.Add("Cookie: " + Cookies[i]);
            headers.Sort();
            StringBuilder res = new StringBuilder();
            if (code == ResponseCode.None)
                res.Append(Method.ToString()).Append(" ").Append(Path).Append(" ").Append(Version).Append("\n").Append("Host: ").Append(Host).Append("\n");
            else
                res.Append(Version).Append(" ").Append((int)code).Append(" ").Append(code.ToString()).Append("\n");
            for (int i = 0; i < headers.Count; i++)
                res.Append(headers[i]).Append("\n");
            if (code != ResponseCode.None)
                res.Append("Content-Length: " + Encoding.UTF8.GetByteCount(Body));
            res.Append("\n\n");
            res.Append(Body).Append("\n");
            string ress = res.ToString();
            return ress;
        }
    }
}