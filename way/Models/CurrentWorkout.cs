using System.Text.Json;

namespace way.Models
{
    public class CurrentWorkout
    {
        public int TrainingId { get; set; }
        public DateTime TrainingDate { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string Sets { get; set; } = string.Empty;
        public int CountSets { get; set; }
        public int TimeRest { get; set; }

        public List<OperatingSet>? GetSets()
        {
            if (Sets == string.Empty)
                return null;
            return JsonSerializer.Deserialize<List<OperatingSet>>(Sets);
        }
        public void SetSets(List<OperatingSet> sets)
        {
            Sets = JsonSerializer.Serialize(sets);
        }

        public string RepsString
        {
            get
            {
                List<OperatingSet>? listsets = GetSets();
                if (listsets != null)
                {
                    List<string> stringsets = [];
                    for (int i = 0; i < CountSets; i++)
                    {
                        if (listsets[i].W > 0)
                            stringsets.Add($"{listsets[i].R} × {listsets[i].W} kg");
                        else
                            stringsets.Add($"{listsets[i].R}");
                    }
                    return string.Join(" - ", stringsets);
                }
                else
                    return string.Empty;
            }
        }
        public string TimeRestString
        {
            get
            {
                int Minute = TimeRest / 60;
                int Sec = TimeRest - Minute * 60;
                if (Minute > 0 && Sec > 0) return $"{Minute} мин {Sec} сек";
                else if (Minute == 0 && Sec > 0) return $"{Sec} сек";
                else if (Minute > 0 && Sec == 0) return $"{Minute} мин";
                else return string.Empty;
            }
        }
        public string ExerciseLabel
        {
            get
            {
                string ExerciseText = string.Empty;
                if (ExerciseName != string.Empty)
                {
                    if (TimeRest > 0)
                        return $"🦾 {ExerciseName}\n" +
                            $"Количество подходов: {CountSets}" +
                            $"\n\n{RepsString}\n\n" +
                            $"Отдых: {TimeRestString}";
                    else
                        return $"🦾 {ExerciseName}\n" +
                            $"Количество подходов: {CountSets}" +
                            $"\n\n{RepsString}";
                }
                return ExerciseText;
            }
        }
    }
}
