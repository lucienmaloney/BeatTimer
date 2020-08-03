using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeatTimer
{
    // https://stackoverflow.com/questions/8754111/how-to-read-the-data-in-a-wav-file-to-an-array
    public class WavReader
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
                    audio[i] = (audio[i] + bytesToDouble(wav[pos], wav[pos + 1])) / 2;
                    pos += 2;
                }
                i++;
            }

            samplerate = bytesToDouble(wav[24], wav[25], wav[26], wav[27]);
        }
    }
}
