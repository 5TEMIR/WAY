using System.Diagnostics;

namespace way.Services;

public class ExerciseAnalyzer
{
    private readonly int _sampleRate;

    public ExerciseAnalyzer(int sampleRate = 100)
    {
        _sampleRate = sampleRate;
    }

    public async Task<ExerciseAnalysis> AnalyzeExerciseAsync(ProcessedImuData data)
    {
        // 1. Анализ периодичности по каждой оси
        var xPeriodTask = Task.Run(() => FindMainPeriodAsync(data.FilteredX));
        var yPeriodTask = Task.Run(() => FindMainPeriodAsync(data.FilteredY));
        var zPeriodTask = Task.Run(() => FindMainPeriodAsync(data.FilteredZ));

        var periods = await Task.WhenAll(xPeriodTask, yPeriodTask, zPeriodTask);
        Debug.WriteLine($"Periods - X: {periods[0]}, Y: {periods[1]}, Z: {periods[2]}");

        // 2. Определяем основную ось упражнения
        var mainAxis = await DetermineMainAxisAsync(periods[0], periods[1], periods[2],
                                                  data.FilteredX, data.FilteredY, data.FilteredZ);

        // 3. Находим повторения по основной оси
        var reps = await FindRepetitionsAsync(mainAxis.Signal, mainAxis.Period);

        // 4. Группируем повторения в подходы
        var sets = GroupIntoSets(reps, mainAxis.Signal);

        var analysis = new ExerciseAnalysis
        {
            Repetitions = reps,
            Sets = sets,
            MainAxis = mainAxis.AxisName,
            DominantPeriod = mainAxis.Period / (float)_sampleRate,
            AverageRestTime = CalculateAverageRestTime(sets, mainAxis.Signal, _sampleRate)
        };

        Debug.WriteLine($"Average rest time: {analysis.AverageRestTime} seconds");
        return analysis;
    }

    private async Task<AxisAnalysis> DetermineMainAxisAsync(int xPeriod, int yPeriod, int zPeriod,
                                                          float[] x, float[] y, float[] z)
    {
        return await Task.Run(() =>
        {
            var axes = new[]
            {
                new { Period = xPeriod, Signal = x, AxisName = "X" },
                new { Period = yPeriod, Signal = y, AxisName = "Y" },
                new { Period = zPeriod, Signal = z, AxisName = "Z" }
            };

            return axes.OrderByDescending(a => a.Period)
                      .Select(a => new AxisAnalysis
                      {
                          Period = a.Period,
                          Signal = a.Signal,
                          AxisName = a.AxisName
                      })
                      .First();
        });
    }

    private async Task<int> FindMainPeriodAsync(float[] signal)
    {
        return await Task.Run(() =>
        {
            var autocorr = ComputeAutocorrelation(signal);

            int searchStart = Math.Min(signal.Length, (int)(0.5 * _sampleRate));
            int searchEnd = Math.Min(autocorr.Length, 5 * _sampleRate);

            int maxLag = searchStart;
            float maxValue = autocorr[searchStart];

            for (int lag = searchStart; lag < searchEnd; lag++)
            {
                if (autocorr[lag] > maxValue)
                {
                    maxValue = autocorr[lag];
                    maxLag = lag;
                }
            }

            return maxLag;
        });
    }

    private float[] ComputeAutocorrelation(float[] signal)
    {
        int n = Math.Min(1000, signal.Length);
        float[] result = new float[n];

        // Оптимизация: используем Span и избегаем bounds checking
        var signalSpan = signal.AsSpan();
        var resultSpan = result.AsSpan();

        for (int lag = 0; lag < n; lag++)
        {
            float sum = 0;
            for (int i = 0; i < n - lag; i++)
            {
                sum += signalSpan[i] * signalSpan[i + lag];
            }
            resultSpan[lag] = sum / (n - lag);
        }

        return result;
    }

    private async Task<List<Repetition>> FindRepetitionsAsync(float[] signal, int estimatedPeriod)
    {
        return await Task.Run(() =>
        {
            var peaks = new List<int>();
            int window = (int)(estimatedPeriod * 0.5);
            var signalSpan = signal.AsSpan();

            for (int i = window; i < signal.Length - window; i++)
            {
                bool isPeak = true;
                float currentValue = signalSpan[i];

                for (int j = i - window; j <= i + window; j++)
                {
                    if (j == i) continue;
                    if (signalSpan[j] >= currentValue)
                    {
                        isPeak = false;
                        break;
                    }
                }

                if (isPeak) peaks.Add(i);
            }

            return RefinePeaks(peaks, signal, estimatedPeriod);
        });
    }

    private List<Repetition> RefinePeaks(List<int> peaks, float[] signal, int estimatedPeriod)
    {
        if (peaks.Count < 2) return new List<Repetition>();

        var refined = new List<Repetition>();
        float minPeakHeight = peaks.Max(p => signal[p]) * 0.3f;
        var signalSpan = signal.AsSpan();

        for (int i = 0; i < peaks.Count; i++)
        {
            int currentPeak = peaks[i];
            if (signalSpan[currentPeak] < minPeakHeight) continue;

            int start = i > 0 ? (peaks[i - 1] + currentPeak) / 2 : 0;
            int end = i < peaks.Count - 1 ? (currentPeak + peaks[i + 1]) / 2 : signal.Length - 1;

            refined.Add(new Repetition
            {
                StartIndex = start,
                PeakIndex = currentPeak,
                EndIndex = end,
                PeakValue = signalSpan[currentPeak]
            });
        }

        return refined;
    }

    private List<ExerciseSet> GroupIntoSets(List<Repetition> reps, float[] signal)
    {
        var sets = new List<ExerciseSet>();
        if (reps.Count == 0) return sets;

        var currentSet = new ExerciseSet { StartIndex = reps[0].StartIndex };
        float restThreshold = 3.0f * _sampleRate;

        for (int i = 0; i < reps.Count; i++)
        {
            currentSet.Repetitions.Add(reps[i]);

            if (i < reps.Count - 1)
            {
                float interval = reps[i + 1].StartIndex - reps[i].EndIndex;
                if (interval > restThreshold)
                {
                    currentSet.EndIndex = reps[i].EndIndex;
                    sets.Add(currentSet);
                    currentSet = new ExerciseSet { StartIndex = reps[i + 1].StartIndex };
                }
            }
        }

        currentSet.EndIndex = reps.Last().EndIndex;
        sets.Add(currentSet);

        return sets;
    }

    private float CalculateAverageRestTime(List<ExerciseSet> sets, float[] signal, int sampleRate)
    {
        if (sets == null || sets.Count < 2 || signal == null || sampleRate <= 0)
            return 0;

        float totalRestTime = 0;
        int restPeriodsCount = 0;

        for (int i = 0; i < sets.Count - 1; i++)
        {
            int restStart = sets[i].EndIndex;
            int restEnd = sets[i + 1].StartIndex;
            float restDuration = (restEnd - restStart) / (float)sampleRate;

            totalRestTime += restDuration;
            restPeriodsCount++;
        }

        return restPeriodsCount > 0 ? totalRestTime / restPeriodsCount : 0;
    }
}

public class ExerciseAnalysis
{
    public List<Repetition> Repetitions { get; set; } = new();
    public List<ExerciseSet> Sets { get; set; } = new();
    public string MainAxis { get; set; }
    public float DominantPeriod { get; set; }
    public float AverageRestTime { get; set; }
}

public class Repetition
{
    public int StartIndex { get; set; }
    public int PeakIndex { get; set; }
    public int EndIndex { get; set; }
    public float PeakValue { get; set; }
}

public class ExerciseSet
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public List<Repetition> Repetitions { get; set; } = new();
}

public class AxisAnalysis
{
    public float[] Signal { get; set; }
    public int Period { get; set; }
    public string AxisName { get; set; }
}
