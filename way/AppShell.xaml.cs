using way.Views;

namespace way
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(TrainingsPage),typeof(TrainingsPage));
            Routing.RegisterRoute(nameof(TrainingPage),typeof(TrainingPage));
            Routing.RegisterRoute(nameof(WorkoutPage), typeof(WorkoutPage));
            Routing.RegisterRoute(nameof(TrainingDetailsPage), typeof(TrainingDetailsPage));
        }
    }
}
