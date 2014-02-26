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
            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    var server0 = new HttpServer(new Logic.DummyLogic(), 80);
                    server0.Log += (s, e) => { Console.WriteLine(e.Text); };
                    server0.Run();
                }
                catch
                {

                }
            });

            try
            {
                var server = new HttpServer(new Logic.DummyLogic(), 8080);
                server.Log += (s, e) => { Console.WriteLine(e.Text); };
                server.Run();
            }
            catch
            { }
        }
    }
}
