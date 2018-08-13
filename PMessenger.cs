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
        // Spinner that indicates loading
        static Spinner spinner;
        
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

            app.Command("client", (command) =>
            {

                var user = command.Option("-u|--user",
                    "Sets the client userId", CommandOptionType.SingleValue);
                var getClients = command.Option("-lc|--listClients",
                    "List all connected clients", CommandOptionType.NoValue);
                var addRecipients = command.Option("-r|--recipient",
                    "Add a recipient for the conversation", CommandOptionType.MultipleValue);

                var port = command.Option("-p|--port",
                    "Set server listening port", CommandOptionType.SingleValue);
                var ip = command.Option("-i|--ip",
                    "Set server listening ip address", CommandOptionType.SingleValue);

                // This is a command that has it's own options.
                command.ExtendedHelpText = "Client side appliction for client comunication";
                command.Description = "This side is needed for comunicating with another client in the server";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
                {
                    Client client = new Client(null, user.Value());

                    if (addRecipients.HasValue())
                    {
                        foreach (string res in addRecipients.Values)
                        {
                            client.recipientsIds.Add(res);
                        }
                    }


                    if (port.HasValue())
                    {
                        client.PORT = Convert.ToInt32(port.Value());
                    }

                    if (ip.HasValue())
                    {
                        client.SERVER_IP = ip.Value();
                    }

                    if (getClients.HasValue())
                    {
                        client.GetPeers();
                    }
                    else
                    {
                        client.RegisterClient(client.SessionId);
                    }
                    return 0;
                });
            });

            app.Command("server", (command) =>
            {
                // This is a command that has it's own options.
                command.ExtendedHelpText = "Server side application for managing the clients messages";
                command.Description = "This side of the application helps the clients comunicate with each other, this works as a mediator.";
                command.HelpOption("-?|-h|--help");

                var port = command.Option("-p|--port",
                    "Set server listening port", CommandOptionType.SingleValue);
                var ip = command.Option("-i|--ip",
                    "Set server listening ip address", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {

                    Server server = new Server();
                    if (port.HasValue())
                    {
                        server.PORT = Convert.ToInt32(port.Value());
                    }

                    if (ip.HasValue())
                    {
                        server.SERVER_IP = ip.Value();
                    }

                    server.StartServer();
                    return 0;
                });
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
}
