using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.HTTP
{
    public sealed class HeaderFields : Dictionary<string, string>
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
                        throw new ArgumentException();
                }
                base[key] = value;
            }
        }
    }
}
