\# DSP Experiments



\## Overview



This directory contains two small experiments evaluating the FFT-based dominant frequency estimation used in AcoustiCore. Both experiments use synthetic audio signals to measure frequency estimation accuracy and robustness to noise under controlled conditions.



\## FFT Size Evaluation



Larger FFT sizes provide finer frequency-bin spacing but require longer audio frames. Although this relationship is well established, the experiment measures its practical effect on AcoustiCore's implementation and provides quantitative support for the selected FFT size of 4096.



A total of 200 sine waves between 50 Hz and 10 kHz were analyzed at a 44.1 kHz sample rate using four FFT sizes. Accuracy was evaluated using Mean Absolute Error (MAE).



| FFT Size | Frame Duration | MAE |

| --- | --- | --- |

| 1024 | 23.22 ms | 10.75 Hz |

| 2048 | 46.44 ms | 5.41 Hz |

| 4096 | 92.88 ms | 2.70 Hz |

| 8192 | 185.76 ms | 1.35 Hz |



!\[FFT size evaluation](AcoustiCore\_DspExperiments/plots/output/fft\_accuracy.png)



FFT 4096 was retained as a practical compromise between frequency estimation accuracy and audio frame duration. Increasing the size to 8192 reduced MAE by approximately 1.35 Hz while doubling the frame duration.



\## Noise Robustness



The second experiment evaluates dominant frequency estimation in the presence of additive Gaussian white noise.



A 440 Hz sine wave was tested at seven signal-to-noise ratios (SNR), from −10 dB to −22 dB, using FFT 4096. Each SNR level was tested with 300 reproducible noise realizations.



| SNR | Correct Bin Rate | MAE |

| --- | --- | --- |

| −10 dB | 100.0% | 1.43 Hz |

| −12 dB | 100.0% | 1.43 Hz |

| −14 dB | 100.0% | 1.43 Hz |

| −16 dB | 100.0% | 1.43 Hz |

| −18 dB | 98.3% | 89.91 Hz |

| −20 dB | 84.0% | 1501.71 Hz |

| −22 dB | 51.7% | 5104.30 Hz |



!\[Correct bin rate](AcoustiCore\_DspExperiments/plots/output/noise\_correct\_bin\_rate.png)



!\[Frequency estimation error](AcoustiCore\_DspExperiments/plots/output/noise\_mae.png)



The algorithm correctly identified the reference frequency bin in all 300 trials per SNR level down to −16 dB. Accuracy decreased at lower SNR levels as noise increasingly dominated the spectrum.



## Experiment Files

The experiments were implemented in C#/.NET 8 using AcoustiCore's existing audio processing methods. The repository includes the experiment source code, CSV measurement data, and a Python script used to generate the figures.

The experiments use synthetic signals and do not evaluate microphone performance, sound-source classification, or end-to-end system latency.