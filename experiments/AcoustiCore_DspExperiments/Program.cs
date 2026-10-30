
using AcoustiCore_DspExperiments.Experiments;

Console.WriteLine("AcoustiCore DSP Experiments\n");
Console.WriteLine("1. FFT Size Experiment");
Console.WriteLine("2. Noise Robustness Experiment");

Console.Write("\nSelect experiment: ");
string? choice = Console.ReadLine();

switch (choice)
{
    case "1":
        FftSizeExperiment.Run();
        break;

    case "2":
        NoiseRobustnessExperiment.Run();
        break;

    default:
        Console.WriteLine("Invalid selection.");
        break;
}
