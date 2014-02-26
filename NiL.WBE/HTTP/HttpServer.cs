using System;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiL.WBE.HTTP
{
    public sealed class LoggerEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public LoggerEventArgs(string text)
        {
            this.Text = text;
        }
    }

    public class HttpServer
    {
        private _ClientsHolder[] holders;

        private class _ClientsHolder
        {
            public class Client
            {
                public int lastActivity;
                public Socket socket;
            }

            private HttpServer owner;
            public List<Client> Clients { get; private set; }
            public int ClientsCount { get; private set; }

            public _ClientsHolder(HttpServer owner)
            {
                this.owner = owner;
                Clients = new List<Client>();
            }

            public void Add(Socket client)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i] == null)
                    {
                        Clients[i] = new Client() { socket = client, lastActivity = Environment.TickCount };
                        ClientsCount++;
                        return;
                    }
                }
                Clients.Add(new Client() { socket = client, lastActivity = Environment.TickCount });
            }

            private void process()
            {
                while (owner.Working)
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (Clients[i] != null)
                        {
                            try
                            {
                                var client = Clients[i];
                                if (client.socket.Poll(100, SelectMode.SelectRead))
                                {
                                    if (client.socket.Available > 0)
                                    {
                                        List<byte> buf = new List<byte>();
                                        byte[] temp = new byte[client.socket.Available];
                                        while (client.socket.Available > 0)
                                        {
                                            client.socket.Receive(temp, Math.Min(temp.Length, client.socket.Available), SocketFlags.Partial);
                                            buf.AddRange(temp);
                                            for (int j = 0; j < temp.Length; j++)
                                                temp[j] = 0;
                                        }
                                        if (buf.Count > 0)
                                        {
                                            client.lastActivity = Environment.TickCount;
                                            var m = Encoding.UTF8.GetString(buf.ToArray());
                                            client.socket.Send(Encoding.UTF8.GetBytes(owner.Logic.Process(owner, HTTP.HttpPack.Parse(m))));
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (Environment.TickCount - client.lastActivity > 20000)
                                                client.socket.Receive(null, 0, SocketFlags.Peek);
                                        }
                                        catch
                                        {
                                            Clients[i] = null;
                                            ClientsCount--;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                owner.log(e.Message);
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }

            public async void Start(int holderIndex)
            {
                owner.log(this.GetType().Name + " #" + holderIndex + " start working");
                var t = new Task(process);
                t.Start();
                await t;
                owner.log(this.GetType().Name + " #" + holderIndex + " stoped");
            }
        }

        private TcpListener listener;

        public bool Working { get; private set; }
        public Logic.LogicProvider Logic { get; private set; }

        public event EventHandler<LoggerEventArgs> Log;
        private void log(string text)
        {
            if (Log != null)
                Log(this, new LoggerEventArgs(DateTime.Now + ": " + text));
        }

        public HttpServer(Logic.LogicProvider logic)
            : this(logic, 80)
        {

        }

        public HttpServer(Logic.LogicProvider logic, int port)
        {
            Logic = logic;
            listener = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            holders = new _ClientsHolder[Environment.ProcessorCount];
            for (int i = 0; i < holders.Length; i++)
            {
                holders[i] = new _ClientsHolder(this);
            }
        }

        public void Run()
        {
            log("starting server at port " + listener.LocalEndpoint.ToString().Split(':')[1]);
            Working = true;
            try
            {
                for (int i = 0; i < holders.Length; i++)
                    holders[i].Start(i);
                listener.Start();
                while (Working)
                {
                    var client = listener.AcceptSocket();
                    int mc = 0;
                    int hi = 0;
                    for (int i = 0; i < holders.Length; i++)
                    {
                        if (holders[i].ClientsCount < mc)
                        {
                            mc = holders[i].ClientsCount;
                            hi = i;
                        }
                    }
                    holders[hi].Add(client);
                }
            }
            catch (Exception e)
            {
                log("Error: " + e);
            }
            finally
            {
                log("HttpServer stopped");
                Working = false;
            }
        }

        public void Stop()
        {
            Working = false;
        }
    }
}
