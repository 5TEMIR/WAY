using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using SkiaSharp;
using System.Collections.ObjectModel;
using way.Data;
using way.Models;

namespace way.ViewModels
{
    [QueryProperty("OperatingExercise", "Exercise")]
    public partial class StatisticViewModel : ObservableObject
    {
        private readonly DataBaseContext WorkoutDataBase;

        public StatisticViewModel(DataBaseContext database)
        {
            WorkoutDataBase = database;
        }

        [ObservableProperty]
        Exercise? _operatingExercise = null;

        [RelayCommand]
        private async Task LoadExercises()
        {
            List<CurrentWorkout> exercises = await WorkoutDataBase.GetWorkoutsByExerciseIdAsync(OperatingExercise.Id);




            foreach (var exercise in exercises)
            {
                int maxreps = exercise.GetSets().Max<OperatingSet>(x => x.R);
                double maxweight = exercise.GetSets().Max<OperatingSet>(x => x.W);
                MaxRepsPoints.Add(new DateTimePoint(exercise.TrainingDate, maxreps));
                MaxWeightPoints.Add(new DateTimePoint(exercise.TrainingDate, maxweight));
            }

            Series = [MaxReps];
        }

        [ObservableProperty]
        public ObservableCollection<ISeries> _series;

        [RelayCommand]
        private void ChartMaxReps()
        {
            Series = [MaxReps];
        }

        [RelayCommand]
        private void ChartMaxWeight()
        {
            Series = [MaxWeight];
        }

        public List<DateTimePoint> MaxRepsPoints = [];

        public LineSeries<DateTimePoint> MaxReps
        {
            get
            {
                return new LineSeries<DateTimePoint>()
                {
                    Name = "Max Reps",
                    Values = MaxRepsPoints,
                    Fill = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(new(25, 118, 210)),
                    GeometryStroke = new SolidColorPaint(new(25, 118, 210))
                };
            }
        }

        public List<DateTimePoint> MaxWeightPoints = [];

        public LineSeries<DateTimePoint> MaxWeight
        {
            get
            {
                return new LineSeries<DateTimePoint>()
                {
                    Name = "Max Weight",
                    Values = MaxWeightPoints,
                    Fill = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(new(211, 33, 45)),
                    GeometryStroke = new SolidColorPaint(new(211, 33, 45))
                };
            }
        }

        public Axis[] XAxes { get; set; } =
        [
            new DateTimeAxis(TimeSpan.FromDays(3), date => date.ToString("dd.MM.yyyy")){ TextSize = 10}
        ];

        public DrawMarginFrame Frame { get; set; } = new()
        {
            Stroke = new SolidColorPaint
            {
                Color = new(195, 195, 195),
                StrokeThickness = 1
            }
        };

        public SolidColorPaint LegendTextPaint { get; set; } =
        new SolidColorPaint
        {
            Color = new SKColor(255, 255, 255),
        };
    }
}
