using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading;

namespace pmessenger
{
    class Program
    {
        static void Main(string[] args)
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

            // ADD ARGUMENT
            //var argOne = app.Argument("argOne", "App argument one");
            //var argTwo = app.Argument("argTwo", "App argument two");

            // EXECUTION
            app.OnExecute(() =>
            {

                if (startServer.HasValue())
                {
                    Console.WriteLine("Starting server");
                    var spinner = new Spinner(10, 10);

                    spinner.Start();
                    
                    Thread.Sleep(10000);

                    spinner.Stop();
                }
                else {
                    Console.WriteLine("Dont");
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
                Console.WriteLine("PMessenger executing...");
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
