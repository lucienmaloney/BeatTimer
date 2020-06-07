using System;
using System.Collections.Generic;

namespace Kinemotik
{
    public struct RangeSelection<T>
    {
        private IList<T> _list;
        private int _start;
        private int _end;

        public int Start { get => _start; }
        public int End { get => _end; }
        public IList<T> List { get => _list; }

        public T this[int key]
        {
            get => _list[key];
            set => _list[key] = value;
        }

        public RangeSelection(IList<T> list, int start = 0, int end = int.MaxValue)
        {
            _list = list;
            _start = Math.Max(0, start);
            _end = Math.Min(list.Count - 1, end);
        }
    }

    public static class RangeSelectionExtensions
    {
        public static RangeSelection<T> RangeSelect<T>(this IList<T> list, int start, int end)
        {
            return new RangeSelection<T>(list, start, end);
        }

        #region Sum
        public static float Sum(this RangeSelection<float> r)
        {
            float sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static double Sum(this RangeSelection<double> r)
        {
            double sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static decimal Sum(this RangeSelection<decimal> r)
        {
            decimal sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static int Sum(this RangeSelection<int> r)
        {
            int sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static uint Sum(this RangeSelection<uint> r)
        {
            uint sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static long Sum(this RangeSelection<long> r)
        {
            long sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static ulong Sum(this RangeSelection<ulong> r)
        {
            ulong sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static short Sum(this RangeSelection<short> r)
        {
            short sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static ushort Sum(this RangeSelection<ushort> r)
        {
            ushort sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static sbyte Sum(this RangeSelection<sbyte> r)
        {
            sbyte sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }

        public static byte Sum(this RangeSelection<byte> r)
        {
            byte sum = 0;

            for (int i = r.Start; i <= r.End; ++i)
                sum += r[i];

            return sum;
        }
        #endregion

        #region Min
        public static float Min(this RangeSelection<float> r)
        {
            float min = float.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static double Min(this RangeSelection<double> r)
        {
            double min = double.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static decimal Min(this RangeSelection<decimal> r)
        {
            decimal min = decimal.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static int Min(this RangeSelection<int> r)
        {
            int min = int.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static uint Min(this RangeSelection<uint> r)
        {
            uint min = uint.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static long Min(this RangeSelection<long> r)
        {
            long min = long.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static ulong Min(this RangeSelection<ulong> r)
        {
            ulong min = ulong.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static short Min(this RangeSelection<short> r)
        {
            short min = short.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static ushort Min(this RangeSelection<ushort> r)
        {
            ushort min = ushort.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static sbyte Min(this RangeSelection<sbyte> r)
        {
            sbyte min = sbyte.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }

        public static byte Min(this RangeSelection<byte> r)
        {
            byte min = byte.MaxValue;

            for (int i = r.Start; i <= r.End; ++i)
                min = Math.Min(min, r[i]);

            return min;
        }
        #endregion

        #region Max
        public static float Max(this RangeSelection<float> r)
        {
            float max = float.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static double Max(this RangeSelection<double> r)
        {
            double max = double.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static decimal Max(this RangeSelection<decimal> r)
        {
            decimal max = decimal.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static int Max(this RangeSelection<int> r)
        {
            int max = int.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static uint Max(this RangeSelection<uint> r)
        {
            uint max = uint.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static long Max(this RangeSelection<long> r)
        {
            long max = long.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static ulong Max(this RangeSelection<ulong> r)
        {
            ulong max = ulong.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static short Max(this RangeSelection<short> r)
        {
            short max = short.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static ushort Max(this RangeSelection<ushort> r)
        {
            ushort max = ushort.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static sbyte Max(this RangeSelection<sbyte> r)
        {
            sbyte max = sbyte.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }

        public static byte Max(this RangeSelection<byte> r)
        {
            byte max = byte.MinValue;

            for (int i = r.Start; i <= r.End; ++i)
                max = Math.Max(max, r[i]);

            return max;
        }
        #endregion
    }
}