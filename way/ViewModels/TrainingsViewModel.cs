using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;
using way.Views;

namespace way.ViewModels
{
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
        private ObservableCollection<CurrentTraining> _trainings = [];

        [RelayCommand]
        public async Task LoadTrainingsAsync()
        {
            Trainings = [];
            await LoadTrainingsIncrementlyAsync();
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
        private async Task GoToTrainingAsync()
        {
            await Shell.Current.GoToAsync(nameof(TrainingPage), true);
        }

        [RelayCommand]
        private async Task GoToTrainingDetailsAsync(CurrentTraining training)
        {
            await Shell.Current.GoToAsync(nameof(TrainingDetailsPage), true,
                new Dictionary<string, object> { { "Training", training } });
        }
    }
}
