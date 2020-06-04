using System;
using System.Collections.Generic;
using System.Text;

namespace BeatTimer
{
    class Index
    {
        static void Main(string[] args)
        {
            var beats = BeatTimer.beatdata(args[0]);
            Console.WriteLine($"Getting beat data for {args[0]}...");
            foreach (var beat in beats)
            {
                Console.WriteLine(beat);
            }
        }
    }
}
