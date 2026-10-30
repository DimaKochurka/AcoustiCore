using SpectrumAnalyzer.Services;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;

namespace AcoustiCore_DspExperiments.Experiments;

public static class FftSizeExperiment
{
    public static void Run()
    {
        const int sampleRate = 44100;
        const double amplitude = 0.5;

        const double minFrequency = 50.0;
        const double maxFrequency = 10000.0;
        const int frequencyCount = 200;

        int[] fftSizes = { 1024, 2048, 4096, 8192 };

        AudioService audioService = new()
        {
            SampleRate = sampleRate
        };

        string resultsDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "results"));

        Directory.CreateDirectory(resultsDirectory);

        string csvPath = Path.Combine(
            resultsDirectory, "fft_accuracy.csv");

        using StreamWriter writer = new StreamWriter(
            csvPath, false, Encoding.UTF8);

        writer.WriteLine(
            "fft_size,reference_hz,estimated_hz,absolute_error_hz");

        Console.WriteLine("AcoustiCore FFT Size Experiment\n");
        Console.WriteLine($"Sample rate: {sampleRate} Hz");
        Console.WriteLine($"Frequency range: {minFrequency} - {maxFrequency} Hz");
        Console.WriteLine($"Test frequencies: {frequencyCount}");
        Console.WriteLine();

        // Generate the same frequencies for every FFT size
        double[] testFrequencies = new double[frequencyCount];

        for (int i = 0; i < frequencyCount; i++)
        {
            testFrequencies[i] = minFrequency +
                i * (maxFrequency - minFrequency) /
                (frequencyCount - 1);
        }

        foreach (int fftSize in fftSizes)
        {
            audioService.FftSize = fftSize;

            double sumError = 0;

            foreach (double frequency in testFrequencies)
            {
                double[] signal = new double[fftSize];

                for (int n = 0; n < fftSize; n++)
                {
                    signal[n] = amplitude * Math.Sin(
                        2.0 * Math.PI * frequency * n / sampleRate);
                }

                double[] spectrum =
                    audioService.ProcessFft(signal, out _);

                double estimatedFrequency =
                    audioService.GetDominantFrequency(spectrum);

                double error = Math.Abs(
                    estimatedFrequency - frequency);

                sumError += error;

                writer.WriteLine(string.Join(",",
                    fftSize.ToString(CultureInfo.InvariantCulture),
                    frequency.ToString("F6", CultureInfo.InvariantCulture),
                    estimatedFrequency.ToString("F6", CultureInfo.InvariantCulture),
                    error.ToString("F6", CultureInfo.InvariantCulture)
                ));
            }

            double mae = sumError / frequencyCount;

            double frameDurationMs =
                1000.0 * fftSize / sampleRate;

            Console.WriteLine($"FFT size: {fftSize}");
            Console.WriteLine($"  Frame duration: {frameDurationMs:F2} ms");
            Console.WriteLine($"  MAE:            {mae:F4} Hz");
            Console.WriteLine();
        }

        Console.WriteLine($"CSV saved to: {csvPath}");

    }
}
