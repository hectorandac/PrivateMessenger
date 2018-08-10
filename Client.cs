using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace pmessenger
{
    class Client
    {
        public Socket ClientSocket { get; set; }
        public string SessionId { get; set; }
        public string PublicKey { get; set; }

        public Client(Socket s, string id, string pk)
        {
            this.ClientSocket = s;
            this.SessionId = id;
            this.PublicKey = pk;
        }
    }
}
