using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;
using way.Views;

namespace way.ViewModels
{
    [QueryProperty("OperatingTraining", "Training")]
    [QueryProperty("IsUpdateTrainings", "Update")]
    public partial class TrainingsViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        public TrainingsViewModel(DataBaseContext database)
        {
            WorkoutDataBase = database;
        }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isUpdateTrainings = false;

        //[ObservableProperty]
        //private Training? _operatingTraining = null;

        //async partial void OnOperatingTrainingChanged(Training? oldValue, Training? newValue)
        //{
        //    if (OperatingTraining is not null && oldValue != newValue)
        //    {
        //        await WorkoutDataBase.SaveTrainingAsync(OperatingTraining);
        //        OperatingTraining = null;
        //        Trainings = [];
        //        await LoadTrainingsIncrementlyAsync();
        //    }
        //}

        [ObservableProperty]
        private ObservableCollection<CurrentTraining> _trainings = [];

        [RelayCommand]
        private async Task LoadTrainingsAsync()
        {
            if (IsUpdateTrainings == false)
                return;
            Trainings = [];
            await LoadTrainingsIncrementlyAsync();
            IsUpdateTrainings = false;
        }

        [RelayCommand]
        private async Task LoadTrainingsIncrementlyAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            CurrentTraining training = new();
            List<CurrentWorkout> workouts = await WorkoutDataBase.GetTrainingsByCountAsync(Trainings.Count, 20);
            foreach (CurrentWorkout workout in workouts)
            {
                if (training.Workouts.Count == 0)
                {
                    training.Id = workout.TrainingId;
                    training.TrainingDate = workout.TrainingDate;
                    training.Workouts.Add(workout);
                }
                else
                {
                    if (workout.TrainingId == training.Id)
                    {
                        training.Workouts.Add(workout);
                    }
                    else
                    {
                        Trainings.Add(training);

                        training = new()
                        {
                            Id = workout.TrainingId,
                            TrainingDate = workout.TrainingDate
                        };
                        training.Workouts.Add(workout);
                    }
                }
            }
            if (workouts.Count > 0)
                Trainings.Add(training);

            IsBusy = false;
        }

        [RelayCommand]
        private async Task DeleteTrainingAsync(CurrentTraining training)
        {
            await WorkoutDataBase.DeleteWorkoutsByTrainingIdAsync(training.Id);
            await WorkoutDataBase.DeleteTrainingByIdAsync(training.Id);
            Trainings?.Remove(training);
        }

        [RelayCommand]
        private async Task CountsAsync()
        {
            List<int> counts = await WorkoutDataBase.GetCounts();
            await Shell.Current.DisplayAlert("Counts",string.Join(" - ",counts),"ok");
        }
        [RelayCommand]
        private async Task GoToTrainingAsync(Training? training)
        {
            if (training is null)
                await Shell.Current.GoToAsync(nameof(TrainingPage), true);
            else
                await Shell.Current.GoToAsync(nameof(TrainingPage), true,
                    new Dictionary<string, object> { { "Training", training } });
        }
    }
}
