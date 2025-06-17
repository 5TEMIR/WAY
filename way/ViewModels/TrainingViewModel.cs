using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;
using way.Views;

namespace way.ViewModels
{
    [QueryProperty("OperatingWorkout", "Workout")]
    public partial class TrainingViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        private readonly TrainingsViewModel Trainings;

        public TrainingViewModel(DataBaseContext database, TrainingsViewModel trainings)
        {
            WorkoutDataBase = database;

            Trainings = trainings;
        }

        [ObservableProperty]
        private CurrentWorkout? _operatingWorkout = null;

        partial void OnOperatingWorkoutChanged(CurrentWorkout? oldValue, CurrentWorkout? newValue)
        {
            if (OperatingWorkout is not null && newValue != oldValue)
            {
                OperatingWorkouts.Add(OperatingWorkout);
                TrainingIsReady = true;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OperatingWeekday))]
        private DateTime _operatingDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<CurrentWorkout> _operatingWorkouts = [];

        [RelayCommand]
        private void DeleteWorkout(CurrentWorkout workout)
        {
            OperatingWorkouts.Remove(workout);

            if (OperatingWorkouts.Count == 0) TrainingIsReady = false;
        }

        [ObservableProperty]
        private bool _trainingIsReady = false;

        public string OperatingWeekday
        {
            get
            {
                string DayOfWeek = OperatingDate.DayOfWeek.ToString();
                if (DayOfWeek == "Monday")
                    return "ПОНЕДЕЛЬНИК";
                else if (DayOfWeek == "Tuesday")
                    return "ВТОРНИК";
                else if (DayOfWeek == "Wednesday")
                    return "СРЕДА";
                else if (DayOfWeek == "Thursday")
                    return "ЧЕТВЕРГ";
                else if (DayOfWeek == "Friday")
                    return "ПЯТНИЦА";
                else if (DayOfWeek == "Saturday")
                    return "СУББОТА";
                else
                    return "ВОСКРЕСЕНЬЕ";
            }
        }

        [RelayCommand]
        private async Task GoToWorkoutAsync()
        {
            await Shell.Current.GoToAsync(nameof(WorkoutPage), true);
        }

        [RelayCommand]
        private async Task GoToTrainingsAsync()
        {
            if (OperatingDate.Date == DateTime.Today)
                OperatingDate = DateTime.Now;

            int trainingid = await WorkoutDataBase.SaveTrainingAsync(new() { TrainingDate = OperatingDate });

            List<Workout> workouts = [];
            foreach (CurrentWorkout workout in OperatingWorkouts)
            {
                int exerciseid = await WorkoutDataBase.GetExerciseIdByNameAsync(workout.ExerciseName);
                workouts.Add(new()
                {
                    TrainingId = trainingid,
                    ExerciseId = exerciseid,
                    Sets = workout.Sets,
                    TimeRest = workout.TimeRest,
                    CountSets = workout.CountSets
                });
            }

            await WorkoutDataBase.SaveWorkoutsAsync(workouts);

            await Trainings.LoadTrainingsAsync();

            await Shell.Current.GoToAsync("..", true);
        }
    }
}
