using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.WBE.HTTP;

namespace NiL.WBE.Logic
{
    public abstract class LogicProvider
    {
        protected LogicProvider()
        {

        }

        public abstract string Process(HTTP.HttpServer server, HttpPack pack);
    }
}
