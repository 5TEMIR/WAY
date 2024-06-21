using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;
using way.Views;

namespace way.ViewModels
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        public StatisticsViewModel(DataBaseContext database)
        {
            WorkoutDataBase = database;
        }

        [ObservableProperty]
        int _trainingsCount;

        [ObservableProperty]
        ObservableCollection<Exercise> _exercises = [];

        [RelayCommand]
        private async Task GetTrainingsCountAndExercisesAsync()
        {
            TrainingsCount = await WorkoutDataBase.GetTrainingsCountAsync();
            Exercises = [.. await WorkoutDataBase.GetExercisesAsync()];
        }

        [RelayCommand]
        private async Task GoToStatisticAsync(Exercise exercise)
        {
            await Shell.Current.GoToAsync(nameof(StatisticPage), true,
                    new Dictionary<string, object> { { "Exercise", exercise } });
        }
    }
}
