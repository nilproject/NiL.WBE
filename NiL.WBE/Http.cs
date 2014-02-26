using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Http
{
    enum HttpRequestMethod
    {
        GET,
        POST
    }

    class HttpHeaders : Dictionary<string, string>
    {
        public new string this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                switch (key.ToLower())
                {
                    case "method":
                    case "cookie":
                    case "content-length":
                    case "user-agent":
                    case "host":
                        throw new ArgumentException();
                }
                base[key] = value;
            }
        }
    }

    class Cookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Expired { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }

        public Cookie()
        {
            Name = "";
            Value = "";
            Domain = "";
            Path = "";
            Expired = "";
        }

        public override string ToString()
        {
            return Name + "=" + Value + ";";
        }
    }

    class Response
    {
        public string Header { get; private set; }
        public string Content { get; private set; }
        public Cookie[] Cookies { get; private set; }

        public Response(string header, string content, Cookie[] cookie)
        {
            Header = header;
            Content = content;
            Cookies = cookie;
        }
    }

    class HttpRequest
    {
        private string referal;
        public Uri Uri { get; private set; }
        public HttpHeaders Headers { get; private set; }
        public HttpRequestMethod Method { get; set; }
        public List<Cookie> Cookies { get; private set; }
        public string UserAgent { get; set; }
        public string POSTContent { get; set; }
        public bool AutoRedirect { get; set; }

        private HttpRequest()
        {
            POSTContent = "";
            UserAgent = "";
        }

        public HttpRequest(Uri uri)
        {
            Uri = uri;
            Headers = new HttpHeaders();
            Cookies = new List<Cookie>();
        }

        public Response GetResponse(int port = 80, int maxredirect = 10)
        {
            string content = Method.ToString() + " " + Uri.PathAndQuery + " HTTP/1.1\n";
            List<string> headers = new List<string>();
            if (!Headers.ContainsKey("User-Agent"))
                Headers.Add("User-Agent", UserAgent);
            else
            {
                Headers.Remove("User-Agent");
                Headers.Add("User-Agent", UserAgent);
            }
            foreach (var p in Headers)
                headers.Add(p.Key + ":" + p.Value + '\n');
            for (int i = 0; i < Cookies.Count; i++)
                headers.Add("Cookie:" + Cookies[i] + '\n');
            if (referal != null)
                headers.Add("Referer:" + referal + '\n');
            headers.Sort(StringComparer.Create(System.Threading.Thread.CurrentThread.CurrentCulture, true));
            content += "Host:" + Uri.DnsSafeHost + '\n';
            for (int i = 0; i < headers.Count; i++)
            {
                content += headers[i];
            }
            if (((POSTContent ?? "") != "") && (Method == HttpRequestMethod.POST))
            {
                content += "Content-Length:" + POSTContent.Length + '\n';
                content += '\n' + POSTContent ?? "";
            }
            else
                content += "\n\n";
            System.Net.Sockets.TcpClient client;
            client = new System.Net.Sockets.TcpClient();
            client.Connect(Uri.DnsSafeHost, port);
            var buf = Encoding.UTF8.GetBytes(content);
            client.GetStream().Write(buf, 0, buf.Length);
            var stream = client.GetStream();
            List<byte> response = new List<byte>();
            string respHeader = "";
            int b = 0;
            bool endHeader = false;
            while ((b = stream.ReadByte()) != -1)
            {
                if (b == '\n')
                {
                    if ((response.Count == 0) || ((response.Count == 1) && (response[0] == 13)))
                    {
                        if (!endHeader)
                        {
                            endHeader = true;
                            response.Clear();
                            continue;
                        }
                    }
                    response.Add((byte)b);
                    if (!endHeader)
                    {
                        respHeader += Encoding.UTF8.GetString(response.ToArray());
                        response.Clear();
                    }
                }
                else
                    response.Add((byte)b);
            }
            if (headers.Contains("Connection"))
            {
                if (Headers["Connection"] == "Close")
                    client.Close();
            }
            else
                client.Close();
            string contentType = respHeader.Substring(respHeader.IndexOf("Content-Type:"));
            contentType = contentType.Substring(13, contentType.IndexOf('\n') - 13).Trim();
            string responseContent = "";
            if (contentType.IndexOf("text/") != -1)
            {
                string charset = (contentType.IndexOf("charset") != -1) ? contentType.Substring(contentType.IndexOf("charset") + 8) : "utf-8";
                if (charset.IndexOf(";") != -1)
                    charset = charset.Substring(0, charset.IndexOf(';'));
                responseContent = Encoding.GetEncoding(charset).GetString(response.ToArray());
            }
            if ((AutoRedirect) && (maxredirect > 0))
            {
                string result = respHeader.Substring(0, respHeader.IndexOf('\n')).Trim();
                int sci = respHeader.IndexOf("\nSet-Cookie: ");
                while (sci != -1)
                {
                    int eqi = respHeader.IndexOf('=', sci);
                    string name = respHeader.Substring(sci + 13, eqi - sci - 13);

                    string value = respHeader.Substring(eqi + 1, respHeader.IndexOf(';', sci) - eqi - 1);
                    if (value != "deleted")
                    {
                        if (Cookies.FindIndex((x) => { return x.Name == name; }) == -1)
                            Cookies.Add(new Cookie() { Name = name, Value = value });
                        else
                            Cookies.Find((x) => { return x.Name == name; }).Value = value;
                    }
                    else
                    {
                        int index = Cookies.FindIndex((x) => { return name == x.Name; });
                        if (index != -1)
                            Cookies.RemoveAt(index);
                    }
                    sci = respHeader.IndexOf("\nSet-Cookie: ", sci + 1);
                }
                if (result == "HTTP/1.1 302 Found")
                {                    
                    string redurl = respHeader.Substring(respHeader.IndexOf("\nLocation:"));
                    redurl = redurl.Substring(redurl.IndexOf(':') + 1);
                    redurl = redurl.Substring(0, redurl.IndexOf('\n')).Trim();
                    if (redurl == "/")
                        redurl = Uri.Scheme + "://" + Uri.Host + "/";
                    Uri reuri = new Uri(redurl);
                    return (new HttpRequest() { Uri = reuri, UserAgent = UserAgent, AutoRedirect = true, Headers = Headers, Cookies = Cookies, referal = Uri.DnsSafeHost + Uri.PathAndQuery }).GetResponse(port, maxredirect - 1);
                }
            }
            return new Response(respHeader, responseContent, Cookies.ToArray());
        }
    }
}
