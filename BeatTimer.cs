using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FFTWSharp;
using Kinemotik;

namespace BeatTimer
{
    public struct Beat
    {
        public Beat(double time, int section, int beat, double intensity, double prominence)
        {
            T = time;
            I = intensity;
            P = prominence;
            S = section;
            B = beat;
        }

        public double T { get; }
        public double I { get; }
        public double P { get; }
        public int S { get; }
        public int B { get; }

        public override string ToString()
        {
            return $"Time: {T,10:0.000000}, Section: {S,3}, Beat: {B,5} Intensity: {I,10:0.00}, Prominence: {P,10:0.00}";
        }
    }

    class BeatTimer
    {
#if !UNITY_5_3_OR_NEWER
        public static List<Beat> beatdata(String wavfilename)
        {
            WavReader.readWav(wavfilename, out double[] data, out double samplerate);
            return beatdata(data, samplerate);
        }
#endif
        public static List<Beat> beatdata(double[] data, double samplerate)
        {
            int step = 128;
            int size = 2048;
            var spec = spectrogram(data, size, step);
            var bpm = getbpm(spec, samplerate, step);
            var rolling = rollingsum(spec, 5);
            var del = bpmtodel(bpm, samplerate, step);
            var indexes = beatindexes(rolling, del / 8);
            return beatdata(spec, indexes, samplerate, step, bpm);
        }

        /// <summary>
        ///   Get beat data (intensity and prominence) for each beat from their indexes
        /// </summary>
        /// <param name="spec">Spectrogram data</param>
        /// <param name="indexes">Beat start indexes</param>
        /// <param name="samplerate">48000.0hz, 44100.0hz, etc</param>
        /// <param name="step">FFT increment</param>
        /// <returns>Beat data array</returns>
        public static List<Beat> beatdata(double[] spec, int[] indexes, double samplerate, int step, double bpm)
        {
            var beats = new List<Beat>();
            double starttime = indextotime(indexes[0], samplerate, step);
            double deltatime = 60 / (bpm * 8);

            int section = 0;
            int beat = 0;
            int previndex = 0;

            foreach (int index in indexes)
            {
                var selection = spec.RangeSelect(index - 5, index + 5);

                double indextime = indextotime(index, samplerate, step);
                double time = starttime + beat * deltatime;
                if (time - indextime < deltatime * 0.5)
                {
                    if (Math.Abs(time - indextime) > 0.005)
                    {
                        beat = 0;
                        section++;
                        starttime = indextime;
                        time = indextime;
                    }

                    // Only add the beat if the time delta is more than half of our previously established delta:

                    // Intensity is total sum of audio in short range
                    double intensity = selection.Sum();
                    // Prominence is ratio between max in short range to min of long preceeding range
                    double prominence = selection.Max() / (spec.RangeSelect(previndex, index).Min() + 1);

                    beats.Add(new Beat(time, section, beat, intensity, prominence));
                    beat++;
                    previndex = index;
                }
            }

            // Find first instance of whole beat and chop off preceeding notes so that first beat is whole beat
            var intensities = beats.ConvertAll(new Converter<Beat, double>((Beat b) => b.I));
            var wholebeatindex = firstbeatindex(intensities.RangeSelect(0, intensities.Count - 1), 8);
            beats.RemoveRange(0, wholebeatindex);
            return beats;
        }

        /// <summary>
        ///   Get the eighth-note beat times from a given spectrogram and bpm index delta
        /// </summary>
        /// <param name="spec">Spectrogram data</param>
        /// <param name="del">How far between beats</param>
        /// <returns>Beat start indexes</returns>
        public static int[] beatindexes(double[] spec, double del)
        {
            var indexes = new List<int>();
            // Readjust the starting point every 1000 samples (roughly every 2.5 seconds)
            // This is to counteract tempo drift due to changing bpm's or other factors
            for (int x = 0; x + 2000 < spec.Length; x += 1000)
            {
                int upper = x + 3000 > spec.Length ? spec.Length : x + 3000;
                int limit = upper == spec.Length ? upper : x + 1000;
                int firstindex = firstbeatindex(spec.RangeSelect(x, upper - 1), del) + x;
                int index = firstindex;
                int i = 0;
                while (index < limit)
                {
                    indexes.Add(index);
                    i++;
                    index = (int)Math.Round(firstindex + i * del);
                }
            }
            return indexes.ToArray();
        }

        /// <summary>
        ///   The index of the first beat determined by which starting point produces a sequence of highest intensity
        /// </summary>
        /// <param name="spec">Spectrogram data</param>
        /// <param name="del">How far between beats</param>
        /// <returns>First beat index</returns>
        public static int firstbeatindex(RangeSelection<double> spec, double del)
        {
            int highestindex = 0;
            double highestsum = 0;
            int len = spec.Length;

            for (int i = 0; i < del; i++)
            {
                double sum = 0;
                int index = i;
                int x = 0;

                while (index < len)
                {
                    sum += spec[index];
                    x++;
                    index = (int)Math.Round(i + x * del);
                }

                if (sum > highestsum)
                {
                    highestsum = sum;
                    highestindex = i;
                }
            }
            return highestindex;
        }

        /// <summary>
        ///   Get sum at each index of itself and surrounding values up to and including radius
        /// </summary>
        /// <param name="arr">Data array</param>
        /// <param name="radius">How far to sum. 0 will get just the index. 1 will get index and its 2 neighbors</param>
        /// <returns>Data array rolled</returns>
        public static double[] rollingsum(double[] arr, int radius)
        {
            double[] arr2 = new double[arr.Length];
            double sum = arr.RangeSelect(0, radius - 1).Sum();
            for (int i = 0; i < arr.Length; i++)
            {
                // Don't go out of bounds now
                double subtract = (i - radius - 1) >= 0 ? arr[i - radius - 1] : 0;
                double add = (i + radius) < arr.Length ? arr[i + radius] : 0;
                sum = sum - subtract + add;
                arr2[i] = sum;
            }
            return arr2;
        }

        public static double indextotime(int index, double samplerate, int step)
        {
            return (index + 18) * step / samplerate;
        }

        public static double bpmtodel(double bpm, double samplerate, int step)
        {
            return samplerate * 60 / (step * bpm);
        }

        /// <summary>
        ///   Determine the total difference between unmodified wave and itself shifted x units
        /// </summary>
        /// <param name="arr">Audio data mainly, but could be any data with periodic repetitions</param>
        /// <param name="x">Number of indices to shift when comparing</param>
        /// <returns>Total "cost" of the shift. The lower the more similar</returns>
        public static double comb(double[] arr, int x)
        {
            double sum = 0;
            int count = 0;
            for (int i = 0; i + x < arr.Length; i++)
            {
                sum += Math.Abs(arr[i] - arr[i + x]);
                count++;
            }
            return sum / count;
        }

        public static double comb(double[] arr, double x) {
            int lower = (int)Math.Floor(x);
            int upper = (int)Math.Ceiling(x);
            if (upper == lower) {
                return comb(arr, lower);
            }
            return (upper - x) * comb(arr, lower) + (x - lower) * comb(arr, upper);
        }

        /// <summary>
        ///   Estimate the bpm to nearest integer of audio spectral flux data
        /// </summary>
        /// <param name="spec">Flux array: get using spectrogram method</param>
        /// <param name="samplerate">48000.0hz, 44100.0hz, etc</param>
        /// <param name="step">FFT increment</param>
        /// <returns>Estimated bpm</returns>
        public static double getbpm(double[] spec, double samplerate, int step, int lowerbound = 70, int upperbound = 170)
        {
            int minindex = 0;
            double freqmin = double.MaxValue;

            double[] bins = new double[upperbound - lowerbound + 3];
            for (int i = lowerbound - 1; i <= upperbound + 1; i++) {
                double del = bpmtodel(i, samplerate, step);
                bins[i + 1 - lowerbound] = comb(spec, del) + comb(spec, del * 2) + comb(spec, del * 4);
            }

            for (int i = 1; i < upperbound - lowerbound + 1; i++)
            {
                if (bins[i] < bins[i - 1] && bins[i] < bins[i + 1] && bins[i] < freqmin)
                {
                    minindex = i;
                    freqmin = bins[i];
                }
            }

            return minindex + lowerbound - 1;
        }


        /// <summary>
        ///   Calculates the spectral flux of a real double array
        ///   <example>
        ///     <code>
        ///       var specflux = spectrogram(somedata, 4096, 1024);
        ///     </code>
        ///   </example>
        /// </summary>
        /// <param name="arr">Any real data</param>
        /// <param name="size">FFT length (power of 2)</param>
        /// <param name="step">How much to increment for start of following FFT</param>
        /// <returns>Array reprenting change in sum of frequencies over time</returns>
        public static double[] spectrogram(double[] arr, int size, int step)
        {
            int n = arr.Length;
            // FFT of real values is symmetric, so we only need an array half the length + 1
            int fftlength = size / 2 + 1;
            // The number of groupings of size shifted step apart you can fit in n, minus 10
            int speclength = (n - size + step) / step - 10;

            double[] spec = new double[speclength];
            double[,] ffts = new double[11, fftlength];
            // FFT is returned as complex number with every other index being imaginary, so double length
            double[] fft = new double[fftlength * 2];

            IntPtr ptr = fftw.malloc(size * sizeof(double));
            IntPtr ptrout = fftw.malloc(fftlength * 2 * sizeof(double));
            IntPtr plan = fftw.dft_r2c_1d(size, ptr, ptrout, fftw_flags.Estimate);

            for (int i = 0; i + size + step < n; i += step)
            {
                int index = i / step;
                Marshal.Copy(arr, i, ptr, size);
                fftw.execute(plan);
                Marshal.Copy(ptrout, fft, 0, fftlength * 2);

                int modindex = index % 11;
                int minusindex = index - 10;
                int modminusindex = minusindex % 11;

                for (int j = 0; j < fftlength * 2; j += 2)
                {
                    // Set to absolute value of complex number
                    ffts[modindex, j / 2] = Math.Sqrt(fft[j] * fft[j] + fft[j + 1] * fft[j + 1]);
                }
                if (index >= 10)
                {
                    double diff = 0;
                    double sum = 0;
                    for (int j = 0; j < fftlength; j++)
                    {
                        diff = ffts[modindex, j] - ffts[modminusindex, j];
                        // Only add difference if positive. Results in better results
                        sum += diff > 0 ? diff : 0;
                    }
                    spec[minusindex] = sum;
                }
            }
            fftw.destroy_plan(plan);
            fftw.free(ptr);
            fftw.cleanup();
            return spec;
        }
    }
}
