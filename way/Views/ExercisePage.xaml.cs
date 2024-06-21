using way.ViewModels;

namespace way.Views;

public partial class ExercisePage : ContentPage
{
	public ExercisePage(ExerciseViewModel viewmodel)
	{
		InitializeComponent();

        BindingContext = viewmodel;
	}
}