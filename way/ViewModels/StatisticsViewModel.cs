using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using way.Data;
using way.Models;
using way.Views;
using System.Collections.ObjectModel;

namespace way.ViewModels
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly DataBaseContext _database;

        [ObservableProperty]
        private int _totalTrainings;

        [ObservableProperty]
        private int _totalExercises;

        public ObservableCollection<Exercise> Exercises { get; } = new();

        public StatisticsViewModel(DataBaseContext database)
        {
            _database = database;
            LoadStatistics();
        }

        [RelayCommand]
        private async Task LoadStatistics()
        {
            var counts = await _database.GetCounts();
            TotalTrainings = counts[0];
            TotalExercises = counts[2];
            
            var exercises = await _database.GetExercisesAsync();
            Exercises.Clear();
            foreach (var exercise in exercises)
            {
                Exercises.Add(exercise);
            }
        }

        [RelayCommand]
        private async Task GoToExerciseStats(Exercise exercise)
        {
            if (exercise == null) return;
            
            await Shell.Current.GoToAsync(nameof(ExerciseStatsPage), true,
                new Dictionary<string, object> { { "Exercise", exercise } });
        }
    }
}
