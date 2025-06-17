using way.ViewModels;

namespace way.Views;

public partial class TrainingDetailsPage : ContentPage
{
	public TrainingDetailsPage(TrainingDetailsViewModel viewmodel)
	{
		InitializeComponent();

        BindingContext = viewmodel;
    }
}