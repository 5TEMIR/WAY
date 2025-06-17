using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using way.Data;
using way.Models;

namespace way.ViewModels
{
    [QueryProperty("OperationTraining", "Training")]
    public partial class TrainingDetailsViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        private readonly TrainingsViewModel Trainings;

        public TrainingDetailsViewModel(DataBaseContext database, TrainingsViewModel trainings)
        {
            WorkoutDataBase = database;

            Trainings = trainings;
        }

        [ObservableProperty]
        private CurrentTraining _operationTraining;

        [RelayCommand]
        private async Task DeleteTrainingAsync()
        {
            await WorkoutDataBase.DeleteWorkoutsByTrainingIdAsync(OperationTraining.Id);
            await WorkoutDataBase.DeleteTrainingByIdAsync(OperationTraining.Id);

            await Trainings.LoadTrainingsAsync();

            await Shell.Current.GoToAsync("..", true);
        }
    }
}
