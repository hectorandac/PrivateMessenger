using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace pmessenger
{
    class RequiredConfiguration
    {
        // Spinner that indicates loading
        public Spinner spinner = new Spinner(10, 10);
        public Socket socket;

        // Max attempts
        public const int MAX_RECEIVE_ATTEMPT = 10;
        public int receiveAttempt = 0;

        // Limit request size
        private const int BUFFER_SIZE = 8192;
        public byte[] buffer = new byte[BUFFER_SIZE];

        // Default values
        public int PORT = 10094;
        public string SERVER_IP = "127.0.0.1";
    }
}
