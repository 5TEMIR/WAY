using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using way.Models;

namespace way.ViewModels
{
    public partial class WorkoutViewModel : ObservableObject
    {
        private readonly IPopupService popupService;

        public WorkoutViewModel(IPopupService popupservice)
        {
            popupService = popupservice;

            OperatingSets.Add(new OperatingSet(0, 0));
        }

        [ObservableProperty]
        private CurrentWorkout? _operatingWorkout = null;

        [ObservableProperty]
        private string _operatingTitle = string.Empty;

        partial void OnOperatingTitleChanged(string value) { CheckWorkoutReady(); }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OperatingTimeRest))]
        private int _operatingTimeRestMin = 0;

        partial void OnOperatingTimeRestMinChanged(int value) { CheckWorkoutReady(); }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OperatingTimeRest))]
        private int _operatingTimeRestSec = 0;

        partial void OnOperatingTimeRestSecChanged(int value) { CheckWorkoutReady(); }

        [ObservableProperty]
        private ObservableCollection<OperatingSet> _operatingSets = [];

        [ObservableProperty]
        private bool _restIsVisible = false;

        [ObservableProperty]
        private bool _workoutIsReady = false;

        private void CheckWorkoutReady()
        {
            if (OperatingTitle.Length > 0)
            {
                if (OperatingSets.Count > 1 && (OperatingTimeRestMin > 0 || OperatingTimeRestSec > 0))
                    WorkoutIsReady = true;
                else if (OperatingSets.Count == 1)
                    WorkoutIsReady = true;
                else WorkoutIsReady = false;
            }
            else WorkoutIsReady = false;
        }

        public string OperatingTimeRest
        {
            get
            {
                if (OperatingTimeRestMin >= 10 && OperatingTimeRestSec >= 10)
                    return $"{OperatingTimeRestMin}:{OperatingTimeRestSec}";

                else if (OperatingTimeRestMin >= 10 && (0 < OperatingTimeRestSec && OperatingTimeRestSec < 10))
                    return $"{OperatingTimeRestMin}:0{OperatingTimeRestSec}";

                else if ((0 < OperatingTimeRestMin && OperatingTimeRestMin < 10) && OperatingTimeRestSec >= 10)
                    return $"0{OperatingTimeRestMin}:{OperatingTimeRestSec}";

                else if ((0 < OperatingTimeRestMin && OperatingTimeRestMin < 10) && (0 < OperatingTimeRestSec && OperatingTimeRestSec < 10))
                    return $"0{OperatingTimeRestMin}:0{OperatingTimeRestSec}";

                else if (OperatingTimeRestMin == 0 && OperatingTimeRestSec >= 10)
                    return $"00:{OperatingTimeRestSec}";

                else if (OperatingTimeRestMin == 0 && (0 < OperatingTimeRestSec && OperatingTimeRestSec < 10))
                    return $"00:0{OperatingTimeRestSec}";

                else if (OperatingTimeRestMin >= 10 && OperatingTimeRestSec == 0)
                    return $"{OperatingTimeRestMin}:00";

                else if ((0 < OperatingTimeRestMin && OperatingTimeRestMin < 10) && OperatingTimeRestSec == 0)
                    return $"0{OperatingTimeRestMin}:00";

                else return "00:00";
            }
        }

        [RelayCommand]
        private void AddSet()
        {
            if (OperatingSets.Count > 0)
            {
                OperatingSets.Add(new OperatingSet(OperatingSets.Last().R, OperatingSets.Last().W));
            }
            else
            {
                OperatingSets.Add(new OperatingSet(0, 0));
            }

            if (OperatingSets.Count > 1) { RestIsVisible = true; }

            CheckWorkoutReady();
        }

        [RelayCommand]
        private void DeleteSet(OperatingSet set)
        {
            OperatingSets.Remove(set);

            if (OperatingSets.Count <= 1)
            {
                RestIsVisible = false;
                OperatingTimeRestMin = 0;
                OperatingTimeRestSec = 0;
            }

            CheckWorkoutReady();
        }

        [RelayCommand]
        private async Task DisplayTimeRestPickerPopupAsync()
        {
            await popupService.ShowPopupAsync<TimeRestPickerViewModel>();
        }

        [RelayCommand]
        private async Task GoToTrainingAsync()
        {
            foreach (OperatingSet set in OperatingSets)
            {
                if (set.R == 0)
                {
                    await Shell.Current.DisplayAlert("Не указано количество повторений", "Укажите количество повторений.", "OK");
                    return;
                }
            }

            OperatingWorkout = new()
            {
                ExerciseName = OperatingTitle.ToLower(),
                CountSets = OperatingSets.Count,
                TimeRest = OperatingTimeRestMin * 60 + OperatingTimeRestSec
            };
            OperatingWorkout.SetSets([.. OperatingSets]);

            await Shell.Current.GoToAsync("..", true,
                new Dictionary<string, object> { { "Workout", OperatingWorkout } });
        }

        [RelayCommand]
        [UnsupportedOSPlatform("MacCatalyst")]
        private async Task OnShowKeyboard(ITextInput view, CancellationToken token)
        {
            try
            {
                await view.ShowKeyboardAsync(token);
            }
            catch (Exception) { }
        }
    }
}
