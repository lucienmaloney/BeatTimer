using System;
using System.Collections.Generic;

namespace BeatTimer {
    public struct FP
    {
        public FP(double time, int f, int p)
        {
            T = time;
            F = f;
            P = p;
        }

        public FP(double time, double f, double p) {
            T = time;
            F = f < 350 ? 0 : f < 1000 ? 1 : 2;
            P = p < 5000 ? 0 : p < 8000 ? 1 : 2;
        }

        public double T { get; }
        public int F { get; }
        public int P { get; }

        public override string ToString()
        {
            return $"Time: {T,10:0.00}, Freneticism: {F,10}, Physicality: {P,10}";
        }
    }

    class BeatParser {
        public static List<FP> FPdata(List<Beat> beats) {
            var fp = new List<FP>();
            for (int i = 0; i < beats.Count; i += 64) {
                double p = 0;
                double f = 0;
                double time = beats[i].T;
                double prev = 0;

                int max = Math.Min(64, beats.Count - i);
                for (int j = 0; j < max; j++) {
                    p += Math.Sqrt(beats[i + j].I);
                    f += Math.Abs(beats[i + j].P - prev);
                    prev = beats[i + j].P;
                }

                fp.Add(new FP(time, f, p));
            }
            return fp;
        }
    }
}