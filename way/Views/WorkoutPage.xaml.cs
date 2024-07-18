using way.ViewModels;

namespace way.Views;

public partial class WorkoutPage : ContentPage
{
	public WorkoutPage(WorkoutViewModel viewmodel)
	{
		InitializeComponent();

        BindingContext = viewmodel;
	}
}