using System.Diagnostics;
using System.Numerics;

namespace way.Services;

public class ImuDataProcessor
{
    private readonly int _sampleRate;
    private readonly int _windowSize;

    public ImuDataProcessor(int sampleRate = 100, int filterWindowSize = 5)
    {
        _sampleRate = sampleRate;
        _windowSize = filterWindowSize;
    }

    public async Task<ProcessedImuData> ProcessDataAsync(List<ImuData> rawData)
    {
        float[] x = new float[rawData.Count];
        float[] y = new float[rawData.Count];
        float[] z = new float[rawData.Count];

        Parallel.For(0, rawData.Count, i =>
        {
            x[i] = rawData[i].AccelX;
            y[i] = rawData[i].AccelY;
            z[i] = rawData[i].AccelZ;
        });

        // Параллельная обработка компонент
        Debug.WriteLine("ApplyAdaptiveFilterParallel");
        var filteredX = await Task.Run(() => ApplyAdaptiveFilterParallel(x));
        var filteredY = await Task.Run(() => ApplyAdaptiveFilterParallel(y));
        var filteredZ = await Task.Run(() => ApplyAdaptiveFilterParallel(z));

        Debug.WriteLine("DetrendSignal");
        var detrendedX = await Task.Run(() => DetrendSignal(filteredX));
        var detrendedY = await Task.Run(() => DetrendSignal(filteredY));
        var detrendedZ = await Task.Run(() => DetrendSignal(filteredZ));

        return new ProcessedImuData
        {
            FilteredX = filteredX,
            FilteredY = filteredY,
            FilteredZ = filteredZ
        };
    }

    private float[] ApplyAdaptiveFilterParallel(float[] data)
    {
        float[] filtered = new float[data.Length];
        int processorCount = Environment.ProcessorCount;
        int chunkSize = data.Length / processorCount;

        Parallel.For(0, processorCount, chunk =>
        {
            int start = chunk * chunkSize;
            int end = (chunk == processorCount - 1) ? data.Length : start + chunkSize;

            Span<float> windowBuffer = stackalloc float[_windowSize];

            for (int i = start; i < end; i++)
            {
                int windowStart = Math.Max(0, i - _windowSize / 2);
                int windowEnd = Math.Min(data.Length, i + _windowSize / 2 + 1);
                int windowSize = windowEnd - windowStart;

                var window = windowBuffer.Slice(0, windowSize);
                new Span<float>(data, windowStart, windowSize).CopyTo(window);

                if (IsOutlier(window, data[i]))
                {
                    filtered[i] = QuickSelectMedian(window);
                }
                else
                {
                    filtered[i] = VectorizedAverage(window);
                }
            }
        });

        return filtered;
    }

    private bool IsOutlier(Span<float> window, float value)
    {
        float median = QuickSelectMedian(window);
        float mad = MedianAbsoluteDeviation(window, median);
        return Math.Abs(value - median) > 3 * mad;
    }

    private float QuickSelectMedian(Span<float> span)
    {
        int k = span.Length / 2;
        QuickSelect(span, k);
        if (span.Length % 2 == 0)
        {
            float a = span[k];
            QuickSelect(span, k - 1);
            return (a + span[k - 1]) / 2;
        }
        return span[k];
    }

    private static void QuickSelect(Span<float> span, int k)
    {
        int left = 0;
        int right = span.Length - 1;

        while (left < right)
        {
            int pivotIndex = Partition(span, left, right);
            if (pivotIndex == k) break;
            if (pivotIndex < k) left = pivotIndex + 1;
            else right = pivotIndex - 1;
        }
    }

    private static int Partition(Span<float> span, int left, int right)
    {
        float pivot = span[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (span[j] <= pivot)
            {
                i++;
                (span[i], span[j]) = (span[j], span[i]);
            }
        }

        (span[i + 1], span[right]) = (span[right], span[i + 1]);
        return i + 1;
    }

    private float MedianAbsoluteDeviation(Span<float> window, float median)
    {
        Span<float> deviations = stackalloc float[window.Length];
        for (int i = 0; i < window.Length; i++)
        {
            deviations[i] = Math.Abs(window[i] - median);
        }
        return QuickSelectMedian(deviations);
    }

    private float VectorizedAverage(Span<float> window)
    {
        if (Vector.IsHardwareAccelerated && window.Length >= Vector<float>.Count * 2)
        {
            var sumVec = Vector<float>.Zero;
            int i = 0;
            for (; i <= window.Length - Vector<float>.Count; i += Vector<float>.Count)
            {
                sumVec += new Vector<float>(window.Slice(i));
            }
            float sum = Vector.Dot(sumVec, Vector<float>.One);
            for (; i < window.Length; i++) sum += window[i];
            return sum / window.Length;
        }
        else
        {
            float sum = 0;
            for (int i = 0; i < window.Length; i++) sum += window[i];
            return sum / window.Length;
        }
    }

    private float[] DetrendSignal(float[] signal)
    {
        float sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
        int n = signal.Length;

        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += signal[i];
            sumXY += i * signal[i];
            sumXX += i * i;
        }

        float slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        float intercept = (sumY - slope * sumX) / n;

        float[] detrended = new float[n];
        for (int i = 0; i < n; i++)
        {
            detrended[i] = signal[i] - (slope * i + intercept);
        }

        return detrended;
    }
}

public struct ProcessedImuData
{
    public float[] FilteredX { get; set; }
    public float[] FilteredY { get; set; }
    public float[] FilteredZ { get; set; }
}
