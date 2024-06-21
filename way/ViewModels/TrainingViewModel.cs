using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;
using way.Views;

namespace way.ViewModels
{
    [QueryProperty("OperatingExercise", "Exercise")]
    [QueryProperty("OperatingTraining", "Training")]
    public partial class TrainingViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        public TrainingViewModel(DataBaseContext database)
        {
            WorkoutDataBase = database;
        }

        [ObservableProperty]
        private CurrentWorkout? _operatingExercise = null;

        partial void OnOperatingExerciseChanged(CurrentWorkout? oldValue, CurrentWorkout? newValue)
        {
            if (OperatingExercise is not null && newValue != oldValue)
            {
                OperatingExercises.Add(OperatingExercise);
                TrainingIsReady = true;
            }
        }

        [ObservableProperty]
        private Training? _operatingTraining = null;

        //partial void OnOperatingTrainingChanged(Training? value)
        //{
        //    if (OperatingTraining is not null)
        //    {
        //        OperatingDate = OperatingTraining.Date;
        //        List<Exercise>? exercises = OperatingTraining.GetExercises();
        //        if (exercises is not null)
        //        {
        //            foreach (var exercise in exercises)
        //            {
        //                OperatingExercises?.Add(exercise);
        //            }
        //        }
        //    }
        //}

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OperatingWeekday))]
        private DateTime _operatingDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<CurrentWorkout> _operatingExercises = [];

        [RelayCommand]
        private void DeleteExercise(CurrentWorkout exercise)
        {
            OperatingExercises.Remove(exercise);

            if (OperatingExercises.Count == 0) TrainingIsReady = false;
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
        private async Task GoToExersiceAsync(Exercise? exercise)
        {
            if (exercise == null)
                await Shell.Current.GoToAsync(nameof(ExercisePage), true);
            else
                await Shell.Current.GoToAsync(nameof(ExercisePage), true,
                    new Dictionary<string, object> { { "Exercise", exercise } });
        }

        [RelayCommand]
        private async Task GoToTrainingsAsync()
        {
            if (OperatingDate.Date == DateTime.Today)
                OperatingDate = DateTime.Now;

            int trainingid = await WorkoutDataBase.SaveTrainingAsync(new() { TrainingDate = OperatingDate });
            
            List<Workout> workouts = [];
            foreach (CurrentWorkout workout in OperatingExercises)
            {
                int exerciseid = await WorkoutDataBase.GetExerciseIdByNameAsync(workout.ExerciseName);
                workouts.Add(new(){
                    TrainingId = trainingid,
                    ExerciseId = exerciseid,
                    Sets = workout.Sets,
                    TimeRest = workout.TimeRest,
                    CountSets = workout.CountSets
                });
            }

            await WorkoutDataBase.SaveWorkoutsAsync(workouts);

            await Shell.Current.GoToAsync($"..?Update={true}", true);
        }
    }
}
