﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.HTTP
{
    public sealed class HeaderFields
    {
        private Dictionary<string, string> headers;

        public string this[string key]
        {
            get
            {
                var res = "";
                headers.TryGetValue(key, out res);
                return res; ;
            }
            set
            {
                switch (key.ToLower())
                {
                    case "method":
                    case "cookie":
                    case "content-length":
                    case "content-type":
                    case "set-cookie":
                        throw new ArgumentException();
                }
                headers[key] = value;
            }
        }

        public HeaderFields()
        {
            headers = new Dictionary<string, string>();
        }

        public void Add(string name, string value)
        {
            this[name] = value;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return headers.GetEnumerator();
        }
    }
}
