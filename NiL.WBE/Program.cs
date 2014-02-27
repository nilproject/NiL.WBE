using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.WBE.HTTP;

namespace NiL.WBE
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer(new Logic.DummyLogic(), 80);
            server.Log += (s, e) => { Console.WriteLine(e.Text); };
            server.Run();
        }
    }
}
