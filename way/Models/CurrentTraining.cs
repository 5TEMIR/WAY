namespace way.Models
{
    public class CurrentTraining
    {
        public int Id { get; set; }
        public DateTime TrainingDate { get; set; }
        public List<CurrentWorkout> Workouts { get; set; } = [];

        public string DateString
        {
            get
            {
                string DayOfWeek = TrainingDate.DayOfWeek.ToString();
                if (DayOfWeek == "Monday")
                    return $"{TrainingDate:dd.MM.yyyy} ПОНЕДЕЛЬНИК";
                else if (DayOfWeek == "Tuesday")
                    return $"{TrainingDate:dd.MM.yyyy} ВТОРНИК";
                else if (DayOfWeek == "Wednesday")
                    return $"{TrainingDate:dd.MM.yyyy} СРЕДА";
                else if (DayOfWeek == "Thursday")
                    return $"{TrainingDate:dd.MM.yyyy} ЧЕТВЕРГ";
                else if (DayOfWeek == "Friday")
                    return $"{TrainingDate:dd.MM.yyyy} ПЯТНИЦА";
                else if (DayOfWeek == "Saturday")
                    return $"{TrainingDate:dd.MM.yyyy} СУББОТА";
                else
                    return $"{TrainingDate:dd.MM.yyyy} ВОСКРЕСЕНЬЕ";
            }
        }
        public string TrainingLabel
        {
            get
            {
                string TrainingText = string.Empty;

                if (TrainingDate != DateTime.MinValue)
                {
                    List<string> stringexercisenames = [];
                    TrainingText = $"⚡ {DateString}\n\n" + "\U0001f9be ";
                    foreach (CurrentWorkout workout in Workouts)
                    {
                        stringexercisenames.Add(workout.ExerciseName);
                    }
                    TrainingText += string.Join(", ", stringexercisenames);
                }

                return TrainingText;
            }
        }
        public string TrainingDetails
        {
            get
            {
                string TrainingText = string.Empty;

                if (TrainingDate != DateTime.MinValue)
                {
                    foreach (CurrentWorkout workout in Workouts)
                    {
                        TrainingText += $"{workout.WorkoutLabel}\n\n";
                    }
                    return TrainingText.Remove(TrainingText.Length - 2);
                }

                return TrainingText;
            }
        }
    }
}
