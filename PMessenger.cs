using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace pmessenger
{
    class PMessenger
    {
        static Spinner spinner;
        static int PORT = 10094;
        static string SERVER_IP = "127.0.0.1";
        static Socket serverSocket;
        private const int BUFFER_SIZE = 8192;
        private static byte[] buffer = new byte[BUFFER_SIZE];
        private static Dictionary<string, Client> Clients = new Dictionary<string, Client>();
        const int MAX_RECEIVE_ATTEMPT = 10;
        static int receiveAttempt = 0;
        static Socket clientSocket;

        public static string ListToString(Dictionary<string, Client> list)
        {
            string text = "";
            foreach( KeyValuePair<string, Client> client in list )
            {
                text += client.Key + Environment.NewLine;
            };
            return text;
        }

        private static void StartServer()
        {
            spinner.Start();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(100);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
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

        public static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = null;
            try
            {
                socket = serverSocket.EndAccept(result);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e) {           
                Console.WriteLine(e.ToString());
            }
        }

        public static Client FindRecipient(string recipientId)
        {
            return Clients[recipientId];
        }

        public static void ReceiveCallback(IAsyncResult result)
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
                                Client client = new Client(socket, request.userId, request.publicKey);
                                Clients.Add(request.userId, client);
                                Console.WriteLine("User with id: {0} | REGISTERED | on => " + DateTime.Now, request.userId);
                                break;
                            case "MESSAGE-USER":
                                Console.WriteLine("Sending message from: {0}  ##|##  to: {1}", request.userId, request.recipientId);
                                Client recipient = FindRecipient(request.recipientId);
                                Socket recipientSocket = recipient.ClientSocket;
                                recipientSocket.Send(Encoding.ASCII.GetBytes(request.userId + ": " + request.message));
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
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Disconnected Socket");
            }
            catch (Exception e)
            { // this exception will happen when "this" is be disposed...
                Console.WriteLine("receiveCallback fails with exception! " + e.ToString());
            }
        }

        public static List<string> recipientsIds = new List<string>();

        private static String PrintRecipients(List<string> res)
        {
            string text = "Recipient list";
            foreach(string r in res)
            {
                text += r + Environment.NewLine;
            }
            return text;
        }

        private static void RegisterClient(string userId, string publicKey)
        {
            spinner.Start();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect(3, 3);
            string request = ("{" +
                        "'request': 'REGISTE-USER'," +
                        "'userId': '" + userId + "'," +
                        "'publicKey': '" + publicKey + "'}");
            byte[] bytes = Encoding.ASCII.GetBytes(request);
            clientSocket.Send(bytes);
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
                        clientSocket.Send(bytesToSend);
                    }
                }
            } while (result.ToLower().Trim() != "exit");
        }

        private static void GetPeers()
        {
            spinner.Start();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect(3, 3);
            clientSocket.Send(Encoding.ASCII.GetBytes("{'request': 'GET-USERS'}"));
            spinner.Stop();

            string result = "";
            do
            {
                result = Console.ReadLine(); //you need to change this part
                if (result.ToLower().Trim() != "exit")
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(result); //Again, note that your data is actually of byte[], not string
                    //do something on bytes by using the reference such that you can type in HEX STRING but sending thing in bytes
                    clientSocket.Send(bytes);
                }
            } while (result.ToLower().Trim() != "exit");
        }

        static void LoopConnect(int noOfRetry, int attemptPeriodInSeconds)
        {
            int attempts = 0;
            while (!clientSocket.Connected && attempts < noOfRetry)
            {
                try
                {
                    ++attempts;
                    IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(SERVER_IP), PORT, EndConnectCallback, null);
                    result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(attemptPeriodInSeconds));
                    System.Threading.Thread.Sleep(attemptPeriodInSeconds * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.ToString());
                }
            }
            if (!clientSocket.Connected)
            {
                Console.WriteLine("Connection attempt is unsuccessful!");
                return;
            }
        }

        private static void EndConnectCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                if (clientSocket.Connected)
                {
                    clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), clientSocket);
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

        static int receiveAttemptClient = 0;

        private static void ReceiveCallbackClient(IAsyncResult result)
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
                        receiveAttemptClient = 0;
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
                    else if (receiveAttemptClient < MAX_RECEIVE_ATTEMPT)
                    { //not exceeding the max attempt, try again
                        ++receiveAttemptClient;
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), socket);
                    }
                    else
                    { //completely fails!
                        Console.WriteLine("receiveCallback is failed!");
                        receiveAttemptClient = 0;
                        clientSocket.Close();
                    }
                }
            }
            catch (Exception e)
            { // this exception will happen when "this" is be disposed...
                Console.WriteLine("receiveCallback is failed! " + e.ToString());
            }
        }
        
        static void Main(string[] args)
        {
            spinner = new Spinner(10, 10);
            terminalControls(args);
        }

        static void terminalControls(String[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "PMessenger";
            app.Description = "Securely comunicate with others.";

            app.ExtendedHelpText = "This console application allow the users to comunicate in a secue manner."
                + Environment.NewLine + "It allows multiple platforms such as linux, windows and MacOS to comunicate";

            // Shows the hep text
            app.HelpOption("-?|-h|--help");

            // OPTIONS
            var startServer = app.Option("-s|--server",
                "Initiates or stop the server", CommandOptionType.NoValue);
            var registerClient = app.Option("-c|--client",
                "Registers the client application", CommandOptionType.MultipleValue);
            var getClients = app.Option("-lc|--listclients",
                "List all connected clients", CommandOptionType.NoValue);
            var port = app.Option("-p|--port",
                "Set server listening port", CommandOptionType.SingleValue);
            var ip = app.Option("-i|--ip",
                "Set server listening ip address", CommandOptionType.SingleValue);
            var addRecipients = app.Option("-r|--recipient",
                "Add a recipient for the conversation", CommandOptionType.MultipleValue);

            // EXECUTION
            app.OnExecute(() =>
            {
                if (addRecipients.HasValue())
                {
                    foreach(string res in addRecipients.Values)
                    {
                        recipientsIds.Add(res);
                        Console.WriteLine(res);
                    }
                }

                if (getClients.HasValue())
                {
                    GetPeers();
                }

                if (port.HasValue())
                {
                    PORT = Convert.ToInt32(port.Value());
                }

                if (ip.HasValue())
                {
                    SERVER_IP = ip.Value();
                }

                if (startServer.HasValue())
                {
                    Console.WriteLine("Starting server...");
                    StartServer();
                }

                if (registerClient.HasValue())
                {
                    Console.WriteLine("Registering client");
                    RegisterClient(registerClient.Values[0], registerClient.Values[1]);
                }


                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
        }
    }

    //Request structure
    public class Request
    {
        public string request;
        public string userId = "";
        public string publicKey = "";
        public string message = "";
        public string recipientId = "";

        public Request(string request, string userId = "", string publicKey = "", string message = "", string recipientId = "")
        {
            this.request = request;
            this.userId = userId;
            this.publicKey = publicKey;
            this.message = message;
            this.recipientId = recipientId;
        }
    }
}
