using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Web;

namespace NiL.WBE.Logic
{
    public abstract class LogicProvider
    {
        protected LogicProvider()
        {

        }

        public abstract void Process(HttpRequest request, HttpResponse response, HttpApplication application);
    }
}
