using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pmessenger
{
    class Server: RequiredConfiguration
    {
        // List of clients connected
        private static Dictionary<string, Client> Clients = new Dictionary<string, Client>();

        public static string ListToString(Dictionary<string, Client> list)
        {
            string text = "";
            foreach (KeyValuePair<string, Client> client in list)
            {
                text += client.Key + Environment.NewLine;
            };
            return text;
        }

        public void StartServer()
        {
            spinner.Start();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            socket.Listen(100);
            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            spinner.Stop();

            Console.WriteLine("Server ip: {0}", SERVER_IP);
            Console.WriteLine("Server port: {0}", PORT);
            Console.WriteLine("SERVER STARTED | Displaying log...");

            string result = "";
            do
            {
                result = Console.ReadLine();
            } while (result.ToLower().Trim() != "exit");

        }

        public void AcceptCallback(IAsyncResult result)
        {
            Socket socketAccepted = null;
            try
            {
                socketAccepted = socket.EndAccept(result);
                socketAccepted.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socketAccepted);
                socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static Client FindRecipient(string recipientId)
        {
            return Clients[recipientId];
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = null;
            try
            {
                socket = (Socket)result.AsyncState;
                if (socket.Connected)
                {
                    int received = socket.EndReceive(result);
                    if (received > 0)
                    {
                        byte[] data = new byte[received];
                        Buffer.BlockCopy(buffer, 0, data, 0, data.Length);

                        JsonSerializer serializer = new JsonSerializer();
                        JObject reader = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data));
                        Request request = (Request)serializer.Deserialize(new JTokenReader(reader), typeof(Request));
                        string target = request.request;

                        switch (target)
                        {
                            case "GET-USERS":
                                socket.Send(Encoding.ASCII.GetBytes(ListToString(Clients)));
                                break;
                            case "REGISTE-USER":
                                Client client = new Client(socket, request.userId);
                                Clients.Add(request.userId, client);
                                Console.WriteLine("User with id: {0} | REGISTERED | on => " + DateTime.Now, request.userId);
                                break;
                            case "MESSAGE-USER":
                                try
                                {
                                    Console.WriteLine("Sending message from: {0}  ##|##  to: {1}", request.userId, request.recipientId);
                                    Client recipient = FindRecipient(request.recipientId);
                                    Socket recipientSocket = recipient.socket;
                                    recipientSocket.Send(Encoding.ASCII.GetBytes(request.userId + ": " + request.message));
                                }
                                catch(KeyNotFoundException)
                                {
                                    socket.Send(Encoding.ASCII.GetBytes("[[ User:: "+ request.recipientId +" not found ]]"));
                                }
                                break;
                            default:
                                //Message retrieval part
                                //Suppose you only want to declare that you receive data from a client to that client
                                string msg = "Received: " + DateTime.Now;
                                socket.Send(Encoding.ASCII.GetBytes(msg));
                                break;
                        }
                        receiveAttempt = 0; //reset receive attempt
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                    }
                    else if (receiveAttempt < MAX_RECEIVE_ATTEMPT)
                    { //fail but not exceeding max attempt, repeats
                        ++receiveAttempt; //increase receive attempt;
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket); //repeat beginReceive
                    }
                    else
                    { //completely fails!
                        Console.WriteLine("receiveCallback fails!"); //don't repeat beginReceive
                        receiveAttempt = 0; //reset this for the next connection
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("Disconnected Socket");
            }
            catch (Exception e)
            { // this exception will happen when "this" is be disposed...
                Console.WriteLine("receiveCallback fails with exception! " + e.ToString());
            }
        }
    }
}
