using SpectrumAnalyzer.Services;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace AcoustiCore_DspExperiments.Experiments;

public static class NoiseRobustnessExperiment
{
    public static void Run()
    {
        const int sampleRate = 44100;
        const int fftSize = 4096;
        const double frequency = 440.0;
        const double amplitude = 0.5;
        const int repetitions = 300;

        double[] snrLevels = { -10, -12, -14, -16, -18, -20, -22 };

        // Generate the reference sine wave
        double[] signal = new double[fftSize];

        for (int n = 0; n < fftSize; n++)
        {
            signal[n] = amplitude * Math.Sin(
                2.0 * Math.PI * frequency * n / sampleRate);
        }

        double signalRms = CalculateRms(signal);

        AudioService audioService = new()
        {
            SampleRate = sampleRate,
            FftSize = fftSize
        };

        string resultsDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "results"));

        Directory.CreateDirectory(resultsDirectory);

        string csvPath = Path.Combine(
            resultsDirectory, "noise_robustness.csv");

        using StreamWriter writer = new StreamWriter(
            csvPath, false, Encoding.UTF8);

        writer.WriteLine(
            "snr_db,seed,estimated_hz,absolute_error_hz,correct_bin");

        double binSpacing = (double)sampleRate / fftSize;

        int referenceBin =
            (int)Math.Round(frequency / binSpacing);

        Console.WriteLine("AcoustiCore Noise Robustness\n");
        Console.WriteLine($"Reference frequency: {frequency} Hz");
        Console.WriteLine($"FFT size: {fftSize}");
        Console.WriteLine($"Repetitions per SNR: {repetitions}");
        Console.WriteLine();

        foreach (double snrDb in snrLevels)
        {
            double sumError = 0;
            int correctBins = 0;

            for (int seed = 0; seed < repetitions; seed++)
            {
                Random random = new Random(1000 + seed);
                double[] noise = new double[fftSize];

                // Generate Gaussian noise using Box-Muller
                for (int n = 0; n < fftSize; n++)
                {
                    double u1 = Math.Max(
                        random.NextDouble(), 1e-12);

                    double u2 = random.NextDouble();

                    noise[n] = Math.Sqrt(-2.0 * Math.Log(u1))
                        * Math.Cos(2.0 * Math.PI * u2);
                }

                // Scale noise to the target SNR
                double noiseRms = CalculateRms(noise);

                double targetNoiseRms =
                    signalRms / Math.Pow(10.0, snrDb / 20.0);

                double scale = targetNoiseRms / noiseRms;

                double[] noisySignal = new double[fftSize];

                for (int n = 0; n < fftSize; n++)
                {
                    noisySignal[n] = signal[n] + noise[n] * scale;
                }

                // Process the noisy signal
                double[] spectrum =
                    audioService.ProcessFft(noisySignal, out _);

                double estimatedFrequency =
                    audioService.GetDominantFrequency(spectrum);

                double error = Math.Abs(
                    estimatedFrequency - frequency);

                int estimatedBin = (int)Math.Round(
                    estimatedFrequency / binSpacing);

                bool isCorrect = estimatedBin == referenceBin;

                if (isCorrect)
                    correctBins++;

                sumError += error;

                writer.WriteLine(string.Join(",",
                    snrDb.ToString("F0", CultureInfo.InvariantCulture),
                    (1000 + seed).ToString(CultureInfo.InvariantCulture),
                    estimatedFrequency.ToString("F6", CultureInfo.InvariantCulture),
                    error.ToString("F6", CultureInfo.InvariantCulture),
                    isCorrect ? "1" : "0"
                ));
            }

            double mae = sumError / repetitions;

            double correctRate =
                100.0 * correctBins / repetitions;

            Console.WriteLine(
                $"SNR: {snrDb,4:F0} dB | " +
                $"Correct bin rate: {correctRate,6:F1}% | " +
                $"MAE: {mae,8:F2} Hz");
        }

        Console.WriteLine();
        Console.WriteLine($"CSV saved to: {csvPath}");

    }

    private static double CalculateRms(double[] data)
    {
        double sumSquares = 0;

        foreach (double sample in data)
            sumSquares += sample * sample;

        return Math.Sqrt(sumSquares / data.Length);
    }
}
