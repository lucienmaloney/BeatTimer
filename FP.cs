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

/*
        public FP(double time, double f, double p) {
            double paverage = 51275.34475664618;
            double pdeviation = 23120.601566104728;
            double faverage = 1466585.6747473634;
            double fdeviation = 816264.2562507965;

            T = time;
            F = f > faverage + fdeviation / 2 ? 2 : f > faverage - fdeviation / 2 ? 1 : 0;
            P = p > paverage + pdeviation / 2 ? 2 : p > paverage - pdeviation / 2 ? 1 : 0;
        }*/

        public double T { get; }
        public int F { get; }
        public int P { get; }

        public override string ToString()
        {
            return $"Time: {T,10:0.00}, Freneticism: {F,10}, Physicality: {P,10}";
        }
    }

    class BeatParser {
        // Average physicality and freneticism calculated by a sampling and averaging a collection of many different songs
        private static double paverageaverage = 51275.34475664618;
        private static double pdeviationaverage = 23120.601566104728;
        private static double faverageaverage = 1466585.6747473634;
        private static double fdeviationaverage = 816264.2562507965;
        
        public static List<FP> FPdata(List<Beat> beats) {
            var fp = new List<FP>();
            var times = new List<double>();
            var physicalities = new List<double>();
            var freneticisms = new List<double>();
            
            for (int i = 0; i < beats.Count; i += 64) {
                double p = 0;
                double f = 0;

                int max = Math.Min(i + 64, beats.Count);
                int min = max - 64;
                for (int j = max - 1; j >= min; j--) {
                    p += beats[j].A;
                    f += beats[j].I;
                }

                physicalities.Add(p);
                freneticisms.Add(f);
                times.Add(beats[i].T);
            }

            stddev(physicalities, out double pdev, out double paverage);
            stddev(freneticisms, out double fdev, out double faverage);
            pdev = (pdev + pdeviationaverage) / 2;
            paverage = (paverage + paverageaverage) / 2;
            fdev = (fdev + fdeviationaverage) / 2;
            faverage = (faverage + faverageaverage) / 2;

            for (int i = 0; i < times.Count; i++) {
                int pvalue = physicalities[i] > paverage + pdev / 2 ? 2 : physicalities[i] > paverage - pdev / 2 ? 1 : 0;
                int fvalue = freneticisms[i]  > faverage + fdev / 2 ? 2 : freneticisms[i]  > faverage - fdev / 2 ? 1 : 0;
                fp.Add(new FP(times[i], fvalue, pvalue));
            }

            return fp;
        }

        // https://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list
        public static void stddev(List<double> values, out double deviation, out double average) {
            double sum = 0;
            for (int i = 0; i < values.Count; i++) {
                sum += values[i];
            }
            average = sum / values.Count;
            sum = 0;
            for (int i = 0; i < values.Count; i++) {
                sum += Math.Pow(values[i] - average, 2);
            }
            deviation = Math.Sqrt(sum / (values.Count - 1));
        }
    }
}