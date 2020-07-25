using System;

namespace BeatTimer
{
    class Index
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Getting beat data for {args[0]}...");
            var beats = BeatTimer.beatdata(args[0]);
            var fpdata = BeatParser.FPdata(beats);
            
            foreach (var fp in fpdata)
            {
                Console.WriteLine(fp);
            }
        }
    }
}
