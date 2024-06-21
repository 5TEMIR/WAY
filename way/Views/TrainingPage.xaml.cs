using way.ViewModels;

namespace way.Views;

public partial class TrainingPage : ContentPage
{
	public TrainingPage(TrainingViewModel viewmodel)
	{
		InitializeComponent();
		BindingContext = viewmodel;
	}
}