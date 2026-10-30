
from pathlib import Path

import pandas as pd
import matplotlib.pyplot as plt


# File paths
project_dir = Path(__file__).resolve().parent.parent
results_dir = project_dir / "results"
output_dir = project_dir / "plots" / "output"
output_dir.mkdir(parents=True, exist_ok=True)

# Plot style
plt.rcParams.update({
    "font.size": 11,
    "axes.titlesize": 14,
    "axes.labelsize": 11,
    "figure.dpi": 120,
})


def save_plot(fig, filename):
    fig.tight_layout()
    fig.savefig(output_dir / filename, dpi=300, bbox_inches="tight")


def add_labels(ax, x, y, fmt, offset=8):
    for xi, yi in zip(x, y):
        ax.annotate(
            fmt.format(yi),
            (xi, yi),
            xytext=(0, offset),
            textcoords="offset points",
            ha="center",
            fontsize=10
        )


# FFT size experiment
fft_df = pd.read_csv(results_dir / "fft_accuracy.csv")

fft_summary = (
    fft_df.groupby("fft_size")["absolute_error_hz"]
    .mean()
    .reset_index()
    .sort_values("fft_size")
)

fft_sizes = fft_summary["fft_size"]
fft_mae = fft_summary["absolute_error_hz"]
positions = range(len(fft_sizes))

fig1, ax1 = plt.subplots(figsize=(8, 5))

ax1.plot(positions, fft_mae, marker="o", linewidth=2)
add_labels(ax1, positions, fft_mae, "{:.2f} Hz")

ax1.set_title("FFT Size vs Frequency Estimation Error")
ax1.set_xlabel("FFT Size (samples)")
ax1.set_ylabel("Mean Absolute Error (Hz)")
ax1.set_xticks(list(positions), fft_sizes)
ax1.set_ylim(0, fft_mae.max() * 1.2)
ax1.grid(True, alpha=0.3)

save_plot(fig1, "fft_accuracy.png")


# Noise robustness experiment
noise_df = pd.read_csv(results_dir / "noise_robustness.csv")

noise_summary = (
    noise_df.groupby("snr_db")
    .agg(
        correct_bin_rate=("correct_bin", "mean"),
        mae_hz=("absolute_error_hz", "mean")
    )
    .reset_index()
    .sort_values("snr_db")
)

noise_summary["correct_bin_rate"] *= 100

snr = noise_summary["snr_db"]
correct_rate = noise_summary["correct_bin_rate"]
noise_mae = noise_summary["mae_hz"]


# Correct bin rate
fig2, ax2 = plt.subplots(figsize=(8, 5))

ax2.plot(snr, correct_rate, marker="o", linewidth=2)
add_labels(ax2, snr, correct_rate, "{:.1f}%", offset=-18)

ax2.set_title("Noise Robustness: Correct Bin Rate")
ax2.set_xlabel("Signal-to-Noise Ratio (dB)")
ax2.set_ylabel("Correct Bin Rate (%)")
ax2.set_xticks(snr)
ax2.set_ylim(0, 110)
ax2.grid(True, alpha=0.3)

save_plot(fig2, "noise_correct_bin_rate.png")


# Frequency estimation error
fig3, ax3 = plt.subplots(figsize=(8, 5))

ax3.plot(snr, noise_mae, marker="o", linewidth=2)
add_labels(ax3, snr, noise_mae, "{:.2f} Hz")

ax3.set_title("Noise Robustness: Frequency Estimation Error")
ax3.set_xlabel("Signal-to-Noise Ratio (dB)")
ax3.set_ylabel("Mean Absolute Error (Hz)")
ax3.set_xticks(snr)
ax3.set_ylim(0, noise_mae.max() * 1.2)
ax3.grid(True, alpha=0.3)

save_plot(fig3, "noise_mae.png")


# Summary
print("\nFFT Size Experiment:")
print(fft_summary.to_string(index=False))

print("\nNoise Robustness Experiment:")
print(noise_summary.to_string(index=False))

print(f"\nPlots saved to: {output_dir}")

plt.show()
