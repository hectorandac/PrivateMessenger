using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace pmessenger
{

    //Spinner animation for loading
    public class Spinner : IDisposable
    {
        private const string Sequence = @"#";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        //Constructor
        public Spinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        //Starts the spinner
        public void Start()
        {
            Draw('[');
            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        //Stops the spinner
        public void Stop()
        {
            active = false;
            Draw(']');
            Draw('\n');
        }

        //Draw char and add delay
        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        //Draw char on terminal
        private void Draw(char c)
        {
            Console.Write(c);
        }

        //Make changes to terminal color and select the char to draw
        private void Turn()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Draw(Sequence[++counter % Sequence.Length]);
            Console.ResetColor();
        }

        //Stops the spinner;
        public void Dispose()
        {
            Stop();
        }
    }
}
