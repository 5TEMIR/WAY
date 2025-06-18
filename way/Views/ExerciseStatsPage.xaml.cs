using way.ViewModels;

namespace way.Views;

public partial class ExerciseStatsPage : ContentPage
{
	public ExerciseStatsPage(ExerciseStatsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
