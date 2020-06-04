using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FFTWSharp;

namespace BeatTimer
{
    public struct Beat
    {
        public Beat(double time, double intensity, double prominence)
        {
            T = time;
            I = intensity;
            P = prominence;
        }

        public double T { get; }
        public double I { get; }
        public double P { get; }

        public override string ToString()
        {
            return $"Time: {T,10:0.00}, Intensity: {I,10:0.00}, Prominence: {P,10:0.00}";
        }
    }

    class BeatTimer
    {
        static void Main(string[] args)
        {
            var beats = beatdata(args[0]);
            Console.WriteLine($"Getting beat data for {args[0]}...");
            foreach (var beat in beats)
            {
                Console.WriteLine(beat);
            }
        }

        static Beat[] beatdata(String wavfilename)
        {
            WAV.readWav(wavfilename, out double[] data, out double samplerate);
            return beatdata(data, samplerate);
        }

        static Beat[] beatdata(double[] data, double samplerate)
        {
            int step = 128;
            int size = 2048;
            var spec = spectrogram(data, size, step);
            var bpm = getbpm(spec, samplerate, step);
            var rolling = rollingavg(spec, 5);
            var del = bpmtodel(bpm, samplerate, step);
            var indexes = beatindexes(rolling, del / 8);
            return beatdata(spec, indexes, samplerate, step);
        }

        static Beat[] beatdata(double[] spec, int[] indexes, double samplerate, int step)
        {
            int len = spec.Length;
            var beats = new List<Beat>();
            int previndex = 0;

            foreach (int index in indexes)
            {
                int lower = index - 5 >= 0 ? index - 5 : 0;
                int upper = index + 6 <= len ? index + 6 : len;
                var selection = spec[lower..upper];

                double time = indextotime(index, samplerate, step);
                double intensity = selection.Sum();
                double prominence = selection.Max() / (spec[previndex..(index + 1)].Min() + 1);
                beats.Add(new Beat(time, intensity, prominence));
                previndex = index;
            }
            return beats.ToArray();
        }

        static int[] beatindexes(double[] spec, double del)
        {
            var indexes = new List<int>();
            for (int x = 0; x + 2000 < spec.Length; x += 2000)
            {
                int upper = x + 4000 > spec.Length ? spec.Length : x + 2000;
                int firstindex = firstbeatindex(spec[x..upper], del) + x;
                int index = firstindex;
                int i = 0;
                while (index < upper)
                {
                    indexes.Add(index);
                    i++;
                    index = (int)Math.Round(firstindex + i * del);
                }
            }
            return indexes.ToArray();
        }

        static int firstbeatindex(double[] spec, double del)
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

        static double[] rollingavg(double[] arr, int radius)
        {
            double[] arr2 = new double[arr.Length];
            double sum = arr[0..radius].Sum();
            for (int i = 0; i < arr.Length; i++)
            {
                double subtract = (i - radius - 1) >= 0 ? arr[i - radius - 1] : 0;
                double add = (i + radius) < arr.Length ? arr[i + radius] : 0;
                sum = sum - subtract + add;
                arr2[i] = sum;
            }
            return arr2;
        }

        static double indextotime(int index, double samplerate, int step)
        {
            return (index + 18) * step / samplerate;
        }

        static double bpmtodel(double bpm, double samplerate, int step)
        {
            return samplerate * 60 / (step * bpm);
        }

        static double comb(double[] arr, int x)
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

        /// <summary>
        ///   Estimate the bpm to nearest integer of audio spectral flux data
        /// </summary>
        /// <param name="spec">Flux array: get using spectrogram method</param>
        /// <param name="samplerate">48000.0hz, 44100.0hz, etc</param>
        /// <param name="step">FFT increment</param>
        /// <returns>Estimated bpm</returns>
        public static double getbpm(double[] spec, double samplerate, int step)
        {
            int lower = (int)Math.Floor(bpmtodel(75, samplerate, step)) - 1;
            int upper = (int)Math.Ceiling(bpmtodel(15, samplerate, step)) + 1;

            int minindex = 0;
            double freqmin = double.MaxValue;

            double[] bins = new double[upper - lower + 1];
            for (int i = lower; i <= upper; i++)
            {
                bins[i - lower] = comb(spec, i);
            }

            for (int i = 1; i < upper - lower; i++)
            {
                if (bins[i] < bins[i - 1] && bins[i] < bins[i + 1] && bins[i] < freqmin)
                {
                    minindex = i;
                    freqmin = bins[i];
                }
            }

            double selectedbpm = bpmtodel(lower + minindex, samplerate, step);
            double bpm = selectedbpm * 8;
            return bpm > 400 ? Math.Round(bpm / 2) / 2 : Math.Round(bpm) / 2;
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
            double[,] ffts = new double[11,fftlength];
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
                for (int j = 0; j < fftlength * 2; j += 2)
                {
                    // Set to absolute value of complex number
                    ffts[index % 11, j / 2] = Math.Sqrt(fft[j] * fft[j] + fft[j + 1] * fft[j + 1]);
                }
                if (index >= 10)
                {
                    for (int j = 0; j < fftlength; j++)
                    {
                        double diff = ffts[index % 11, j] - ffts[(index - 10) % 11, j];
                        // Only add difference if positive. Results in better results
                        spec[index - 10] += diff > 0 ? diff : 0;
                    }
                }
            }
            fftw.destroy_plan(plan);
            fftw.free(ptr);
            fftw.cleanup();
            return spec;
        }
    }

    // https://stackoverflow.com/questions/8754111/how-to-read-the-data-in-a-wav-file-to-an-array
    public class WAV
    {
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }

        static double bytesToDouble(byte b1, byte b2, byte b3, byte b4)
        {
            return (double)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
        }

        public static void readWav(string filename, out double[] audio, out double samplerate)
        {
            byte[] wav = File.ReadAllBytes(filename);
            int channels = wav[22];
            int pos = 12;
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            int samples = (wav.Length - pos) / 2;
            if (channels == 2) samples /= 2;

            audio = new double[samples];

            int i = 0;
            while (pos + 4 < wav.Length)
            {
                audio[i] = bytesToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channels == 2)
                {
                    audio[i] += bytesToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }

            samplerate = bytesToDouble(wav[24], wav[25], wav[26], wav[27]);
        }
    }
}
