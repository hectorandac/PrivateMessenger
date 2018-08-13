using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pmessenger
{
    class Client: RequiredConfiguration
    {
        
        public string SessionId { get; set; }

        public Client(Socket s, string id)
        {
            this.socket = s;
            this.SessionId = id;
        }

        public List<string> recipientsIds = new List<string>();

        private static String PrintRecipients(List<string> res)
        {
            string text = "Recipient list";
            foreach (string r in res)
            {
                text += r + Environment.NewLine;
            }
            return text;
        }

        public void RegisterClient(string userId)
        {
            spinner.Start();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect(3, 3);
            string request = ("{" +
                        "'request': 'REGISTE-USER'," +
                        "'userId': '" + userId + "'}");
            byte[] bytes = Encoding.ASCII.GetBytes(request);
            socket.Send(bytes);
            spinner.Stop();
            Console.WriteLine("Connected");

            string result = "";
            do
            {
                result = Console.ReadLine();
                if (result.Contains("-r"))
                {
                    string[] recipients = result.Replace("-r", "").Trim().Split(',');
                    foreach (string res in recipients)
                    {
                        if (!recipientsIds.Contains(res))
                            recipientsIds.Add(res);
                    }
                    PrintRecipients(recipientsIds);
                }
                else if (result.ToLower().Trim() != "exit")
                {
                    foreach (string res in recipientsIds)
                    {
                        string message = "{" +
                            "'request': 'MESSAGE-USER'," +
                            "'userId': '" + userId + "'," +
                            "'message': '" + result + "'," +
                            "'recipientId': '" + res + "'" +
                            "}";
                        byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
                        socket.Send(bytesToSend);
                    }
                }
            } while (result.ToLower().Trim() != "exit");
        }

        public void GetPeers()
        {
            spinner.Start();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect(3, 3);
            socket.Send(Encoding.ASCII.GetBytes("{'request': 'GET-USERS'}"));
            spinner.Stop();

            string result = "";
            do
            {
                Console.WriteLine("[[ Type 'exit' to continue ]]");
                result = Console.ReadLine();
            } while (result.ToLower().Trim() != "exit");
        }


        private void LoopConnect(int noOfRetry, int attemptPeriodInSeconds)
        {
            int attempts = 0;
            while (!socket.Connected && attempts < noOfRetry)
            {
                try
                {
                    ++attempts;
                    IAsyncResult result = socket.BeginConnect(IPAddress.Parse(SERVER_IP), PORT, EndConnectCallback, null);
                    result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(attemptPeriodInSeconds));
                    System.Threading.Thread.Sleep(attemptPeriodInSeconds * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.ToString());
                }
            }
            if (!socket.Connected)
            {
                Console.WriteLine("Connection attempt is unsuccessful!");
                return;
            }
        }

        public void EndConnectCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);
                if (socket.Connected)
                {
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), socket);
                }
                else
                {
                    Console.WriteLine("End of connection attempt, fail to connect...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("End-connection attempt is unsuccessful! " + e.ToString());
            }
        }

        public void ReceiveCallbackClient(IAsyncResult result)
        {
            System.Net.Sockets.Socket socket = null;
            try
            {
                socket = (System.Net.Sockets.Socket)result.AsyncState;
                if (socket.Connected)
                {
                    int received = socket.EndReceive(result);
                    if (received > 0)
                    {
                        receiveAttempt = 0;
                        byte[] data = new byte[received];
                        Buffer.BlockCopy(buffer, 0, data, 0, data.Length); //There are several way to do this according to https://stackoverflow.com/questions/5099604/any-faster-way-of-copying-arrays-in-c in general, System.Buffer.memcpyimpl is the fastest
                        //DO ANYTHING THAT YOU WANT WITH data, IT IS THE RECEIVED PACKET!
                        //Notice that your data is not string! It is actually byte[]
                        //For now I will just print it out
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Encoding.UTF8.GetString(data));
                        Console.ResetColor();
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), socket);
                    }
                    else if (receiveAttempt < MAX_RECEIVE_ATTEMPT)
                    { //not exceeding the max attempt, try again
                        ++receiveAttempt;
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), socket);
                    }
                    else
                    { //completely fails!
                        Console.WriteLine("receiveCallback is failed!");
                        receiveAttempt = 0;
                        this.socket.Close();
                    }
                }
            }
            catch (Exception e)
            { // this exception will happen when "this" is be disposed...
                Console.WriteLine("receiveCallback is failed! " + e.ToString());
            }
        }
    }
}
