using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace pmessenger
{
    class Program
    {
        static Spinner spinner;
        static int PORT = 10094;
        static string SERVER_IP = "127.0.0.1";
        static Socket serverSocket;

        private static void StartServer()
        {
            spinner.Start();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(100);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            spinner.Stop();

            Console.WriteLine("\nServer ip: {0}", SERVER_IP);
            Console.WriteLine("Server port: {0}", PORT);
            Console.WriteLine("SERVER STARTED | Displaying log...");

            string result = "";
            do
            {
                result = Console.ReadLine();
            } while (result.ToLower().Trim() != "exit");

        }

        private const int BUFFER_SIZE = 8192;
        private static byte[] buffer = new byte[BUFFER_SIZE];
        public static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = null;
            try
            {
                Socket socketClient = (Socket)result.AsyncState;
                Client client = new Client(socketClient, "test", "test");
                Clients.Add(client);

                foreach (Client client2 in Clients)
                {
                    Console.WriteLine(client2.SessionId);
                }

                socket = serverSocket.EndAccept(result);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e) {           
                Console.WriteLine(e.ToString());
            }
        }

        static public List<Client> Clients = new List<Client>();

        const int MAX_RECEIVE_ATTEMPT = 10;
        static int receiveAttempt = 0;
        public static void ReceiveCallback(IAsyncResult result)
        {

            foreach(Client client in Clients)
            {
                if (client.ClientSocket.Connected)
                {
                    string msg = "Hola tengo tu socket";
                    client.ClientSocket.Send(Encoding.ASCII.GetBytes(msg));
                }
            }

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
                        Console.WriteLine(Encoding.UTF8.GetString(data)); //Here I just print it, but you need to do something else                     

                        //Message retrieval part
                        //Suppose you only want to declare that you receive data from a client to that client
                        string msg = "Received: " + DateTime.Now;
                        socket.Send(Encoding.ASCII.GetBytes(msg));

                        receiveAttempt = 0; //reset receive attempt
                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket); //repeat beginReceive
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
            catch (Exception e)
            { // this exception will happen when "this" is be disposed...
                Console.WriteLine("receiveCallback fails with exception! " + e.ToString());
            }
        }

        static Socket clientSocket;

        private static void RegisterClient()
        {
            spinner.Start();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect(3, 3);
            spinner.Stop();
            Console.WriteLine("Connected");
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
                        Console.WriteLine("Server: " + Encoding.UTF8.GetString(data));
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

            // Shows the program version.
            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            });

            // OPTIONS
            var startServer = app.Option("-s|--server",
                "Initiates or stop the server", CommandOptionType.NoValue);
            var registerClient = app.Option("-c|--client",
                "Registers the client application", CommandOptionType.NoValue);
            var port = app.Option("-p|--port",
                "Set server listening port", CommandOptionType.SingleValue);
            var ip = app.Option("-i|--ip",
                "Set server listening ip address", CommandOptionType.SingleValue);

            // ADD ARGUMENT
            //var argOne = app.Argument("argOne", "App argument one");
            //var argTwo = app.Argument("argTwo", "App argument two");

            // EXECUTION
            app.OnExecute(() =>
            {

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
                    RegisterClient();
                }


                return 0;
            });

            // COMMANDS EXAMPLES
            /*app.Command("simple-command", (command) =>
            {
                //description and help text of the command.
                command.Description = "This is the description for simple-command.";
                command.ExtendedHelpText = "This is the extended help text for simple-command.";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
                {
                    Console.WriteLine("simple-command is executing");

                    //Do the command's work here, or via another object/method

                    Console.WriteLine("simple-command has finished.");
                    return 0; //return 0 on a successful execution
                });

            }
            );

            app.Command("complex-command", (command) =>
            {
                // This is a command that has it's own options.
                command.ExtendedHelpText = "This is the extended help text for complex-command.";
                command.Description = "This is the description for complex-command.";
                command.HelpOption("-?|-h|--help");

                // There are 3 possible option types:
                // NoValue
                // SingleValue
                // MultipleValue

                // MultipleValue options can be supplied as one or multiple arguments
                // e.g. -m valueOne -m valueTwo -m valueThree
                var multipleValueOption = command.Option("-m|--multiple-option <value>",
                    "A multiple-value option that can be specified multiple times",
                    CommandOptionType.MultipleValue);

                // SingleValue: A basic Option with a single value
                // e.g. -s sampleValue
                var singleValueOption = command.Option("-s|--single-option <value>",
                    "A basic single-value option",
                    CommandOptionType.SingleValue);

                // NoValue are basically booleans: true if supplied, false otherwise
                var booleanOption = command.Option("-b|--boolean-option",
                    "A true-false, no value option",
                    CommandOptionType.NoValue);

                command.OnExecute(() =>
                {
                    Console.WriteLine("complex-command is executing");

                    // Do the command's work here, or via another object/method                    

                    // Grab the values of the various options. when not specified, they will be null.

                    // The NoValue type has no Value property, just the HasValue() method.
                    bool booleanOptionValue = booleanOption.HasValue();

                    // MultipleValue returns a List<string>
                    List<string> multipleOptionValues = multipleValueOption.Values;

                    // SingleValue returns a single string
                    string singleOptionValue = singleValueOption.Value();

                    // Check if the various options have values and display them.
                    // Here we're checking HasValue() to see if there is a value before displaying the output.
                    // Alternatively, you could just handle nulls from the Value properties
                    if (booleanOption.HasValue())
                    {
                        Console.WriteLine("booleanOption option: {0}", booleanOptionValue.ToString());
                    }

                    if (multipleValueOption.HasValue())
                    {
                        Console.WriteLine("multipleValueOption option(s): {0}", string.Join(",", multipleOptionValues));
                    }

                    if (singleValueOption.HasValue())
                    {
                        Console.WriteLine("singleValueOption option: {0}", singleOptionValue ?? "null");
                    }

                    Console.WriteLine("complex-command has finished.");
                    return 0; // return 0 on a successful execution
                });
            });*/

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

    public class Spinner : IDisposable
    {
        private const string Sequence = @"#";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public Spinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        public void Start()
        {
            Draw('[');
            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            active = false;
            Draw(']');
            Draw('\n');
        }

        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        private void Draw(char c)
        {
            Console.Write(c);
        }

        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
